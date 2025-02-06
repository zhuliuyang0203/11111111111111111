// <copyright file="DriverFactory.cs" company="Selenium Committers">
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
// </copyright>

using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chromium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Safari;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace OpenQA.Selenium.Environment
{
    public class DriverFactory
    {
        private readonly string driverPath;
        private readonly string browserBinaryLocation;
        private readonly Dictionary<Browser, Type> serviceTypes = new Dictionary<Browser, Type>();
        private readonly Dictionary<Browser, Type> optionsTypes = new Dictionary<Browser, Type>();

        public DriverFactory(string driverPath, string browserBinaryLocation)
        {
            this.driverPath = driverPath;
            this.browserBinaryLocation = browserBinaryLocation;

            this.PopulateServiceTypes();
            this.PopulateOptionsTypes();
        }

        private void PopulateOptionsTypes()
        {
            this.optionsTypes[Browser.Chrome] = typeof(ChromeOptions);
            this.optionsTypes[Browser.Edge] = typeof(EdgeOptions);
            this.optionsTypes[Browser.Firefox] = typeof(FirefoxOptions);
            this.optionsTypes[Browser.IE] = typeof(InternetExplorerOptions);
            this.optionsTypes[Browser.Safari] = typeof(SafariOptions);
        }

        private void PopulateServiceTypes()
        {
            this.serviceTypes[Browser.Chrome] = typeof(ChromeDriverService);
            this.serviceTypes[Browser.Edge] = typeof(EdgeDriverService);
            this.serviceTypes[Browser.Firefox] = typeof(FirefoxDriverService);
            this.serviceTypes[Browser.IE] = typeof(InternetExplorerDriverService);
            this.serviceTypes[Browser.Safari] = typeof(SafariDriverService);
        }

        public event EventHandler<DriverStartingEventArgs> DriverStarting;

        public IWebDriver CreateDriver(Type driverType, bool logging = false)
        {
            return CreateDriverWithOptions(driverType, null, logging);
        }

        public IWebDriver CreateDriverWithOptions(Type driverType, DriverOptions driverOptions, bool enableLogging = false)
        {
            Console.WriteLine($"Creating new driver of {driverType} type...");

            Browser browser = Browser.All;
            DriverService service = null;
            DriverOptions options = null;

            IWebDriver driver;
            if (typeof(ChromeDriver).IsAssignableFrom(driverType))
            {
                browser = Browser.Chrome;
                options = GetDriverOptions<ChromeOptions>(driverType, driverOptions);

                var chromeOptions = (ChromeOptions)options;
                chromeOptions.AddArguments("--no-sandbox", "--disable-dev-shm-usage");

                service = CreateService<ChromeDriverService>();
                if (!string.IsNullOrEmpty(this.browserBinaryLocation))
                {
                    ((ChromeOptions)options).BinaryLocation = this.browserBinaryLocation;
                }
                if (enableLogging)
                {
                    ((ChromiumDriverService)service).EnableVerboseLogging = true;
                }
            }
            else if (typeof(EdgeDriver).IsAssignableFrom(driverType))
            {
                browser = Browser.Edge;
                options = GetDriverOptions<EdgeOptions>(driverType, driverOptions);

                var edgeOptions = (EdgeOptions)options;
                edgeOptions.AddArguments("--no-sandbox", "--disable-dev-shm-usage");

                service = CreateService<EdgeDriverService>();
                if (!string.IsNullOrEmpty(this.browserBinaryLocation))
                {
                    ((EdgeOptions)options).BinaryLocation = this.browserBinaryLocation;
                }
                if (enableLogging)
                {
                    ((ChromiumDriverService)service).EnableVerboseLogging = true;
                }
            }
            else if (typeof(InternetExplorerDriver).IsAssignableFrom(driverType))
            {
                browser = Browser.IE;
                options = GetDriverOptions<InternetExplorerOptions>(driverType, driverOptions);
                service = CreateService<InternetExplorerDriverService>();
                if (enableLogging)
                {
                    ((InternetExplorerDriverService)service).LoggingLevel = InternetExplorerDriverLogLevel.Trace;
                }
            }
            else if (typeof(FirefoxDriver).IsAssignableFrom(driverType))
            {
                browser = Browser.Firefox;
                options = GetDriverOptions<FirefoxOptions>(driverType, driverOptions);
                service = CreateService<FirefoxDriverService>();
                if (!string.IsNullOrEmpty(this.browserBinaryLocation))
                {
                    ((FirefoxOptions)options).BinaryLocation = this.browserBinaryLocation;
                }
                if (enableLogging)
                {
                    ((FirefoxDriverService)service).LogLevel = FirefoxDriverLogLevel.Trace;
                }
            }
            else if (typeof(SafariDriver).IsAssignableFrom(driverType))
            {
                browser = Browser.Safari;
                options = GetDriverOptions<SafariOptions>(driverType, driverOptions);
                service = CreateService<SafariDriverService>();
            }

            if (!string.IsNullOrEmpty(this.driverPath) && service != null)
            {
                service.DriverServicePath = Path.GetDirectoryName(this.driverPath);
                service.DriverServiceExecutableName = Path.GetFileName(this.driverPath);
            }

            this.OnDriverLaunching(service, options);

            if (browser != Browser.All)
            {
                ConstructorInfo ctorInfo = driverType.GetConstructor([this.serviceTypes[browser], this.optionsTypes[browser]]);
                if (ctorInfo != null)
                {
                    return (IWebDriver)ctorInfo.Invoke([service, options]);
                }
            }

            driver = (IWebDriver)Activator.CreateInstance(driverType);
            return driver;
        }

        protected void OnDriverLaunching(DriverService service, DriverOptions options)
        {
            if (this.DriverStarting != null)
            {
                this.DriverStarting(this, new DriverStartingEventArgs(service, options));
            }
        }

        private TOptions GetDriverOptions<TOptions>(Type driverType, DriverOptions overriddenOptions)
            where TOptions : DriverOptions, new()
        {
            TOptions options;

            PropertyInfo defaultOptionsProperty = driverType.GetProperty("DefaultOptions", BindingFlags.Public | BindingFlags.Static);
            if (defaultOptionsProperty != null && defaultOptionsProperty.PropertyType == typeof(TOptions))
            {
                options = (TOptions)defaultOptionsProperty.GetValue(null, null);
            }
            else
            {
                options = new TOptions();
            }

            if (overriddenOptions != null)
            {
                options.PageLoadStrategy = overriddenOptions.PageLoadStrategy;
                options.UnhandledPromptBehavior = overriddenOptions.UnhandledPromptBehavior;
                options.Proxy = overriddenOptions.Proxy;

                options.ScriptTimeout = overriddenOptions.ScriptTimeout;
                options.PageLoadTimeout = overriddenOptions.PageLoadTimeout;
                options.ImplicitWaitTimeout = overriddenOptions.ImplicitWaitTimeout;

                options.UseWebSocketUrl = overriddenOptions.UseWebSocketUrl;
            }

            return options;
        }

        private TService CreateService<TService>()
            where TService : DriverService
        {
            MethodInfo createDefaultServiceMethod = typeof(TService).GetMethod("CreateDefaultService", BindingFlags.Public | BindingFlags.Static, null, [], null);
            if (createDefaultServiceMethod != null && createDefaultServiceMethod.ReturnType == typeof(TService))
            {
                return (TService)createDefaultServiceMethod.Invoke(null, []);
            }

            return default;
        }
    }
}
