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

package org.openqa.selenium.print;

import java.util.HashMap;
import java.util.Map;

public class PageSize {
  private final double height;
  private final double width;

    // Reference for predefined page size constants: https://www.agooddaytoprint.com/page/paper-size-chart-faq
    public static final PageSize A4 = new PageSize(29.7, 21.0); // A4 size in cm
    public static final PageSize LEGAL = new PageSize(35.56, 21.59); // Legal size in cm
    public static final PageSize TABLOID = new PageSize(43.18, 27.94); // Tabloid size in cm
    public static final PageSize LETTER = new PageSize(27.94, 21.59); // Letter size in cm

  public PageSize() {
    // Initialize with defaults. A4 paper size defaults in cms.
    this.height = 27.94;
    this.width = 21.59;
 }

  public PageSize(double height, double width) {
    this.height = height;
    this.width = width;
  }

  public double getHeight() {
    return height;
  }

  public double getWidth() {
    return width;
  }

  public Map<String, Object> toMap() {
    final Map<String, Object> options = new HashMap<>(7);
    options.put("height", getHeight());
    options.put("width", getWidth());

    return options;
  }

    @Override
    public String toString() {
    return "PageSize[width=" + this.getWidth() + ", height=" + this.getHeight() + "]";
 }

}