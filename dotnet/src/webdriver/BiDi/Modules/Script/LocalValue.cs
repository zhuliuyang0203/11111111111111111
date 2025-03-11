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
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace OpenQA.Selenium.BiDi.Modules.Script;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(Boolean), "boolean")]
[JsonDerivedType(typeof(BigInt), "bigint")]
[JsonDerivedType(typeof(Number), "number")]
[JsonDerivedType(typeof(String), "string")]
[JsonDerivedType(typeof(Null), "null")]
[JsonDerivedType(typeof(Undefined), "undefined")]
[JsonDerivedType(typeof(Channel), "channel")]
[JsonDerivedType(typeof(Array), "array")]
[JsonDerivedType(typeof(Date), "date")]
[JsonDerivedType(typeof(Map), "map")]
[JsonDerivedType(typeof(Object), "object")]
[JsonDerivedType(typeof(RegExp), "regexp")]
[JsonDerivedType(typeof(Set), "set")]
public abstract record LocalValue
{
    public static implicit operator LocalValue(int value) { return new Number(value); }
    public static implicit operator LocalValue(string? value) { return value is null ? new Null() : new String(value); }

    // TODO: Extend converting from types
    public static LocalValue ConvertFrom(object? value)
    {
        switch (value)
        {
            case LocalValue:
                return (LocalValue)value;
            case null:
                return new Null();
            case int:
                return (int)value;
            case string:
                return (string)value;
            case object:
                {
                    var type = value.GetType();

                    var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                    List<List<LocalValue>> values = [];

                    foreach (var property in properties)
                    {
                        values.Add([property.Name, ConvertFrom(property.GetValue(value))]);
                    }

                    return new Object(values);
                }
        }
    }

    public static LocalValue FromNode(JsonNode? node)
    {
        if (node is null)
        {
            return new Null();
        }

        switch (node.GetValueKind())
        {
            case JsonValueKind.True:
                return new Boolean(true);

            case JsonValueKind.False:
                return new Boolean(false);

            case JsonValueKind.Number:
                {
                    JsonValue value = node.AsValue();
                    if (value.TryGetValue(out int intValue))
                    {
                        return new Number(intValue);
                    }

                    if (value.TryGetValue(out double doubleValue) && !double.IsInfinity(doubleValue))
                    {
                        return new Number(doubleValue);
                    }

                    return new BigInt(BigInteger.Parse(value.ToJsonString()));
                }

            case JsonValueKind.String:
                return new String(node.GetValue<string>());

            case JsonValueKind.Array:
                return new Array(node.AsArray().Select(FromNode));

            case JsonValueKind.Object:
                return new Map(node.AsObject().ToDictionary(m => m.Key, m => FromNode(m.Value)));

            default:
                throw new InvalidCastException($"Could not convert node {node}");
        }
    }

    public abstract record PrimitiveProtocolLocalValue : LocalValue
    {

    }

    public record BigInt : PrimitiveProtocolLocalValue
    {
        [JsonIgnore]
        public BigInteger Value { get; }

        [JsonInclude]
        [JsonPropertyName("value")]
        public string ValueAsString => Value.ToString();

        public BigInt(BigInteger value)
        {
            Value = value;
        }

        [JsonConstructor]
        internal BigInt(string valueAsString)
        {
            Value = BigInteger.Parse(valueAsString);
        }
    }

    public record Boolean(bool Value) : PrimitiveProtocolLocalValue;

    public record Number(double Value) : PrimitiveProtocolLocalValue
    {
        public static explicit operator Number(double n) => new Number(n);
        public static Number PositiveInfinity { get; } = new Number(double.PositiveInfinity);
        public static Number NegativeInfinity { get; } = new Number(double.NegativeInfinity);
        public static Number NaN { get; } = new Number(double.NaN);
    }

    public record String(string Value) : PrimitiveProtocolLocalValue;

    public record Null : PrimitiveProtocolLocalValue;

    public record Undefined : PrimitiveProtocolLocalValue;

    public record Channel(Channel.ChannelProperties Value) : LocalValue
    {
        [JsonInclude]
        internal string type = "channel";

        public record ChannelProperties(Script.Channel Channel)
        {
            public SerializationOptions? SerializationOptions { get; set; }

            public ResultOwnership? Ownership { get; set; }
        }
    }

    public record Array(IEnumerable<LocalValue> Value) : LocalValue;

    public record Date(string Value) : LocalValue
    {
        public static Date FromDateTime(DateTime value)
        {
            return new Date(value.ToString("o"));
        }
    }

    public record Map(IDictionary<string, LocalValue> Value) : LocalValue; // seems to implement IDictionary

    public record Object(IEnumerable<IEnumerable<LocalValue>> Value) : LocalValue
    {
        public static Object FromDictionary(IDictionary<string, LocalValue> values)
        {
            return new Object([.. values.Select(PairAsList)]);
        }

        private static IEnumerable<LocalValue> PairAsList(KeyValuePair<string, LocalValue> pair)
        {
            return [new String(pair.Key), pair.Value ?? new Null()];
        }
    }

    public record RegExp(RegExp.RegExpValue Value) : LocalValue
    {
        public record RegExpValue(string Pattern)
        {
            public string? Flags { get; set; }
        }

        /// <summary>
        /// Converts a .NET Regex into a BiDi Regex
        /// </summary>
        /// <param name="regex">A .NET Regex.</param>
        /// <returns>A BiDi Regex.</returns>
        /// <remarks>
        /// Note that the .NET regular expression engine does not work the same as the JavaScript engine.
        /// To minimize the differences between the two engines, it is recommended to enabled the <see cref="RegexOptions.ECMAScript"/> option.
        /// </remarks>
        public static RegExp FromRegex(Regex regex)
        {
            RegexOptions options = regex.Options;

            if (options == RegexOptions.None)
            {
                return new RegExp(new RegExpValue(regex.ToString()));
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

            return new RegExp(new RegExpValue(regex.ToString()) { Flags = flags });

        }
    }

    public record Set(IEnumerable<LocalValue> Value) : LocalValue;
}
