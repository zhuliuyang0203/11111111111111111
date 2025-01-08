using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

#nullable enable

namespace OpenQA.Selenium.DevTools.Json;

internal sealed class StringConverter : JsonConverter<string>
{
    public override bool HandleNull => true;

    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            return reader.GetString();
        }
        catch (InvalidOperationException)
        {
            // CDP sometimes sends invalid surrogate pairs on file upload

            var bytes = reader.ValueSpan;
            var sb = new StringBuilder(bytes.Length);
            foreach (byte b in bytes)
            {
                sb.Append(Convert.ToChar(b));
            }

            return sb.ToString();
        }
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value);
}