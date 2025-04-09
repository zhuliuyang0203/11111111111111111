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

package org.openqa.selenium.support.pagefactory;

import java.lang.reflect.Field;
import java.lang.reflect.InvocationHandler;
import java.lang.reflect.ParameterizedType;
import java.lang.reflect.Proxy;
import java.lang.reflect.Type;
import java.util.List;
import org.openqa.selenium.WebElement;
import org.openqa.selenium.WrapsElement;
import org.openqa.selenium.interactions.Locatable;
import org.openqa.selenium.support.FindAll;
import org.openqa.selenium.support.FindBy;
import org.openqa.selenium.support.FindBys;
import org.openqa.selenium.support.pagefactory.internal.LocatingElementHandler;
import org.openqa.selenium.support.pagefactory.internal.LocatingElementListHandler;
import org.openqa.selenium.support.ui.AbstractExtendedElement;

/**
 * Default decorator for use with PageFactory. Will decorate 1) all the WebElement fields and 2)
 * List&lt;WebElement&gt; fields that have {@literal @FindBy}, {@literal @FindBys}, or
 * {@literal @FindAll} annotation with a proxy that locates the elements using the passed in
 * ElementLocatorFactory.
 */
public class DefaultFieldDecorator implements FieldDecorator {

  protected ElementLocatorFactory factory;

  public DefaultFieldDecorator(ElementLocatorFactory factory) {
    this.factory = factory;
  }

  @Override
  public Object decorate(ClassLoader loader, Field field) {
    Class<?> type = field.getType();
    if (!(WebElement.class.isAssignableFrom(type) || isDecoratableList(field))) {
      return null;
    }

    ElementLocator locator = factory.createLocator(field);
    if (locator == null) {
      return null;
    }

    if (WebElement.class.isAssignableFrom(type)) {
      WebElement elementProxy = proxyForLocator(loader, locator);
      if(AbstractExtendedElement.class.isAssignableFrom(type)) {
        try {
          return type.getConstructor(WebElement.class).newInstance(elementProxy);
        } catch (Exception e) {
          return null;
        }
      }
      return elementProxy;
    } else if (List.class.isAssignableFrom(type)) {
      return proxyForListLocator(loader, locator, getErasureType(field));
    }
    else {
      return null;
    }
  }

  protected boolean isDecoratableList(Field field) {
    if (!List.class.isAssignableFrom(field.getType())) {
      return false;
    }

    // Type erasure in Java isn't complete. Attempt to discover the generic
    // type of the list.
    Type genericType = field.getGenericType();
    if (!(genericType instanceof ParameterizedType)) {
      return false;
    }

    Type listType = ((ParameterizedType) genericType).getActualTypeArguments()[0];

    if (!WebElement.class.equals(listType)) {
      if (listType instanceof Class) {
        if (!AbstractExtendedElement.class.isAssignableFrom((Class<?>) listType)) {
          return false;
        }
      } else {
        return false;
      }
    }

    return field.getAnnotation(FindBy.class) != null
        || field.getAnnotation(FindBys.class) != null
        || field.getAnnotation(FindAll.class) != null;
  }

  private Class<?> getErasureType(Field field) {
    Type genericType = field.getGenericType();
    if (!(genericType instanceof ParameterizedType)) {
      return null;
    }
    return (Class<?>) ((ParameterizedType) genericType).getActualTypeArguments()[0];
  }

  protected WebElement proxyForLocator(ClassLoader loader, ElementLocator locator) {
    InvocationHandler handler = new LocatingElementHandler(locator);

    WebElement proxy;
    proxy =
        (WebElement)
            Proxy.newProxyInstance(
                loader,
                new Class[] {WebElement.class, WrapsElement.class, Locatable.class},
                handler);
    return proxy;
  }

  @SuppressWarnings("unchecked")
  protected <T extends WebElement> List<T> proxyForListLocator(ClassLoader loader, ElementLocator locator, Class<?> type) {
    InvocationHandler handler = new LocatingElementListHandler(locator, type);

    List<T> proxy;
    proxy = (List<T>) Proxy.newProxyInstance(loader, new Class[] {List.class}, handler);
    return proxy;
  }
}
