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

package org.openqa.selenium.grid.router.httpd;

import static org.assertj.core.api.Assertions.assertThat;
import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertThrows;
import static org.junit.jupiter.api.Assertions.assertTrue;

import com.google.common.collect.ImmutableList;
import com.google.common.collect.ImmutableMap;
import java.io.StringReader;
import java.util.List;
import org.junit.jupiter.api.Test;
import org.openqa.selenium.grid.config.Config;
import org.openqa.selenium.grid.config.MapConfig;
import org.openqa.selenium.grid.config.TomlConfig;

class RouterOptionsTest {

  @Test
  void shouldReturnEmptyListWhenNoBlockedRoutesConfigured() {
    Config config = new MapConfig(ImmutableMap.of());
    RouterOptions options = new RouterOptions(config);

    List<BlockedRoute> blockedRoutes = options.getBlockedRoutes();
    assertThat(blockedRoutes).isEmpty();
  }

  @Test
  void shouldParseSingleBlockedRoute() {
    Config config =
        new MapConfig(
            ImmutableMap.of(
                "router",
                ImmutableMap.of(
                    "blocked-routes", ImmutableList.of("DELETE:/session/{session-id}"))));
    RouterOptions options = new RouterOptions(config);

    List<BlockedRoute> blockedRoutes = options.getBlockedRoutes();
    assertThat(blockedRoutes).hasSize(1);

    BlockedRoute route = blockedRoutes.get(0);
    assertEquals("DELETE", route.getMethod());
    assertEquals("/session/{session-id}", route.getPath());
  }

  @Test
  void shouldParseMultipleBlockedRoutes() {
    Config config =
        new MapConfig(
            ImmutableMap.of(
                "router",
                ImmutableMap.of(
                    "blocked-routes",
                    ImmutableList.of(
                        "DELETE:/session/{session-id}", "POST:/session", "GET:/status"))));
    RouterOptions options = new RouterOptions(config);

    List<BlockedRoute> blockedRoutes = options.getBlockedRoutes();
    assertThat(blockedRoutes).hasSize(3);

    assertThat(blockedRoutes)
        .anyMatch(
            route ->
                "DELETE".equals(route.getMethod())
                    && "/session/{session-id}".equals(route.getPath()));
    assertThat(blockedRoutes)
        .anyMatch(route -> "POST".equals(route.getMethod()) && "/session".equals(route.getPath()));
    assertThat(blockedRoutes)
        .anyMatch(route -> "GET".equals(route.getMethod()) && "/status".equals(route.getPath()));
  }

  @Test
  void shouldHandleWhitespaceInBlockedRoutes() {
    Config config =
        new MapConfig(
            ImmutableMap.of(
                "router",
                ImmutableMap.of(
                    "blocked-routes",
                    ImmutableList.of(" DELETE : /session/{session-id} ", " POST : /session "))));
    RouterOptions options = new RouterOptions(config);

    List<BlockedRoute> blockedRoutes = options.getBlockedRoutes();
    assertThat(blockedRoutes).hasSize(2);

    assertThat(blockedRoutes)
        .anyMatch(
            route ->
                "DELETE".equals(route.getMethod())
                    && "/session/{session-id}".equals(route.getPath()));
    assertThat(blockedRoutes)
        .anyMatch(route -> "POST".equals(route.getMethod()) && "/session".equals(route.getPath()));
  }

  @Test
  void shouldIgnoreEmptyEntriesInBlockedRoutes() {
    Config config =
        new MapConfig(
            ImmutableMap.of(
                "router",
                ImmutableMap.of(
                    "blocked-routes",
                    ImmutableList.of("DELETE:/session/{session-id}", "", "POST:/session"))));
    RouterOptions options = new RouterOptions(config);

    List<BlockedRoute> blockedRoutes = options.getBlockedRoutes();
    assertThat(blockedRoutes).hasSize(2);
  }

  @Test
  void shouldHandleNullBlockedRoutes() {
    // Test that when blocked-routes is not present in config, it returns empty list
    Config config = new MapConfig(ImmutableMap.of("router", ImmutableMap.of()));
    RouterOptions options = new RouterOptions(config);

    List<BlockedRoute> blockedRoutes = options.getBlockedRoutes();
    assertThat(blockedRoutes).isEmpty();
  }

  @Test
  void shouldHandleEmptyBlockedRoutes() {
    Config config =
        new MapConfig(
            ImmutableMap.of("router", ImmutableMap.of("blocked-routes", ImmutableList.of())));
    RouterOptions options = new RouterOptions(config);

    List<BlockedRoute> blockedRoutes = options.getBlockedRoutes();
    assertThat(blockedRoutes).isEmpty();
  }

