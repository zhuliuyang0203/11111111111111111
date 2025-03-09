// <copyright file="CallFunctionParameterTest.cs" company="Selenium Committers">
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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace OpenQA.Selenium.BiDi.Script;

class CallFunctionParameterTest : BiDiTestFixture
{
    [Test]
    public async Task CanCallFunctionWithDeclaration()
    {
        var res = await context.Script.CallFunctionAsync("() => { return 1 + 2; }", false);

        Assert.That(res, Is.Not.Null);
        Assert.That(res.Realm, Is.Not.Null);
        Assert.That((res.Result as RemoteValue.Number).Value, Is.EqualTo(3));
    }

    [Test]
    public async Task CanCallFunctionWithDeclarationImplicitCast()
    {
        var res = await context.Script.CallFunctionAsync<int>("() => { return 1 + 2; }", false);

        Assert.That(res, Is.EqualTo(3));
    }

    [Test]
    public async Task CanEvaluateScriptWithUserActivationTrue()
    {
        await context.Script.EvaluateAsync("window.open();", true);

        var res = await context.Script.CallFunctionAsync<bool>("""
            () => navigator.userActivation.isActive && navigator.userActivation.hasBeenActive
            """, true, new() { UserActivation = true });

        Assert.That(res, Is.True);
    }

    [Test]
    public async Task CanEvaluateScriptWithUserActivationFalse()
    {
        await context.Script.EvaluateAsync("window.open();", true);

        var res = await context.Script.CallFunctionAsync<bool>("""
            () => navigator.userActivation.isActive && navigator.userActivation.hasBeenActive
            """, true);

        Assert.That(res, Is.False);
    }

    [Test]
    public async Task CanCallFunctionWithArguments()
    {
        var res = await context.Script.CallFunctionAsync("(...args)=>{return args}", false, new()
        {
            Arguments = ["abc", 42]
        });

        Assert.That(res.Result, Is.AssignableFrom<RemoteValue.Array>());
        Assert.That((string)(res.Result as RemoteValue.Array).Value[0], Is.EqualTo("abc"));
        Assert.That((int)(res.Result as RemoteValue.Array).Value[1], Is.EqualTo(42));
    }

    [Test]
    public async Task CanCallFunctionToGetIFrameBrowsingContext()
    {
        driver.Url = UrlBuilder.WhereIs("click_too_big_in_frame.html");

        var res = await context.Script.CallFunctionAsync("""
            () => document.querySelector('iframe[id="iframe1"]').contentWindow
            """, false);

        Assert.That(res, Is.Not.Null);
        Assert.That(res.Result, Is.AssignableFrom<RemoteValue.WindowProxy>());
        Assert.That((res.Result as RemoteValue.WindowProxy).Value, Is.Not.Null);
    }

    [Test]
    public async Task CanCallFunctionToGetElement()
    {
        driver.Url = UrlBuilder.WhereIs("bidi/logEntryAdded.html");

        var res = await context.Script.CallFunctionAsync("""
            () => document.getElementById("consoleLog")
            """, false);

        Assert.That(res, Is.Not.Null);
        Assert.That(res.Result, Is.AssignableFrom<RemoteValue.Node>());
        Assert.That((res.Result as RemoteValue.Node).Value, Is.Not.Null);
    }

    [Test]
    public async Task CanCallFunctionWithAwaitPromise()
    {
        var res = await context.Script.CallFunctionAsync<string>("""
            async function() {
                await new Promise(r => setTimeout(() => r(), 0));
                return "SOME_DELAYED_RESULT";
            }
            """, awaitPromise: true);

        Assert.That(res, Is.EqualTo("SOME_DELAYED_RESULT"));
    }

    [Test]
    public async Task CanCallFunctionWithAwaitPromiseFalse()
    {
        var res = await context.Script.CallFunctionAsync("""
            async function() {
                await new Promise(r => setTimeout(() => r(), 0));
                return "SOME_DELAYED_RESULT";
            }
            """, awaitPromise: false);

        Assert.That(res, Is.Not.Null);
        Assert.That(res.Result, Is.AssignableFrom<RemoteValue.Promise>());
    }

