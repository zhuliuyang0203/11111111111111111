// <copyright file="IgnoreBrowserAttribute.cs" company="Selenium Committers">
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

using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using OpenQA.Selenium.Environment;
using System;

namespace OpenQA.Selenium
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class IgnoreBrowserAttribute : NUnitAttribute, IApplyToTest
    {
        public IgnoreBrowserAttribute(Browser browser)
        {
            this.Value = browser;
        }

        public IgnoreBrowserAttribute(Browser browser, string reason)
            : this(browser)
        {
            this.Reason = reason;
        }

        public Browser Value { get; }

        public string Reason { get; } = string.Empty;

        public void ApplyToTest(Test test)
        {
            if (test.RunState != RunState.NotRunnable)
            {
                Attribute[] ignoreAttributes;
                if (test.IsSuite)
                {
                    ignoreAttributes = test.TypeInfo.GetCustomAttributes<IgnoreBrowserAttribute>(true);
                }
                else
                {
                    ignoreAttributes = test.Method.GetCustomAttributes<IgnoreBrowserAttribute>(true);
                }

                foreach (Attribute attr in ignoreAttributes)
                {
                    if (attr is IgnoreBrowserAttribute browserToIgnoreAttr
                        && IgnoreTestForBrowser(browserToIgnoreAttr.Value))
                    {
                        string ignoreReason = $"Ignoring browser {EnvironmentManager.Instance.Browser}.";
                        if (!string.IsNullOrEmpty(browserToIgnoreAttr.Reason))
                        {
                            ignoreReason = ignoreReason + " " + browserToIgnoreAttr.Reason;
                        }

                        test.RunState = RunState.Ignored;
                        test.Properties.Set(PropertyNames.SkipReason, ignoreReason);
                    }
                }
            }
        }

        private static bool IgnoreTestForBrowser(Browser browserToIgnore)
        {
            return browserToIgnore.Equals(EnvironmentManager.Instance.Browser) || browserToIgnore.Equals(Browser.All) || IsRemoteInstanceOfBrowser(browserToIgnore);
        }

        private static bool IsRemoteInstanceOfBrowser(Browser desiredBrowser)
        {
            return (desiredBrowser, EnvironmentManager.Instance.RemoteCapabilities) switch
            {
                (Browser.IE, "internet explorer") => true,
                (Browser.Firefox, "firefox") => true,
                (Browser.Chrome, "chrome") => true,
                (Browser.Edge, "MicrosoftEdge") => true,
                _ => false,
            };
        }
    }
}
