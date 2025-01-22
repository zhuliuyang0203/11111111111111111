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

package org.openqa.selenium.bidi.network;

import java.util.HashMap;
import java.util.List;
import java.util.Map;
import org.openqa.selenium.internal.Require;

public class SetCacheBehaviorParameters {
  private final CacheBehavior cacheBehavior;
  private final List<String> contexts;

  public SetCacheBehaviorParameters(CacheBehavior cacheBehavior) {
    this(cacheBehavior, null);
  }

  public SetCacheBehaviorParameters(CacheBehavior cacheBehavior, List<String> contexts) {
    this.cacheBehavior = Require.nonNull("Cache behavior", cacheBehavior);
    this.contexts = contexts;
  }

  public Map<String, Object> toMap() {
    Map<String, Object> map = new HashMap<>();
    map.put("cacheBehavior", cacheBehavior.toString());

    if (contexts != null && !contexts.isEmpty()) {
      map.put("contexts", contexts);
    }

    return map;
  }
}
