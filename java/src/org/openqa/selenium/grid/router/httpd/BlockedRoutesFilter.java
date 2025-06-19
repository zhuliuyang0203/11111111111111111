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

import java.net.URI;
import java.util.List;
import java.util.logging.Logger;
import org.openqa.selenium.remote.http.Contents;
import org.openqa.selenium.remote.http.HttpHandler;
import org.openqa.selenium.remote.http.HttpRequest;
import org.openqa.selenium.remote.http.HttpResponse;
import org.openqa.selenium.remote.http.Routable;

/** Filter that blocks requests matching specified routes. */
public class BlockedRoutesFilter implements HttpHandler {

  private static final Logger LOG = Logger.getLogger(BlockedRoutesFilter.class.getName());
  private final List<BlockedRoute> blockedRoutes;
  private final HttpHandler delegate;

  public BlockedRoutesFilter(List<BlockedRoute> blockedRoutes, HttpHandler delegate) {
    this.blockedRoutes = blockedRoutes;
    this.delegate = delegate;
  }

  @Override
  public HttpResponse execute(HttpRequest request) {
    String method = request.getMethod().toString();
    String path = URI.create(request.getUri()).getPath();

    // Check if the request matches any blocked route
    for (BlockedRoute blockedRoute : blockedRoutes) {
      if (blockedRoute.matches(method, path)) {
        LOG.warning(
            "Blocked request: "
                + method
                + " "
                + path
                + " (matches blocked route: "
                + blockedRoute
                + ")");
        return new HttpResponse()
            .setStatus(403) // Forbidden
            .setContent(
                Contents.utf8String("Route blocked by configuration: " + method + " " + path));
      }
    }

    // If not blocked, delegate to the next handler
    return delegate.execute(request);
  }

  /** Creates a Routable that applies the blocked routes filter. */
  public static Routable with(Routable routable, List<BlockedRoute> blockedRoutes) {
    if (blockedRoutes == null || blockedRoutes.isEmpty()) {
      return routable;
    }

    return new Routable() {
      @Override
      public HttpResponse execute(HttpRequest req) {
        return new BlockedRoutesFilter(blockedRoutes, routable).execute(req);
      }

      @Override
      public boolean matches(HttpRequest req) {
        return routable.matches(req);
      }
    };
  }
}
