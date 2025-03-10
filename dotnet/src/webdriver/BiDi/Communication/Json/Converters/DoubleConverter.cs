// <copyright file="DoubleConverter.cs" company="Selenium Committers">
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
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenQA.Selenium.BiDi.Communication.Json.Converters
{
    internal class DoubleConverter : JsonConverter<double>
    {
        public override double Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TryGetDouble(out double d))
            {
                return d;
            }

            var str = reader.GetString() ?? throw new JsonException();

            if (str.Equals("-0", System.StringComparison.Ordinal))
            {
                return -0.0;
            }
            else if (str.Equals("NaN", System.StringComparison.Ordinal))
            {
                return double.NaN;
            }
            else if (str.Equals("Infinity", System.StringComparison.Ordinal))
            {
                return double.PositiveInfinity;
            }
            else if (str.Equals("-Infinity", System.StringComparison.Ordinal))
            {
                return double.NegativeInfinity;
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
        {
            if (double.IsNaN(value))
            {
                writer.WriteStringValue("NaN");
            }
            else if (double.IsPositiveInfinity(value))
            {
                writer.WriteStringValue("Infinity");
            }
            else if (double.IsNegativeInfinity(value))
            {
                writer.WriteStringValue("-Infinity");
            }
            else if (IsNegativeZero(value))
            {
                writer.WriteStringValue("-0");
            }
            else
            {
                writer.WriteNumberValue(value);
            }

            static bool IsNegativeZero(double x)
            {
                const long NegativeZeroBits = -9223372036854775808;

                return BitConverter.DoubleToInt64Bits(x) == NegativeZeroBits;
            }
        }
    }
}
