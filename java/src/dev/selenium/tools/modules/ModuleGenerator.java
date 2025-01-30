// Licensed to the Software Freedom Conservancy (SFC) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The SFC licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.

package dev.selenium.tools.modules;

import com.github.javaparser.*;
import com.github.javaparser.ast.CompilationUnit;
import com.github.javaparser.ast.expr.Name;
import com.github.javaparser.ast.modules.*;
import net.bytebuddy.jar.asm.*;
import org.openqa.selenium.io.TemporaryFilesystem;

import java.io.*;
import java.net.*;
import java.nio.file.*;
import java.nio.file.attribute.BasicFileAttributes;
import java.util.*;
import java.util.concurrent.atomic.AtomicReference;
import java.util.jar.*;
import java.util.spi.ToolProvider;
import java.util.stream.Collectors;
import java.util.stream.Stream;
import java.util.zip.ZipEntry;
import java.util.zip.ZipOutputStream;

public class ModuleGenerator {

    private static final String SERVICE_LOADER = ServiceLoader.class.getName().replace('.', '/');

    public static void main(String[] args) throws IOException {
        Map<String, Set<String>> options = Map.ofEntries(
                Map.entry("--exports", new TreeSet<>()),
                Map.entry("--hides", new TreeSet<>()),
                Map.entry("--uses", new TreeSet<>()),
                Map.entry("--open-to", new TreeSet<>())
        );

        Path outJar = null, inJar = null;
        String moduleName = null;
        boolean isOpen = false;
        Set<Path> modulePath = new TreeSet<>();
        Map<String, Set<String>> opensTo = new TreeMap<>();

        for (int i = 0; i < args.length; i++) {
            String flag = args[i];
            String next = args[++i];

            switch (flag) {
                case "--module-name" -> moduleName = next;
                case "--output" -> outJar = Paths.get(next);
                case "--in" -> inJar = Paths.get(next);
                case "--module-path" -> modulePath.add(Paths.get(next));
                case "--is-open" -> isOpen = Boolean.parseBoolean(next);
                case "--opens-to" -> opensTo.computeIfAbsent(next, k -> new TreeSet<>()).add(args[++i]);
                default -> options.getOrDefault(flag, new TreeSet<>()).add(next);
            }
        }

        validateInputs(moduleName, outJar, inJar);

        Path temp = createTempDir();
        List<String> jdepsArgs = prepareJdepsArgs(modulePath, inJar, temp);
        executeJdeps(jdepsArgs);

        Path moduleInfo = findModuleInfo(temp);
        CompilationUnit unit = parseModuleInfo(moduleInfo);
        ModuleDeclaration moduleDeclaration = configureModule(unit, moduleName, isOpen, options.get("--uses"), inJar);

        writeOptimizedJar(moduleDeclaration, outJar, inJar);
    }

    private static void validateInputs(String moduleName, Path outJar, Path inJar) {
        Objects.requireNonNull(moduleName, "Module name must be set.");
        Objects.requireNonNull(outJar, "Output jar must be set.");
        Objects.requireNonNull(inJar, "Input jar must be set.");
    }

    private static Path createTempDir() {
        return TemporaryFilesystem.getDefaultTmpFS().createTempDir("module-dir", "").toPath();
    }

    private static List<String> prepareJdepsArgs(Set<Path> modulePath, Path inJar, Path temp) throws IOException {
        List<String> jdepsArgs = new LinkedList<>(List.of("--api-only", "--multi-release", "9"));
        if (!modulePath.isEmpty()) {
            Path tmp = Files.createTempDirectory("automatic_module_jars");
            String modulePathStr = modulePath.stream()
                    .map(path -> processPathForJdeps(path, tmp))
                    .collect(Collectors.joining(File.pathSeparator));

            jdepsArgs.addAll(List.of("--module-path", modulePathStr));
        }
        jdepsArgs.addAll(List.of("--generate-module-info", temp.toAbsolutePath().toString(), inJar.toAbsolutePath().toString()));
        return jdepsArgs;
    }