  @Test
  void shouldParseBlockedRoutesFromTomlConfig() {
    String[] rawConfig = {
      "[router]",
      "blocked-routes = [",
      "  \"DELETE:/session/{session-id}\",",
      "  \"POST:/session\",",
      "  \"GET:/status\"",
      "]"
    };
    Config config = new TomlConfig(new StringReader(String.join("\n", rawConfig)));
    RouterOptions options = new RouterOptions(config);

    List<BlockedRoute> blockedRoutes = options.getBlockedRoutes();
    assertThat(blockedRoutes).hasSize(3);

    assertThat(blockedRoutes)
        .anyMatch(
            route ->
                "DELETE".equals(route.getMethod())
                    && "/session/{session-id}".equals(route.getPath()));
    assertThat(blockedRoutes)
        .anyMatch(route -> "POST".equals(route.getMethod()) && "/session".equals(route.getPath()));
    assertThat(blockedRoutes)
        .anyMatch(route -> "GET".equals(route.getMethod()) && "/status".equals(route.getPath()));
  }

  @Test
  void shouldParseBlockedRoutesFromTomlConfigWithWhitespace() {
    String[] rawConfig = {
      "[router]",
      "blocked-routes = [",
      "  \" DELETE : /session/{session-id} \",",
      "  \" POST : /session \"",
      "]"
    };
    Config config = new TomlConfig(new StringReader(String.join("\n", rawConfig)));
    RouterOptions options = new RouterOptions(config);

    List<BlockedRoute> blockedRoutes = options.getBlockedRoutes();
    assertThat(blockedRoutes).hasSize(2);

    assertThat(blockedRoutes)
        .anyMatch(
            route ->
                "DELETE".equals(route.getMethod())
                    && "/session/{session-id}".equals(route.getPath()));
    assertThat(blockedRoutes)
        .anyMatch(route -> "POST".equals(route.getMethod()) && "/session".equals(route.getPath()));
  }

  @Test
  void shouldParseBlockedRoutesFromTomlConfigWithEmptyEntries() {
    String[] rawConfig = {
      "[router]",
      "blocked-routes = [",
      "  \"DELETE:/session/{session-id}\",",
      "  \"\",",
      "  \"POST:/session\"",
      "]"
    };
    Config config = new TomlConfig(new StringReader(String.join("\n", rawConfig)));
    RouterOptions options = new RouterOptions(config);

    List<BlockedRoute> blockedRoutes = options.getBlockedRoutes();
    assertThat(blockedRoutes).hasSize(2);
  }

  @Test
  void shouldParseBlockedRoutesFromTomlConfigWithDeleteSessionFlag() {
    String[] rawConfig = {
      "[router]",
      "blocked-routes = [",
      "  \"POST:/session\",",
      "  \"GET:/status\"",
      "]",
      "blocked-delete-session = true"
    };
    Config config = new TomlConfig(new StringReader(String.join("\n", rawConfig)));
    RouterOptions options = new RouterOptions(config);

    List<BlockedRoute> blockedRoutes = options.getBlockedRoutes();
    assertThat(blockedRoutes).hasSize(3);

    assertThat(blockedRoutes)
        .anyMatch(
            route ->
                "DELETE".equals(route.getMethod())
                    && "/session/{session-id}".equals(route.getPath()));
    assertThat(blockedRoutes)
        .anyMatch(route -> "POST".equals(route.getMethod()) && "/session".equals(route.getPath()));
    assertThat(blockedRoutes)
        .anyMatch(route -> "GET".equals(route.getMethod()) && "/status".equals(route.getPath()));
  }

  @Test
  void shouldParseBlockedRoutesFromTomlConfigWithSingleRoute() {
    String[] rawConfig = {"[router]", "blocked-routes = [\"DELETE:/session/{session-id}\"]"};
    Config config = new TomlConfig(new StringReader(String.join("\n", rawConfig)));
    RouterOptions options = new RouterOptions(config);

    List<BlockedRoute> blockedRoutes = options.getBlockedRoutes();
    assertThat(blockedRoutes).hasSize(1);

    BlockedRoute route = blockedRoutes.get(0);
    assertEquals("DELETE", route.getMethod());
    assertEquals("/session/{session-id}", route.getPath());
  }

  @Test
  void shouldParseEmptyBlockedRoutesFromTomlConfig() {
    String[] rawConfig = {"[router]", "blocked-routes = []"};
    Config config = new TomlConfig(new StringReader(String.join("\n", rawConfig)));
    RouterOptions options = new RouterOptions(config);

    List<BlockedRoute> blockedRoutes = options.getBlockedRoutes();
    assertThat(blockedRoutes).isEmpty();
  }

  @Test
  void shouldParseBlockedRoutesFromTomlConfigWithoutRouterSection() {
    String[] rawConfig = {"[other]", "some-option = \"value\""};
    Config config = new TomlConfig(new StringReader(String.join("\n", rawConfig)));
    RouterOptions options = new RouterOptions(config);

    List<BlockedRoute> blockedRoutes = options.getBlockedRoutes();
    assertThat(blockedRoutes).isEmpty();
  }

  @Test
  void blockedRouteShouldMatchExactPath() {
    BlockedRoute route = BlockedRoute.fromString("DELETE:/session/{session-id}");

    assertTrue(route.matches("DELETE", "/session/123"));
    assertTrue(route.matches("DELETE", "/session/abc-def"));
    assertFalse(route.matches("DELETE", "/session"));
    assertFalse(route.matches("DELETE", "/session/123/extra"));
    assertFalse(route.matches("POST", "/session/123"));
  }

