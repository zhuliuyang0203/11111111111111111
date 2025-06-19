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

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.never;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;
import static org.openqa.selenium.remote.http.HttpMethod.DELETE;
import static org.openqa.selenium.remote.http.HttpMethod.GET;
import static org.openqa.selenium.remote.http.HttpMethod.POST;

import java.util.List;
import org.junit.jupiter.api.Test;
import org.openqa.selenium.remote.http.HttpHandler;
import org.openqa.selenium.remote.http.HttpRequest;
import org.openqa.selenium.remote.http.HttpResponse;
import org.openqa.selenium.remote.http.Routable;

class BlockedRoutesFilterTest {

  @Test
  void shouldBlockMatchingRequest() {
    List<BlockedRoute> blockedRoutes =
        List.of(BlockedRoute.fromString("DELETE:/session/{session-id}"));

    HttpHandler mockHandler = mock(HttpHandler.class);

    BlockedRoutesFilter filter = new BlockedRoutesFilter(blockedRoutes, mockHandler);

    HttpRequest request = new HttpRequest(DELETE, "/session/123");

    HttpResponse response = filter.execute(request);

    assertEquals(403, response.getStatus());
    assertNotNull(response.getContent());
    verify(mockHandler, never()).execute(request);
  }

  @Test
  void shouldAllowNonMatchingRequest() {
    List<BlockedRoute> blockedRoutes =
        List.of(BlockedRoute.fromString("DELETE:/session/{session-id}"));

    HttpHandler mockHandler = mock(HttpHandler.class);
    HttpResponse expectedResponse = new HttpResponse().setStatus(200);
    when(mockHandler.execute(any(HttpRequest.class))).thenReturn(expectedResponse);

    BlockedRoutesFilter filter = new BlockedRoutesFilter(blockedRoutes, mockHandler);

    HttpRequest request = new HttpRequest(GET, "/status");

    HttpResponse response = filter.execute(request);

    assertEquals(200, response.getStatus());
  }

  @Test
  void shouldAllowRequestWithDifferentMethod() {
    List<BlockedRoute> blockedRoutes =
        List.of(BlockedRoute.fromString("DELETE:/session/{session-id}"));

    HttpHandler mockHandler = mock(HttpHandler.class);
    HttpResponse expectedResponse = new HttpResponse().setStatus(200);
    when(mockHandler.execute(any(HttpRequest.class))).thenReturn(expectedResponse);

    BlockedRoutesFilter filter = new BlockedRoutesFilter(blockedRoutes, mockHandler);

    HttpRequest request = new HttpRequest(POST, "/session/123");

    HttpResponse response = filter.execute(request);

    assertEquals(200, response.getStatus());
  }

  @Test
  void shouldAllowRequestWithDifferentPath() {
    List<BlockedRoute> blockedRoutes =
        List.of(BlockedRoute.fromString("DELETE:/session/{session-id}"));

    HttpHandler mockHandler = mock(HttpHandler.class);
    HttpResponse expectedResponse = new HttpResponse().setStatus(200);
    when(mockHandler.execute(any(HttpRequest.class))).thenReturn(expectedResponse);

    BlockedRoutesFilter filter = new BlockedRoutesFilter(blockedRoutes, mockHandler);

    HttpRequest request = new HttpRequest(DELETE, "/session");

    HttpResponse response = filter.execute(request);

    assertEquals(200, response.getStatus());
  }

  @Test
  void shouldBlockMultipleMatchingRoutes() {
    List<BlockedRoute> blockedRoutes =
        List.of(
            BlockedRoute.fromString("DELETE:/session/{session-id}"),
            BlockedRoute.fromString("POST:/session"));

    HttpHandler mockHandler = mock(HttpHandler.class);

    BlockedRoutesFilter filter = new BlockedRoutesFilter(blockedRoutes, mockHandler);

    // Test first blocked route
    HttpRequest deleteRequest = new HttpRequest(DELETE, "/session/123");

    HttpResponse deleteResponse = filter.execute(deleteRequest);
    assertEquals(403, deleteResponse.getStatus());

    // Test second blocked route
    HttpRequest postRequest = new HttpRequest(POST, "/session");

    HttpResponse postResponse = filter.execute(postRequest);
    assertEquals(403, postResponse.getStatus());
  }

  @Test
  void shouldReturnOriginalRoutableWhenNoBlockedRoutes() {
    List<BlockedRoute> blockedRoutes = List.of();

    // Create a real Routable implementation
    Routable originalRoutable =
        new Routable() {
          @Override
          public HttpResponse execute(HttpRequest req) {
            return new HttpResponse().setStatus(200);
          }

          @Override
          public boolean matches(HttpRequest req) {
            return true;
          }
        };

    Routable result = BlockedRoutesFilter.with(originalRoutable, blockedRoutes);

    assertEquals(originalRoutable, result);
  }

  @Test
  void shouldReturnOriginalRoutableWhenBlockedRoutesIsNull() {
    // Create a real Routable implementation
    Routable originalRoutable =
        new Routable() {
          @Override
          public HttpResponse execute(HttpRequest req) {
            return new HttpResponse().setStatus(200);
          }

          @Override
          public boolean matches(HttpRequest req) {
            return true;
          }
        };

    Routable result = BlockedRoutesFilter.with(originalRoutable, null);

    assertEquals(originalRoutable, result);
  }

  @Test
  void shouldHandleCaseInsensitiveMethodMatching() {
    List<BlockedRoute> blockedRoutes =
        List.of(BlockedRoute.fromString("DELETE:/session/{session-id}"));

    HttpHandler mockHandler = mock(HttpHandler.class);

    BlockedRoutesFilter filter = new BlockedRoutesFilter(blockedRoutes, mockHandler);

    HttpRequest request = new HttpRequest(DELETE, "/session/123");

    HttpResponse response = filter.execute(request);

    assertEquals(403, response.getStatus());
  }

  @Test
  void shouldNotBlockRequestWithFollowPath() {
    List<BlockedRoute> blockedRoutes =
        List.of(BlockedRoute.fromString("DELETE:/session/{session-id}"));

    HttpHandler mockHandler = mock(HttpHandler.class);
    HttpResponse expectedResponse = new HttpResponse().setStatus(200);
    when(mockHandler.execute(any(HttpRequest.class))).thenReturn(expectedResponse);

    BlockedRoutesFilter filter = new BlockedRoutesFilter(blockedRoutes, mockHandler);

    HttpRequest request = new HttpRequest(DELETE, "/session/123/se/bidi");

    HttpResponse response = filter.execute(request);

    assertEquals(200, response.getStatus());
  }
}