    private static String processPathForJdeps(Path path, Path tmp) {
        String file = path.getFileName().toString();
        if (file.startsWith("processed_")) {
            Path copy = tmp.resolve(file.substring(10));
            try {
                Files.copy(path, copy, StandardCopyOption.REPLACE_EXISTING);
            } catch (IOException e) {
                throw new UncheckedIOException(e);
            }
            return copy.toString();
        }
        return path.toString();
    }

    private static void executeJdeps(List<String> jdepsArgs) throws IOException {
        ToolProvider jdeps = ToolProvider.findFirst("jdeps").orElseThrow();
        ByteArrayOutputStream bos = new ByteArrayOutputStream();

        try (PrintStream printStream = new PrintStream(bos)) {
            int result = jdeps.run(printStream, printStream, jdepsArgs.toArray(new String[0]));
            if (result != 0) {
                throw new RuntimeException("jdeps failed: " + new String(bos.toByteArray()));
            }
        }
    }

    private static Path findModuleInfo(Path temp) throws IOException {
        AtomicReference<Path> moduleInfo = new AtomicReference<>();
        Files.walkFileTree(temp, new SimpleFileVisitor<>() {
            @Override
            public FileVisitResult visitFile(Path file, BasicFileAttributes attrs) {
                if ("module-info.java".equals(file.getFileName().toString())) {
                    moduleInfo.set(file);
                }
                return FileVisitResult.TERMINATE;
            }
        });

        return Optional.ofNullable(moduleInfo.get()).orElseThrow(() -> new RuntimeException("Unable to read module info"));
    }

    private static CompilationUnit parseModuleInfo(Path moduleInfo) throws IOException {
        ParserConfiguration config = new ParserConfiguration().setLanguageLevel(ParserConfiguration.LanguageLevel.JAVA_11);
        ParseResult<CompilationUnit> parseResult = new JavaParser(config).parse(ParseStart.COMPILATION_UNIT, Providers.provider(moduleInfo));

        return parseResult.getResult().orElseThrow(() -> new RuntimeException("Failed to parse module-info.java"));
    }

    private static ModuleDeclaration configureModule(CompilationUnit unit, String moduleName, boolean isOpen, Set<String> uses, Path inJar) throws IOException {
        ModuleDeclaration moduleDeclaration = unit.getModule()
                .orElseThrow(() -> new RuntimeException("No module declaration in module-info.java"));

        moduleDeclaration.setName(moduleName);
        moduleDeclaration.setOpen(isOpen);

        Set<String> allUses = new TreeSet<>(uses);
        allUses.addAll(readServicesFromClasses(inJar));

        allUses.forEach(service -> moduleDeclaration.addDirective(new ModuleUsesDirective(new Name(service))));

        return moduleDeclaration;
    }

    private static void writeOptimizedJar(ModuleDeclaration moduleDeclaration, Path outJar, Path inJar) throws IOException {
        try (JarOutputStream jarOut = new JarOutputStream(Files.newOutputStream(outJar))) {
            JarEntry entry = new JarEntry("module-info.class");
            jarOut.putNextEntry(entry);
            jarOut.write(moduleDeclaration.toString().getBytes());
            jarOut.closeEntry();

            try (JarInputStream jarIn = new JarInputStream(Files.newInputStream(inJar))) {
                JarEntry jarEntry;
                while ((jarEntry = jarIn.getNextJarEntry()) != null) {
                    if (!"module-info.class".equals(jarEntry.getName())) {
                        jarOut.putNextEntry(new ZipEntry(jarEntry.getName()));
                        jarOut.write(jarIn.readAllBytes());
                        jarOut.closeEntry();
                    }
                }
            }
        }
    }

    private static Set<String> readServicesFromClasses(Path inJar) {
        return new TreeSet<>(); // Simulated service reading logic.
    }
}
