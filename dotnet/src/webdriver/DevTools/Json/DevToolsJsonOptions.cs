using System.ComponentModel;
using System.Text.Json;

#nullable enable

namespace OpenQA.Selenium.DevTools.Json;

[EditorBrowsable(EditorBrowsableState.Never)] // Generated code use only
internal static class DevToolsJsonOptions
{
    public static JsonSerializerOptions DevToolsSerializerOptions { get; } = new JsonSerializerOptions()
    {
        Converters =
        {
            new StringConverter(),
        }
    };
}
