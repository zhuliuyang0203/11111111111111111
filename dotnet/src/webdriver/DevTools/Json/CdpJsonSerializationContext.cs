using System.Text.Json.Serialization;

namespace OpenQA.Selenium.DevTools.Json;

[JsonSerializable(typeof(ICommand))]
[JsonSerializable(typeof(ICommandResponse<>))]
[JsonSourceGenerationOptions(Converters = [typeof(StringConverter)])]
internal sealed partial class CdpSerializationContext : JsonSerializerContext;
