using Newtonsoft.Json;

namespace OpenAI.ChatGPT.Net.DataModels
{
    public record ParameterDetail(
    [property: JsonProperty("type")] string Type,
    [property: JsonProperty("description")] string Description,
    [property: JsonProperty("enum")] List<string> Enum
);
}
