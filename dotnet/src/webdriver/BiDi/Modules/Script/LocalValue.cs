// <copyright file="LocalValue.cs" company="Selenium Committers">
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;

namespace OpenQA.Selenium.BiDi.Modules.Script;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(NumberLocalValue), "number")]
[JsonDerivedType(typeof(StringLocalValue), "string")]
[JsonDerivedType(typeof(NullLocalValue), "null")]
[JsonDerivedType(typeof(UndefinedLocalValue), "undefined")]
[JsonDerivedType(typeof(BooleanLocalValue), "boolean")]
[JsonDerivedType(typeof(BigIntLocalValue), "bigint")]
[JsonDerivedType(typeof(ChannelLocalValue), "channel")]
[JsonDerivedType(typeof(ArrayLocalValue), "array")]
[JsonDerivedType(typeof(DateLocalValue), "date")]
[JsonDerivedType(typeof(MapLocalValue), "map")]
[JsonDerivedType(typeof(ObjectLocalValue), "object")]
[JsonDerivedType(typeof(RegExpLocalValue), "regexp")]
[JsonDerivedType(typeof(SetLocalValue), "set")]
public abstract record LocalValue
{
    public static implicit operator LocalValue(bool? value) { return ConvertFrom(value); }
    public static implicit operator LocalValue(int? value) { return ConvertFrom(value); }
    public static implicit operator LocalValue(double? value) { return ConvertFrom(value); }
    public static implicit operator LocalValue(string? value) { return ConvertFrom(value); }

    // TODO: Extend converting from types
    public static LocalValue ConvertFrom(object? value)
    {
        switch (value)
        {
            case LocalValue localValue:
                return localValue;

            case null:
                return new NullLocalValue();

            case bool b:
                return ConvertFrom(b);

            case int i:
                return ConvertFrom(i);

            case double d:
                return ConvertFrom(d);

            case long l:
                return ConvertFrom(l);

            case DateTime dt:
                return ConvertFrom(dt);

            case BigInteger bigInt:
                return ConvertFrom(bigInt);

            case string str:
                return ConvertFrom(str);

            case IDictionary<string, string?> dictionary:
                return ConvertFrom(dictionary);

            case IDictionary<string, object?> dictionary:
                return ConvertFrom(dictionary);

            case IDictionary<int, object?> dictionary:
                return ConvertFrom(dictionary);

            case ISet<object?> set:
                return ConvertFrom(set);

            case IList set:
                return ConvertFrom(set);

            case IEnumerable<object?> list:
                return ConvertFrom(list);

            default:
                return ReflectionBasedConvertFrom(value);
        }
    }

    public static LocalValue ConvertFrom(bool? value)
    {
        if (value is bool b)
        {
            return new BooleanLocalValue(b);
        }

        return new NullLocalValue();
    }

    public static LocalValue ConvertFrom(int? value)
    {
        if (value is int b)
        {
            return new NumberLocalValue(b);
        }

        return new NullLocalValue();
    }

    public static LocalValue ConvertFrom(double? value)
    {
        if (value is double b)
        {
            return new NumberLocalValue(b);
        }

        return new NullLocalValue();
    }

    public static LocalValue ConvertFrom(long? value)
    {
        if (value is long b)
        {
            return new NumberLocalValue(b);
        }

        return new NullLocalValue();
    }

    public static LocalValue ConvertFrom(string? value)
    {
        if (value is not null)
        {
            return new StringLocalValue(value);
        }

        return new NullLocalValue();
    }

    public static LocalValue ConvertFrom(DateTime? value)
    {
        if (value is null)
        {
            return new NullLocalValue();
        }

        return new DateLocalValue(value.Value.ToString("o"));
    }

    public static LocalValue ConvertFrom(BigInteger? value)
    {
        if (value is not null)
        {
            return new BigIntLocalValue(value.Value.ToString());
        }

        return new NullLocalValue();
    }

    public static LocalValue ConvertFrom(IEnumerable<object?>? value)
    {
        if (value is null)
        {
            return new NullLocalValue();
        }

        LocalValue[] convertedList = [.. value.Select(ConvertFrom)];
        return new ArrayLocalValue(convertedList);
    }

    public static LocalValue ConvertFrom(IList? value)
    {
        if (value is null)
        {
            return new NullLocalValue();
        }

        var type = value.GetType();

        List<LocalValue> list = [];

        foreach (var element in value)
        {
            list.Add(ConvertFrom(element));
        }

        return new ArrayLocalValue(list);
    }

    public static LocalValue ConvertFrom<TValue>(IDictionary<string, TValue?>? value)
    {
        return ConvertFrom<string, TValue>(value);
    }

    public static LocalValue ConvertFrom<TKey, TValue>(IDictionary<TKey, TValue?>? value)
    {
        if (value is null)
        {
            return new NullLocalValue();
        }

        var bidiObject = new List<List<LocalValue>>(value.Count);

        foreach (KeyValuePair<TKey, TValue?> item in value)
        {
            bidiObject.Add([ConvertFrom(item.Key), ConvertFrom(item.Value)]);
        }

        if (typeof(TKey) == typeof(string))
        {
            return new ObjectLocalValue(bidiObject);
        }

        return new MapLocalValue(bidiObject);
    }

    public static LocalValue ConvertFrom<T>(ISet<T?>? value)
    {
        if (value is null)
        {
            return new NullLocalValue();
        }

        LocalValue[] convertedValues = [.. value.Select(x => ConvertFrom(x))];

        return new SetLocalValue(convertedValues);
    }

    private static LocalValue ReflectionBasedConvertFrom(object? value)
    {
        if (value is null)
        {
            return new NullLocalValue();
        }

        const System.Reflection.BindingFlags Flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance;

        System.Reflection.PropertyInfo[] properties = value.GetType().GetProperties(Flags);

        var values = new List<List<LocalValue>>(properties.Length);

        foreach (System.Reflection.PropertyInfo? property in properties)
        {
            object? propertyValue;

            try
            {
                propertyValue = property.GetValue(value);
            }
            catch (Exception ex)
            {
                throw new BiDiException($"Could not retrieve property {property.Name} from {property.DeclaringType}", ex);
            }

            values.Add([property.Name, ConvertFrom(propertyValue)]);
        }

        return new ObjectLocalValue(values);
    }
}

public abstract record PrimitiveProtocolLocalValue : LocalValue;

public record NumberLocalValue(double Value) : PrimitiveProtocolLocalValue
{
    public static explicit operator NumberLocalValue(double n) => new NumberLocalValue(n);
}

public record StringLocalValue(string Value) : PrimitiveProtocolLocalValue;

public record NullLocalValue : PrimitiveProtocolLocalValue;

public record UndefinedLocalValue : PrimitiveProtocolLocalValue;

public record BooleanLocalValue(bool Value) : PrimitiveProtocolLocalValue;

public record BigIntLocalValue(string Value) : PrimitiveProtocolLocalValue;

public record ChannelLocalValue(ChannelProperties Value) : LocalValue
{
    // TODO: Revise why we need it
    [JsonInclude]
    internal string type = "channel";
}

public record ArrayLocalValue(IEnumerable<LocalValue> Value) : LocalValue;

public record DateLocalValue(string Value) : LocalValue;

public record MapLocalValue(IEnumerable<IEnumerable<LocalValue>> Value) : LocalValue;

public record ObjectLocalValue(IEnumerable<IEnumerable<LocalValue>> Value) : LocalValue;

public record RegExpLocalValue(RegExpValue Value) : LocalValue;

public record SetLocalValue(IEnumerable<LocalValue> Value) : LocalValue;
