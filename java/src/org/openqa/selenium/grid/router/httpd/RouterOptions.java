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

import java.util.List;
import java.util.stream.Collectors;
import java.util.stream.Stream;
import org.openqa.selenium.grid.config.Config;

public class RouterOptions {

  static final String NETWORK_SECTION = "network";
  static final String ROUTER_SECTION = "router";

  private final Config config;

  public RouterOptions(Config config) {
    this.config = config;
  }

  public String subPath() {
    return config
        .get(NETWORK_SECTION, "sub-path")
        .map(
            prefix -> {
              prefix = prefix.trim();
              if (!prefix.startsWith("/")) {
                prefix = "/" + prefix; // Prefix with a '/' if absent.
              }
              if (prefix.endsWith("/")) {
                prefix =
                    prefix.substring(0, prefix.length() - 1); // Remove the trailing '/' if present.
              }
              return prefix;
            })
        .orElse("");
  }

  public boolean disableUi() {
    return config.get(ROUTER_SECTION, "disable-ui").map(Boolean::parseBoolean).orElse(false);
  }

  /**
   * Returns a list of blocked routes from the configuration. Each blocked route should be specified
   * in the format "METHOD:path" (e.g., "DELETE:/session/{session-id}"). Multiple routes can be
   * specified as a list. If the blocked-delete-session flag is enabled,
   * DELETE:/session/{session-id} will be automatically added.
   *
   * @return List of blocked routes
   */
  public List<BlockedRoute> getBlockedRoutes() {
    List<BlockedRoute> routes =
        config
            .getAll(ROUTER_SECTION, "blocked-routes")
            .map(
                blockedRoutesList -> {
                  if (blockedRoutesList.isEmpty()) {
                    return List.<BlockedRoute>of();
                  }

                  return blockedRoutesList.stream()
                      .map(String::trim)
                      .filter(s -> !s.isEmpty())
                      .map(BlockedRoute::fromString)
                      .collect(Collectors.toList());
                })
            .orElse(List.of());

    // Add DELETE session route if the flag is enabled
    boolean blockedDeleteSession =
        config.getBool(ROUTER_SECTION, "blocked-delete-session").orElse(false);

    if (blockedDeleteSession) {
      BlockedRoute deleteSessionRoute = new BlockedRoute("DELETE", "/session/{session-id}");
      // Only add if not already present
      if (routes.stream()
          .noneMatch(
              route ->
                  "DELETE".equals(route.getMethod())
                      && "/session/{session-id}".equals(route.getPath()))) {
        routes =
            Stream.concat(routes.stream(), Stream.of(deleteSessionRoute))
                .collect(Collectors.toList());
      }
    }

    return routes;
  }
}
