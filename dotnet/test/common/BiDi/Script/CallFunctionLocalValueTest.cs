// <copyright file="CallFunctionLocalValueTest.cs" company="Selenium Committers">
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
using OpenQA.Selenium.BiDi.Modules.Script;

namespace OpenQA.Selenium.BiDi.Script;

class CallFunctionLocalValueTest : BiDiTestFixture
{
    [Test]
    public void CanCallFunctionWithArgumentUndefined()
    {
        var arg = LocalValue.Undefined;
        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (typeof arg !== 'undefined') {
                throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentNull()
    {
        var arg = LocalValue.Null;
        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg !== null) {
              throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentTrue()
    {
        var arg = LocalValue.True;
        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg !== true) {
              throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentFalse()
    {
        var arg = LocalValue.False;
        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg !== false) {
              throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentBigInt()
    {
        var arg = LocalValue.BigInt(12345);
        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg !== 12345n) {
              throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentEmptyString()
    {
        var arg = LocalValue.String(string.Empty);
        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg !== '') {
                throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentNonEmptyString()
    {
        var arg = LocalValue.String("whoa");
        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg !== 'whoa') {
                throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentRecentDate()
    {
        const string PinnedDateTimeString = "2025-03-09T00:30:33.083Z";

        var arg = new DateLocalValue(PinnedDateTimeString);

        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg.toISOString() !== '{{PinnedDateTimeString}}') {
                throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentEpochDate()
    {
        const string EpochString = "1970-01-01T00:00:00.000Z";

        var arg = new DateLocalValue(EpochString);

        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg.toISOString() !== '{{EpochString}}') {
                throw new Error("Assert failed: " + arg.toISOString());
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentNumberFive()
    {
        var arg = LocalValue.Number(5);

        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg !== 5) {
                throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentNumberNegativeFive()
    {
        var arg = LocalValue.Number(-5);

        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg !== -5) {
                throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentNumberZero()
    {
        var arg = LocalValue.Number(0);

        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg !== 0) {
                throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    [IgnoreBrowser(Selenium.Browser.Edge, "Chromium can't handle -0 argument as a number: https://github.com/w3c/webdriver-bidi/issues/887")]
    [IgnoreBrowser(Selenium.Browser.Chrome, "Chromium can't handle -0 argument as a number: https://github.com/w3c/webdriver-bidi/issues/887")]
    public void CanCallFunctionWithArgumentNumberNegativeZero()
    {
        var arg = LocalValue.Number(double.NegativeZero);

        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (!Object.is(arg, -0)) {
                throw new Error("Assert failed: " + arg.toLocaleString());
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentNumberPositiveInfinity()
    {
        var arg = LocalValue.Number(double.PositiveInfinity);

        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg !== Number.POSITIVE_INFINITY) {
                throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentNumberNegativeInfinity()
    {
        var arg = LocalValue.Number(double.NegativeInfinity);

        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg !== Number.NEGATIVE_INFINITY) {
                throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentNumberNaN()
    {
        var arg = LocalValue.Number(double.NaN);
        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (!isNaN(arg)) {
                throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentRegExp()
    {
        var arg = new RegExpLocalValue(new RegExpValue("foo*") { Flags = "g" });

        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (!arg.test('foo') || arg.source !== 'foo*' || !arg.global) {
                throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentArray()
    {
        var arg = LocalValue.Array(["hi"]);

        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg.length !== 1 || arg[0] !== 'hi') {
                throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentObject()
    {
        var arg = LocalValue.Object([["objKey", "objValue"]]);

        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg.objKey !== 'objValue' || Object.keys(arg).length !== 1) {
                throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentMap()
    {
        var arg = LocalValue.Map([["mapKey", "mapValue"]]);

        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg.get('mapKey') !== 'mapValue' || arg.size !== 1) {
                throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentSet()
    {
        var arg = LocalValue.Set(["setKey"]);

        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (!arg.has('setKey') || arg.size !== 1) {
                throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }
}
