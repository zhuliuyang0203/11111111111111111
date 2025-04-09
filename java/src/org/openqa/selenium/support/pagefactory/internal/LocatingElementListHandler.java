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

package org.openqa.selenium.support.pagefactory.internal;

import java.lang.reflect.Constructor;
import java.lang.reflect.InvocationHandler;
import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Method;
import java.util.ArrayList;
import java.util.List;
import org.openqa.selenium.WebElement;
import org.openqa.selenium.support.pagefactory.ElementLocator;

public class LocatingElementListHandler implements InvocationHandler {
  private final ElementLocator locator;
  private final Class<?> listType;
  private boolean isExtendedElement = false;
  private Constructor<?> cons = null;

  public LocatingElementListHandler(ElementLocator locator, Class<?> listType) {
    this.locator = locator;
    this.listType = listType;
    if(!WebElement.class.equals(listType)) {
      this.isExtendedElement = true;
      try {
        cons = listType.getConstructor(WebElement.class);
      } catch (NoSuchMethodException e) {
        throw new RuntimeException("Constructor with WebElement argument not found for list type: "
                                   + listType.getName());
      }
    }
  }

  public LocatingElementListHandler(ElementLocator locator) {
    this.locator = locator;
    this.listType = WebElement.class;
    this.isExtendedElement = false;
    cons = null;
  }

  @Override
  public Object invoke(Object object, Method method, Object[] objects) throws Throwable {
    List<Object> elementList = new ArrayList<>();
    List<WebElement> elements = locator.findElements();

    if(isExtendedElement && null != cons) {
      for (WebElement element : elements) {
        Object extension = cons.newInstance(element);
        elementList.add(listType.cast(extension));
      }
    }
    try {
        return method.invoke(
          isExtendedElement ? elementList : elements,
          objects);
    } catch (InvocationTargetException e) {
      // Unwrap the underlying exception
      throw e.getCause();
    }
  }
}