  @Test
  void blockedRouteShouldMatchExactPathWithoutParameters() {
    BlockedRoute route = BlockedRoute.fromString("POST:/session");

    assertTrue(route.matches("POST", "/session"));
    assertFalse(route.matches("POST", "/session/123"));
    assertFalse(route.matches("GET", "/session"));
  }

  @Test
  void blockedRouteShouldBeCaseInsensitiveForMethod() {
    BlockedRoute route = BlockedRoute.fromString("DELETE:/session/{session-id}");

    assertTrue(route.matches("delete", "/session/123"));
    assertTrue(route.matches("Delete", "/session/123"));
    assertTrue(route.matches("DELETE", "/session/123"));
  }

  @Test
  void shouldThrowExceptionForInvalidRouteFormat() {
    assertThrows(IllegalArgumentException.class, () -> BlockedRoute.fromString("DELETE"));

    assertThrows(IllegalArgumentException.class, () -> BlockedRoute.fromString(":path"));

    assertThrows(IllegalArgumentException.class, () -> BlockedRoute.fromString("method:"));

    assertThrows(IllegalArgumentException.class, () -> BlockedRoute.fromString(""));

    assertThrows(IllegalArgumentException.class, () -> BlockedRoute.fromString(null));
  }

  @Test
  void blockedRouteToStringShouldReturnOriginalFormat() {
    BlockedRoute route = new BlockedRoute("DELETE", "/session/{session-id}");
    assertEquals("DELETE:/session/{session-id}", route.toString());
  }

  @Test
  void blockedRouteShouldHandleMultiplePathParameters() {
    BlockedRoute route = BlockedRoute.fromString("PUT:/session/{session-id}/element/{element-id}");

    assertTrue(route.matches("PUT", "/session/123/element/456"));
    assertTrue(route.matches("PUT", "/session/abc/element/def"));
    assertFalse(route.matches("PUT", "/session/123/element"));
    assertFalse(route.matches("PUT", "/session/element/456"));
  }

  @Test
  void shouldAddDeleteSessionRouteWhenFlagIsEnabled() {
    Config config =
        new MapConfig(ImmutableMap.of("router", ImmutableMap.of("blocked-delete-session", "true")));
    RouterOptions options = new RouterOptions(config);

    List<BlockedRoute> blockedRoutes = options.getBlockedRoutes();
    assertThat(blockedRoutes).hasSize(1);

    BlockedRoute route = blockedRoutes.get(0);
    assertEquals("DELETE", route.getMethod());
    assertEquals("/session/{session-id}", route.getPath());
  }

  @Test
  void shouldNotAddDuplicateDeleteSessionRoute() {
    Config config =
        new MapConfig(
            ImmutableMap.of(
                "router",
                ImmutableMap.of(
                    "blocked-routes",
                    ImmutableList.of("DELETE:/session/{session-id}"),
                    "blocked-delete-session",
                    "true")));
    RouterOptions options = new RouterOptions(config);

    List<BlockedRoute> blockedRoutes = options.getBlockedRoutes();
    assertThat(blockedRoutes).hasSize(1);

    BlockedRoute route = blockedRoutes.get(0);
    assertEquals("DELETE", route.getMethod());
    assertEquals("/session/{session-id}", route.getPath());
  }

  @Test
  void shouldCombineBlockedRoutesWithDeleteSessionFlag() {
    Config config =
        new MapConfig(
            ImmutableMap.of(
                "router",
                ImmutableMap.of(
                    "blocked-routes",
                    ImmutableList.of("POST:/session", "GET:/status"),
                    "blocked-delete-session",
                    "true")));
    RouterOptions options = new RouterOptions(config);

    List<BlockedRoute> blockedRoutes = options.getBlockedRoutes();
    assertThat(blockedRoutes).hasSize(3);

    assertThat(blockedRoutes)
        .anyMatch(
            route ->
                "DELETE".equals(route.getMethod())
                    && "/session/{session-id}".equals(route.getPath()));
    assertThat(blockedRoutes)
        .anyMatch(route -> "POST".equals(route.getMethod()) && "/session".equals(route.getPath()));
    assertThat(blockedRoutes)
        .anyMatch(route -> "GET".equals(route.getMethod()) && "/status".equals(route.getPath()));
  }

  @Test
  void shouldNotAddDeleteSessionRouteWhenFlagIsDisabled() {
    Config config =
        new MapConfig(
            ImmutableMap.of("router", ImmutableMap.of("blocked-delete-session", "false")));
    RouterOptions options = new RouterOptions(config);

    List<BlockedRoute> blockedRoutes = options.getBlockedRoutes();
    assertThat(blockedRoutes).isEmpty();
  }

  @Test
  void shouldNotAddDeleteSessionRouteWhenFlagIsNotSet() {
    Config config = new MapConfig(ImmutableMap.of());
    RouterOptions options = new RouterOptions(config);

    List<BlockedRoute> blockedRoutes = options.getBlockedRoutes();
    assertThat(blockedRoutes).isEmpty();
  }
}