    [Test]
    public async Task CanCallFunctionWithThisParameter()
    {
        var thisParameter = new LocalValue.Object([["some_property", 42]]);

        var res = await context.Script.CallFunctionAsync<int>("""
            function(){return this.some_property}
            """, false, new() { This = thisParameter });

        Assert.That(res, Is.EqualTo(42));
    }

    [Test]
    public async Task CanCallFunctionWithOwnershipRoot()
    {
        var res = await context.Script.CallFunctionAsync("async function(){return {a:1}}", true, new()
        {
            ResultOwnership = ResultOwnership.Root
        });

        Assert.That(res, Is.Not.Null);
        Assert.That((res.Result as RemoteValue.Object).Handle, Is.Not.Null);
        Assert.That((string)(res.Result as RemoteValue.Object).Value[0][0], Is.EqualTo("a"));
        Assert.That((int)(res.Result as RemoteValue.Object).Value[0][1], Is.EqualTo(1));
    }

    [Test]
    public async Task CanCallFunctionWithOwnershipNone()
    {
        var res = await context.Script.CallFunctionAsync("async function(){return {a:1}}", true, new()
        {
            ResultOwnership = ResultOwnership.None
        });

        Assert.That(res, Is.Not.Null);
        Assert.That((res.Result as RemoteValue.Object).Handle, Is.Null);
        Assert.That((string)(res.Result as RemoteValue.Object).Value[0][0], Is.EqualTo("a"));
        Assert.That((int)(res.Result as RemoteValue.Object).Value[0][1], Is.EqualTo(1));
    }

    [Test]
    public void CanCallFunctionThatThrowsException()
    {
        var action = () => context.Script.CallFunctionAsync("))) !!@@## some invalid JS script (((", false);

        Assert.That(action, Throws.InstanceOf<ScriptEvaluateException>().And.Message.Contain("SyntaxError:"));
    }

    [Test]
    public async Task CanCallFunctionInASandBox()
    {
        // Make changes without sandbox
        await context.Script.CallFunctionAsync("() => { window.foo = 1; }", true);

        var res = await context.Script.CallFunctionAsync("() => window.foo", true, targetOptions: new() { Sandbox = "sandbox" });

        Assert.That(res.Result, Is.AssignableFrom<RemoteValue.Undefined>());

        // Make changes in the sandbox
        await context.Script.CallFunctionAsync("() => { window.foo = 2; }", true, targetOptions: new() { Sandbox = "sandbox" });

        // Check if the changes are present in the sandbox
        res = await context.Script.CallFunctionAsync("() => window.foo", true, targetOptions: new() { Sandbox = "sandbox" });

        Assert.That(res.Result, Is.AssignableFrom<RemoteValue.Number>());
        Assert.That((res.Result as RemoteValue.Number).Value, Is.EqualTo(2));
    }

    [Test]
    public async Task CanCallFunctionInARealm()
    {
        await bidi.BrowsingContext.CreateAsync(Modules.BrowsingContext.ContextType.Tab);

        var realms = await bidi.Script.GetRealmsAsync();

        await bidi.Script.CallFunctionAsync("() => { window.foo = 3; }", true, new Target.Realm(realms[0].Realm));
        await bidi.Script.CallFunctionAsync("() => { window.foo = 5; }", true, new Target.Realm(realms[1].Realm));

        var res1 = await bidi.Script.CallFunctionAsync<int>("() => window.foo", true, new Target.Realm(realms[0].Realm));
        var res2 = await bidi.Script.CallFunctionAsync<int>("() => window.foo", true, new Target.Realm(realms[1].Realm));

        Assert.That(res1, Is.EqualTo(3));
        Assert.That(res2, Is.EqualTo(5));
    }

