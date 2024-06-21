using Newtonsoft.Json;

namespace OpenAI.ChatGPT.Net.DataModels
{
    public record ToolParameters(
        [property: JsonProperty("type")] string Type,
        [property: JsonProperty("properties")] Dictionary<string, ParameterDetail> Properties,
        [property: JsonProperty("required")] List<string> Required
    );
}
