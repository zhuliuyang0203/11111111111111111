// <copyright file="LocalValueConversionTests.cs" company="Selenium Committers">
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

namespace OpenQA.Selenium.BiDi.Script;

class LocalValueConversionTests
{
    [Test]
    public void CanConvertNullBoolToLocalValue()
    {
        bool? arg = null;

        AssertValue(arg);
        AssertValue(LocalValue.ConvertFrom(arg));

        static void AssertValue(LocalValue value)
        {
            Assert.That(value, Is.TypeOf<NullLocalValue>());
        }
    }

    [Test]
    public void CanConvertTrueToLocalValue()
    {
        AssertValue(true);

        AssertValue(LocalValue.ConvertFrom(true));

        static void AssertValue(LocalValue value)
        {
            Assert.That(value, Is.TypeOf<BooleanLocalValue>());
            Assert.That((value as BooleanLocalValue).Value, Is.True);
        }
    }

    [Test]
    public void CanConvertFalseToLocalValue()
    {
        AssertValue(false);

        AssertValue(LocalValue.ConvertFrom(false));

        static void AssertValue(LocalValue value)
        {
            Assert.That(value, Is.TypeOf<BooleanLocalValue>());
            Assert.That((value as BooleanLocalValue).Value, Is.False);
        }
    }

    [Test]
    public void CanConvertNullIntToLocalValue()
    {
        int? arg = null;

        AssertValue(arg);

        AssertValue(LocalValue.ConvertFrom(arg));

        static void AssertValue(LocalValue value)
        {
            Assert.That(value, Is.TypeOf<NullLocalValue>());
        }
    }

    [Test]
    public void CanConvertZeroIntToLocalValue()
    {
        LocalValue arg = 0;

        AssertValue(arg);

        AssertValue(LocalValue.ConvertFrom(0));

        static void AssertValue(LocalValue value)
        {
            Assert.That(value, Is.TypeOf<NumberLocalValue>());
            Assert.That((value as NumberLocalValue).Value, Is.Zero);
        }
    }

    [Test]
    public void CanConvertNullDoubleToLocalValue()
    {
        double? arg = null;

        AssertValue(arg);

        AssertValue(LocalValue.ConvertFrom(arg));

        static void AssertValue(LocalValue value)
        {
            Assert.That(value, Is.TypeOf<NullLocalValue>());
        }
    }

    [Test]
    public void CanConvertZeroDoubleToLocalValue()
    {
        double arg = 0;

        AssertValue(arg);

        AssertValue(LocalValue.ConvertFrom(0));

        static void AssertValue(LocalValue value)
        {
            Assert.That(value, Is.TypeOf<NumberLocalValue>());
            Assert.That((value as NumberLocalValue).Value, Is.Zero);
        }
    }

    [Test]
    public void CanConvertNullStringToLocalValue()
    {
        string arg = null;

        AssertValue(arg);

        AssertValue(LocalValue.ConvertFrom(arg));

        static void AssertValue(LocalValue value)
        {
            Assert.That(value, Is.TypeOf<NullLocalValue>());
        }
    }

    [Test]
    public void CanConvertStringToLocalValue()
    {
        AssertValue("value");

        AssertValue(LocalValue.ConvertFrom("value"));

        static void AssertValue(LocalValue value)
        {
            Assert.That(value, Is.TypeOf<StringLocalValue>());
            Assert.That((value as StringLocalValue).Value, Is.EqualTo("value"));
        }
    }

    [Test]
    public void CanConvertObjectValue()
    {
        var arg = new
        {
            UIntNumber = 5u,
            Array = new int[] { 1, 2 },
            List = new List<string> { "a", "b" }
        };

        var value = LocalValue.ConvertFrom(arg);

        Console.WriteLine(value);

        Assert.That(value, Is.TypeOf<ObjectLocalValue>());

        var objValue = value as ObjectLocalValue;

        Assert.That(objValue.Value, Has.Exactly(3).Count);
    }
}
