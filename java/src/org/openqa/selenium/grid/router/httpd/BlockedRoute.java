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

import java.net.URLDecoder;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.List;

/** Represents a blocked route with HTTP method and path. */
public class BlockedRoute {
  private final String method;
  private final String path;

  public BlockedRoute(String method, String path) {
    this.method = method.toUpperCase();
    this.path = path;
  }

  public String getMethod() {
    return method;
  }

  public String getPath() {
    return path;
  }

  /**
   * Creates a BlockedRoute from a string in the format "METHOD:path".
   *
   * @param routeStr String representation of blocked route
   * @return BlockedRoute instance
   * @throws IllegalArgumentException if the format is invalid
   */
  public static BlockedRoute fromString(String routeStr) {
    if (routeStr == null || routeStr.trim().isEmpty()) {
      throw new IllegalArgumentException("Route string cannot be null or empty");
    }

    String[] parts = routeStr.split(":", 2);
    if (parts.length != 2) {
      throw new IllegalArgumentException(
          "Invalid route format. Expected 'METHOD:path', got: " + routeStr);
    }

    String method = parts[0].trim().toUpperCase();
    String path = parts[1].trim();

    if (method.isEmpty() || path.isEmpty()) {
      throw new IllegalArgumentException("Method and path cannot be empty. Got: " + routeStr);
    }

    return new BlockedRoute(method, path);
  }

  /**
   * Checks if the given HTTP method and request path match this blocked route.
   *
   * @param requestMethod HTTP method of the request
   * @param requestPath Path of the request
   * @return true if the route should be blocked
   */
  public boolean matches(String requestMethod, String requestPath) {
    if (!method.equals(requestMethod.toUpperCase())) {
      return false;
    }

    // Use safe string-based path matching instead of regex to prevent ReDoS attacks
    return matchesPathPattern(path, requestPath);
  }

  /**
   * Safely matches a path pattern against a request path without using regex. Handles path
   * parameters like {session-id} by treating them as wildcards. Both paths are normalized to
   * prevent path traversal attacks.
   *
   * @param pattern The path pattern to match against
   * @param requestPath The actual request path
   * @return true if the paths match
   */
  private boolean matchesPathPattern(String pattern, String requestPath) {
    // Normalize both paths to prevent path traversal attacks
    String normalizedPattern = normalizePath(pattern);
    String normalizedRequestPath = normalizePath(requestPath);

    // Split both paths into segments
    String[] patternSegments = normalizedPattern.split("/", -1); // keep trailing empty segments
    String[] requestSegments = normalizedRequestPath.split("/", -1);

    // Paths must have the same number of segments
    if (patternSegments.length != requestSegments.length) {
      return false;
    }

    // Compare each segment
    for (int i = 0; i < patternSegments.length; i++) {
      String patternSegment = patternSegments[i];
      String requestSegment = requestSegments[i];

      // If both are empty (leading/trailing slash), continue
      if (patternSegment.isEmpty() && requestSegment.isEmpty()) {
        continue;
      }
      // If pattern segment is a path parameter (enclosed in {}), it matches any non-empty segment
      if (isPathParameter(patternSegment)) {
        if (requestSegment.isEmpty()) {
          return false;
        }
      } else {
        // For literal segments, they must match exactly
        if (!patternSegment.equals(requestSegment)) {
          return false;
        }
      }
    }

    return true;
  }

  /**
   * Normalizes a path to prevent path traversal attacks. This method: 1. URL decodes
   * percent-encoded characters 2. Normalizes multiple consecutive slashes to single slashes 3.
   * Resolves path traversal sequences (../) 4. Ensures the path doesn't escape the root directory
   *
   * @param path The path to normalize
   * @return The normalized path
   * @throws IllegalArgumentException if the path contains invalid traversal sequences
   */
  private String normalizePath(String path) {
    if (path == null || path.isEmpty()) {
      return "/";
    }

    try {
      // URL decode the path to handle percent-encoded characters like %2F
      String decodedPath = URLDecoder.decode(path, StandardCharsets.UTF_8);

      // Normalize multiple consecutive slashes to single slashes
      String normalizedPath = decodedPath.replaceAll("/+", "/");

      // Split into segments and resolve path traversal
      String[] segments = normalizedPath.split("/");
      List<String> resolvedSegments = new ArrayList<>();

      for (String segment : segments) {
        if (segment.isEmpty() || ".".equals(segment)) {
          // Skip empty segments and current directory references
          continue;
        } else if ("..".equals(segment)) {
          // Go up one directory level
          if (!resolvedSegments.isEmpty()) {
            resolvedSegments.remove(resolvedSegments.size() - 1);
          } else {
            // Attempting to go above root - this is a security violation
            throw new IllegalArgumentException("Path traversal attack detected: " + path);
          }
        } else {
          // Add normal segment
          resolvedSegments.add(segment);
        }
      }

      // Reconstruct the path
      StringBuilder result = new StringBuilder();
      for (String segment : resolvedSegments) {
        result.append("/").append(segment);
      }

      // Ensure the result starts with / and handle empty path case
      String finalPath = result.toString();
      return finalPath.isEmpty() ? "/" : finalPath;

    } catch (Exception e) {
      // If URL decoding fails or any other error occurs, throw security exception
      throw new IllegalArgumentException("Invalid path format: " + path, e);
    }
  }

  /**
   * Checks if a path segment is a path parameter (enclosed in curly braces).
   *
   * @param segment The path segment to check
   * @return true if it's a path parameter
   */
  private boolean isPathParameter(String segment) {
    return segment.startsWith("{") && segment.endsWith("}") && segment.length() > 2;
  }

  @Override
  public String toString() {
    return method + ":" + path;
  }

  @Override
  public boolean equals(Object obj) {
    if (this == obj) return true;
    if (obj == null || getClass() != obj.getClass()) return false;
    BlockedRoute that = (BlockedRoute) obj;
    return method.equals(that.method) && path.equals(that.path);
  }

  @Override
  public int hashCode() {
    return method.hashCode() * 31 + path.hashCode();
  }
}
