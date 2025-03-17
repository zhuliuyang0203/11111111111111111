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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

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
    public static implicit operator LocalValue(bool? value) { return value is bool b ? (b ? True : False) : Null; }
    public static implicit operator LocalValue(int? value) { return value is int i ? Number(i) : Null; }
    public static implicit operator LocalValue(double? value) { return value is double d ? Number(d) : Null; }
    public static implicit operator LocalValue(string? value) { return value is null ? Null : String(value); }

    // TODO: Extend converting from types
    public static LocalValue ConvertFrom(object? value)
    {
        switch (value)
        {
            case LocalValue localValue:
                return localValue;

            case null:
                return Null;

            case bool b:
                return b ? True : False;

            case int i:
                return Number(i);

            case double d:
                return Number(d);

            case string str:
                return String(str);

            case IEnumerable<object?> list:
                return Array(list.Select(ConvertFrom).ToList());

            case object:
                {
                    var type = value.GetType();

                    var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                    List<List<LocalValue>> values = [];

                    foreach (var property in properties)
                    {
                        values.Add([property.Name, ConvertFrom(property.GetValue(value))]);
                    }

                    return Object(values);
                }
        }
    }

    private static readonly BigInteger MaxDouble = new BigInteger(double.MaxValue);
    private static readonly BigInteger MinDouble = new BigInteger(double.MinValue);

    public static LocalValue ConvertFrom(JsonNode? node)
    {
        if (node is null)
        {
            return Null;
        }

        switch (node.GetValueKind())
        {
            case System.Text.Json.JsonValueKind.Null:
                return Null;

            case System.Text.Json.JsonValueKind.True:
                return True;

            case System.Text.Json.JsonValueKind.False:
                return False;

            case System.Text.Json.JsonValueKind.String:
                return String(node.ToString());

            case System.Text.Json.JsonValueKind.Number:
                {
                    var numberString = node.ToString();

                    var bigNumber = BigInteger.Parse(numberString);

                    if (bigNumber > MaxDouble || bigNumber < MinDouble)
                    {
                        return BigInt(bigNumber);
                    }

                    return Number(double.Parse(numberString));
                }

            case System.Text.Json.JsonValueKind.Array:
                return Array(node.AsArray().Select(ConvertFrom));

            case System.Text.Json.JsonValueKind.Object:
                var convertedToListForm = node.AsObject().Select(property => new LocalValue[] { String(property.Key), ConvertFrom(property.Value) }).ToList();
                return Object(convertedToListForm);

            default:
                throw new InvalidOperationException("Invalid JSON node");
        }
    }

    public static ChannelLocalValue Channel(ChannelLocalValue.ChannelProperties options)
    {
        return new ChannelLocalValue(options);
    }

    public static ArrayLocalValue Array(IEnumerable<LocalValue> values)
    {
        return new ArrayLocalValue(values);
    }

    public static SetLocalValue Set(HashSet<LocalValue> values)
    {
        return new SetLocalValue(values);
    }

    public static ObjectLocalValue Object(IEnumerable<IEnumerable<LocalValue>> values)
    {
        return new ObjectLocalValue(values);
    }

    public static ObjectLocalValue Object(IDictionary<string, LocalValue> values)
    {
        var convertedValues = values.Select(pair => new LocalValue[] { new StringLocalValue(pair.Key), pair.Value }).ToList();
        return new ObjectLocalValue(convertedValues);
    }

    public static MapLocalValue Map(IEnumerable<IEnumerable<LocalValue>> values)
    {
        return new MapLocalValue(values);
    }

    public static MapLocalValue Map(IDictionary<LocalValue, LocalValue> values)
    {
        var convertedValues = values.Select(PairToList).ToList();
        return new MapLocalValue(convertedValues);
    }

    private static LocalValue[] PairToList(KeyValuePair<LocalValue, LocalValue> pair)
    {
        return [pair.Key, pair.Value];
    }

    public static BigIntLocalValue BigInt(BigInteger value)
    {
        return new BigIntLocalValue(value.ToString());
    }

    public static DateLocalValue Date(DateTime value)
    {
        return new DateLocalValue(value.ToString("o"));
    }

    public static StringLocalValue String(string value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), $"string value cannot be null, use {nameof(LocalValue)}.{nameof(Null)} value instead");
        }

        return new StringLocalValue(value);
    }

    public static NumberLocalValue Number(double value)
    {
        return new NumberLocalValue(value);
    }

    public static BooleanLocalValue True { get; } = new BooleanLocalValue(true);

    public static BooleanLocalValue False { get; } = new BooleanLocalValue(false);

    public static NullLocalValue Null { get; } = new NullLocalValue();

    public static UndefinedLocalValue Undefined { get; } = new UndefinedLocalValue();

    /// <summary>
    /// Converts a .NET Regex into a BiDi Regex
    /// </summary>
    /// <param name="regex">A .NET Regex.</param>
    /// <returns>A BiDi Regex.</returns>
    /// <remarks>
    /// Note that the .NET regular expression engine does not work the same as the JavaScript engine.
    /// To minimize the differences between the two engines, it is recommended to enabled the <see cref="RegexOptions.ECMAScript"/> option.
    /// </remarks>
    public static RegExpLocalValue Regex(Regex regex)
    {
        RegexOptions options = regex.Options;

        if (options == RegexOptions.None)
        {
            return new RegExpLocalValue(new RegExpValue(regex.ToString()));
        }

        string flags = string.Empty;

        const RegexOptions NonBacktracking = (RegexOptions)1024;
#if NET8_0_OR_GREATER
        Debug.Assert(NonBacktracking == RegexOptions.NonBacktracking);
#endif

        const RegexOptions NonApplicableOptions = RegexOptions.Compiled | NonBacktracking;

        const RegexOptions UnsupportedOptions =
            RegexOptions.ExplicitCapture |
            RegexOptions.IgnorePatternWhitespace |
            RegexOptions.RightToLeft |
            RegexOptions.CultureInvariant;

        options &= ~NonApplicableOptions;

        if ((options & UnsupportedOptions) != 0)
        {
            throw new NotSupportedException($"The selected RegEx options are not supported in BiDi: {options & UnsupportedOptions}");
        }

        if ((options & RegexOptions.IgnoreCase) != 0)
        {
            flags += "i";
            options = options & ~RegexOptions.IgnoreCase;
        }

        if ((options & RegexOptions.Multiline) != 0)
        {
            options = options & ~RegexOptions.Multiline;
            flags += "m";
        }

        if ((options & RegexOptions.Singleline) != 0)
        {
            options = options & ~RegexOptions.Singleline;
            flags += "s";
        }

        Debug.Assert(options == RegexOptions.None);

        return new RegExpLocalValue(new RegExpValue(regex.ToString()) { Flags = flags });
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

public record ChannelLocalValue(ChannelLocalValue.ChannelProperties Value) : LocalValue
{
    // TODO: Revise why we need it
    [JsonInclude]
    internal string type = "channel";

    public record ChannelProperties(Channel Channel)
    {
        public SerializationOptions? SerializationOptions { get; set; }

        public ResultOwnership? Ownership { get; set; }
    }
}

public record ArrayLocalValue(IEnumerable<LocalValue> Value) : LocalValue;

public record DateLocalValue(string Value) : LocalValue;

public record MapLocalValue(IEnumerable<IEnumerable<LocalValue>> Value) : LocalValue;

public record ObjectLocalValue(IEnumerable<IEnumerable<LocalValue>> Value) : LocalValue;

public record RegExpLocalValue(RegExpValue Value) : LocalValue;

public record SetLocalValue(IEnumerable<LocalValue> Value) : LocalValue;
