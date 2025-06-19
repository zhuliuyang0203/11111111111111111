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

import static org.junit.jupiter.api.Assertions.*;

import org.junit.jupiter.api.Test;

class BlockedRouteTest {

  @Test
  void matchesExactPathAndMethod() {
    BlockedRoute route = new BlockedRoute("GET", "/status");
    assertTrue(route.matches("GET", "/status"));
    assertFalse(route.matches("POST", "/status"));
    assertFalse(route.matches("GET", "/not-status"));
  }

  @Test
  void matchesPathWithParameter() {
    BlockedRoute route = new BlockedRoute("DELETE", "/session/{session-id}");
    assertTrue(route.matches("DELETE", "/session/123"));
    assertTrue(route.matches("DELETE", "/session/abc"));
    assertFalse(route.matches("DELETE", "/session/"));
    assertFalse(route.matches("DELETE", "/session"));
    assertFalse(route.matches("DELETE", "/session/123/extra"));
  }

  @Test
  void methodMatchingIsCaseInsensitive() {
    BlockedRoute route = new BlockedRoute("delete", "/session/{session-id}");
    assertTrue(route.matches("DELETE", "/session/123"));
    assertTrue(route.matches("delete", "/session/123"));
  }

  @Test
  void doesNotMatchIfSegmentCountDiffers() {
    BlockedRoute route = new BlockedRoute("GET", "/foo/{bar}");
    assertFalse(route.matches("GET", "/foo"));
    assertFalse(route.matches("GET", "/foo/bar/baz"));
  }

  @Test
  void handlesLeadingAndTrailingSlashes() {
    BlockedRoute route = new BlockedRoute("GET", "/foo/{bar}/");
    assertTrue(route.matches("GET", "/foo/123/"));
    assertTrue(route.matches("GET", "/foo/123"));
    assertFalse(route.matches("GET", "/foo//"));
  }

  @Test
  void doesNotReDoSOnLongInput() {
    BlockedRoute route = new BlockedRoute("GET", "/foo/{bar}");
    String longPath = "/foo/" + "a".repeat(10000);
    assertTrue(route.matches("GET", longPath));
  }

  @Test
  void preventsPathTraversalWithDoubleSlashes() {
    BlockedRoute route = new BlockedRoute("GET", "/admin/users");
    // These should all be normalized to /admin/users and match
    assertTrue(route.matches("GET", "/admin//users"));
    assertTrue(route.matches("GET", "//admin//users"));
    assertTrue(route.matches("GET", "/admin/users//"));
    assertTrue(route.matches("GET", "///admin///users///"));
  }

  @Test
  void preventsPathTraversalWithEncodedCharacters() {
    BlockedRoute route = new BlockedRoute("GET", "/admin/users");
    // %2F is URL-encoded forward slash
    assertTrue(route.matches("GET", "/admin%2Fusers"));
    assertTrue(route.matches("GET", "%2Fadmin%2Fusers"));
    assertTrue(route.matches("GET", "/admin%2F%2Fusers"));
  }

  @Test
  void preventsPathTraversalWithDotDotSequences() {
    BlockedRoute route = new BlockedRoute("GET", "/admin/users");
    // These should be normalized and match
    assertTrue(route.matches("GET", "/admin/../admin/users"));
    assertTrue(route.matches("GET", "/admin/./users"));
    assertTrue(route.matches("GET", "/admin/../admin/./users"));
  }

  @Test
  void preventsPathTraversalAboveRoot() {
    BlockedRoute route = new BlockedRoute("GET", "/admin/users");
    // These should throw IllegalArgumentException due to path traversal above root
    assertThrows(
        IllegalArgumentException.class, () -> route.matches("GET", "/admin/../../etc/passwd"));
    assertThrows(IllegalArgumentException.class, () -> route.matches("GET", "../../../etc/passwd"));
    assertThrows(IllegalArgumentException.class, () -> route.matches("GET", "/admin/../../"));
  }

  @Test
  void handlesComplexPathTraversalAttempts() {
    BlockedRoute route = new BlockedRoute("GET", "/api/data");
    // Complex combinations of traversal techniques
    assertTrue(route.matches("GET", "/api//data"));
    assertTrue(route.matches("GET", "/api/./data"));
    assertTrue(route.matches("GET", "/api/../api/data"));
    assertTrue(route.matches("GET", "/api%2Fdata"));
    assertTrue(route.matches("GET", "/api%2F%2Fdata"));
  }

  @Test
  void normalizesMultipleTraversalSequences() {
    BlockedRoute route = new BlockedRoute("GET", "/admin/users");
    // Multiple ../ sequences should be resolved correctly
    assertTrue(route.matches("GET", "/admin/../admin/../admin/users"));
    assertTrue(route.matches("GET", "/admin/././users"));
    assertTrue(route.matches("GET", "/admin/../admin/./users"));
  }

  @Test
  void handlesEmptyAndNullPaths() {
    BlockedRoute route = new BlockedRoute("GET", "/");
    assertTrue(route.matches("GET", ""));
    assertTrue(route.matches("GET", null));
  }

  @Test
  void preventsPathTraversalInParameterizedRoutes() {
    BlockedRoute route = new BlockedRoute("DELETE", "/session/{session-id}");
    // Path traversal attempts in parameterized routes should be normalized
    assertTrue(route.matches("DELETE", "/session//123"));
    assertTrue(route.matches("DELETE", "/session/./123"));
    assertTrue(route.matches("DELETE", "/session/../session/123"));
    assertTrue(route.matches("DELETE", "/session%2F123"));
  }

  @Test
  void handlesMixedTraversalTechniques() {
    BlockedRoute route = new BlockedRoute("GET", "/admin/users");
    // Mixed techniques should all be normalized correctly
    assertTrue(route.matches("GET", "/admin//./../admin//users"));
    assertTrue(route.matches("GET", "/admin%2F%2F./../admin/users"));
    assertTrue(route.matches("GET", "/admin//%2F./../admin/users"));
  }

  @Test
  void fromStringHandlesPathTraversalInInput() {
    // Test that fromString method can handle path traversal in the input string
    BlockedRoute route = BlockedRoute.fromString("GET:/admin//users");
    assertTrue(route.matches("GET", "/admin/users"));

    route = BlockedRoute.fromString("GET:/admin%2Fusers");
    assertTrue(route.matches("GET", "/admin/users"));
  }
}