    [Test]
    public async Task CanCallFunctionAndRoundtripArguments_Undefined()
    {
        var response = await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (typeof arg === 'undefined') {
                return arg;
              }

              throw new Error("Assert failed: " + arg);
            }
            """, false, new() { Arguments = [new LocalValue.Undefined()] });

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Undefined>());
        Assert.That(response.Result, Is.EqualTo(new RemoteValue.Undefined()));
    }

    [Test]
    public async Task CanCallFunctionAndRoundtripArguments_Null()
    {
        var response = await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg === null) {
                return arg;
              }

              throw new Error("Assert failed: " + arg);
            }
            """, false, new() { Arguments = [new LocalValue.Null()] });

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Null>());
        Assert.That(response.Result, Is.EqualTo(new RemoteValue.Null()));
    }

    [Test]
    public async Task CanCallFunctionAndRoundtripArguments_EmptyString()
    {
        var response = await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg === '') {
                return arg;
              }

              throw new Error("Assert failed: " + arg);
            }
            """, false, new() { Arguments = [new LocalValue.String(string.Empty)] });

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.String>());
        Assert.That(response.Result, Is.EqualTo(new RemoteValue.String(string.Empty)));
    }

    [Test]
    public async Task CanCallFunctionAndRoundtripArguments_NonEmptyString()
    {
        var response = await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg === 'whoa') {
                return arg;
              }

              throw new Error("Assert failed: " + arg);
            }
            """, false, new() { Arguments = [new LocalValue.String("whoa")] });

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.String>());
        Assert.That(response.Result, Is.EqualTo(new RemoteValue.String("whoa")));
    }

    [Test]
    public async Task CanCallFunctionAndRoundtripArguments_RecentDate()
    {
        const string PinnedDateTimeString = "2025-03-09T00:30:33.083Z";

        var response = await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg.toISOString() === '{{PinnedDateTimeString}}') {
                return arg;
              }

              throw new Error("Assert failed: " + arg);
            }
            """, false, new() { Arguments = [new LocalValue.Date(PinnedDateTimeString)] });

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Date>());
        Assert.That(response.Result, Is.EqualTo(new RemoteValue.Date(PinnedDateTimeString)));
    }

    [Test]
    public async Task CanCallFunctionAndRoundtripArguments_UnixEpoch()
    {
        const string EpochString = "1970-01-01T00:00:00.000Z";

        var response = await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg.toISOString() === '{{EpochString}}') {
                return arg;
              }

              throw new Error("Assert failed: " + arg.toISOString());
            }
            """, false, new() { Arguments = [new LocalValue.Date(EpochString)] });

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Date>());
        Assert.That(response.Result, Is.EqualTo(new RemoteValue.Date(EpochString)));
    }

    [Test]
    public async Task CanCallFunctionAndRoundtripArguments_Number5()
    {
        var response = await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg === 5) {
                return arg;
              }

              throw new Error("Assert failed: " + arg);
            }
            """, false, new() { Arguments = [new LocalValue.Number(5)] });

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Number>());
        Assert.That(response.Result, Is.EqualTo(new RemoteValue.Number(5)));
    }

    [Test]
    public async Task CanCallFunctionAndRoundtripArguments_NumberNegative5()
    {
        var response = await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg === -5) {
                return arg;
              }

              throw new Error("Assert failed: " + arg);
            }
            """, false, new() { Arguments = [new LocalValue.Number(-5)] });

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Number>());
        Assert.That(response.Result, Is.EqualTo(new RemoteValue.Number(-5)));
    }

    [Test]
    public async Task CanCallFunctionAndRoundtripArguments_Number0()
    {
        var response = await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg === 0) {
                return arg;
              }

              throw new Error("Assert failed: " + arg);
            }
            """, false, new() { Arguments = [new LocalValue.Number(0)] });

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Number>());
        Assert.That(response.Result, Is.EqualTo(new RemoteValue.Number(0)));
    }

    [Test]
    public async Task CanCallFunctionAndRoundtripArguments_NumberNegative0()
    {
        var response = await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg === -0) {
                return arg;
              }

              throw new Error("Assert failed: " + arg);
            }
            """, false, new() { Arguments = [new LocalValue.Number(double.NegativeZero)] });

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Number>());
        Assert.That(response.Result, Is.EqualTo(new RemoteValue.Number(double.NegativeZero)));
    }

    [Test]
    public async Task CanCallFunctionAndRoundtripArguments_NumberPositiveInfinity()
    {
        var response = await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg === Number.POSITIVE_INFINITY) {
                return arg;
              }

              throw new Error("Assert failed: " + arg);
            }
            """, false, new() { Arguments = [new LocalValue.Number(double.PositiveInfinity)] });

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Number>());
        Assert.That(response.Result, Is.EqualTo(new RemoteValue.Number(double.PositiveInfinity)));
    }

    [Test]
    public async Task CanCallFunctionAndRoundtripArguments_NumberNegativeInfinity()
    {
        var response = await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg === Number.NEGATIVE_INFINITY) {
                return arg;
              }

              throw new Error("Assert failed: " + arg);
            }
            """, false, new() { Arguments = [new LocalValue.Number(double.NegativeInfinity)] });

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Number>());
        Assert.That(response.Result, Is.EqualTo(new RemoteValue.Number(double.NegativeInfinity)));
    }

    [Test]
    public async Task CanCallFunctionAndRoundtripArguments_NumberNaN()
    {
        var response = await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (isNaN(arg)) {
                return arg;
              }

              throw new Error("Assert failed: " + arg);
            }
            """, false, new() { Arguments = [new LocalValue.Number(double.NaN)] });

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Number>());
        Assert.That(response.Result, Is.EqualTo(new RemoteValue.Number(double.NaN)));
    }

    [Test]
    public async Task CanCallFunctionAndRoundtripArguments_RegExp()
    {
        var response = await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg.test('foo') && arg.source === 'foo*' && arg.global) {
                return arg;
              }

              throw new Error("Assert failed: " + arg);
            }
            """, false, new() { Arguments = [new LocalValue.RegExp(new LocalValue.RegExp.RegExpValue("foo*") { Flags = "g" })] });

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.RegExp>());
        Assert.That(response.Result, Is.EqualTo(new RemoteValue.RegExp(new RemoteValue.RegExp.RegExpValue("foo*") { Flags = "g" })));
    }

    [Test]
    public async Task CanCallFunctionAndRoundtripArguments_Array()
    {
        var response = await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg.length === 1 && arg[0] === 'hi') {
                return arg;
              }

              throw new Error("Assert failed: " + arg);
            }
            """, false, new() { Arguments = [new LocalValue.Array([new LocalValue.String("hi")])] });

        var expectedArray = new RemoteValue.Array { Value = [new RemoteValue.String("hi")] };
        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Array>());
        Assert.That(((RemoteValue.Array)response.Result).Value, Is.EqualTo(expectedArray.Value));
    }

    [Test]
    public async Task CanCallFunctionAndRoundtripArguments_Object()
    {
        var argument = new LocalValue.Object([[new LocalValue.String("key"), new LocalValue.String("value")]]);
        var expected = new RemoteValue.Object
        {
            Value = [[new RemoteValue.String("key"), new RemoteValue.String("value")]]
        };

        var response = await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg.key === 'value' && Object.keys(arg).length === 1) {
                return arg;
              }

              throw new Error("Assert failed: " + arg);
            }
            """, false, new() { Arguments = [argument] });

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Object>());
        Assert.That(((RemoteValue.Object)response.Result).Value, Is.EqualTo(expected.Value));
    }

    [Test]
    public async Task CanCallFunctionAndRoundtripArguments_Map()
    {
        var argument = new LocalValue.Map([[new LocalValue.String("key"), new LocalValue.String("value")]]);
        var expected = new RemoteValue.Map
        {
            Value = [[new RemoteValue.String("key"), new RemoteValue.String("value")]]
        }
        ;

        var response = await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg.get('key') === 'value' && arg.size === 1) {
                return arg;
              }

              throw new Error("Assert failed: " + arg);
            }
            """, false, new() { Arguments = [argument] });

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Map>());
        Assert.That(((RemoteValue.Map)response.Result).Value, Is.EqualTo(expected.Value));
    }

    [Test]
    public async Task CanCallFunctionAndRoundtripArguments_Set()
    {
        var argument = new LocalValue.Set([new LocalValue.String("key")]);
        var expected = new RemoteValue.Set { Value = [new RemoteValue.String("key")] };

        var response = await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg.has('key') && arg.size === 1) {
                return arg;
              }

              throw new Error("Assert failed: " + arg);
            }
            """, false, new() { Arguments = [argument] });

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Set>());
        Assert.That(((RemoteValue.Set)response.Result).Value, Is.EqualTo(expected.Value));
    }
}
