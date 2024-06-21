using Newtonsoft.Json;

namespace OpenAI.ChatGPT.Net.DataModels
{
    public record ToolFunction(
        [property: JsonProperty("name")] string Name,
        [property: JsonProperty("description")] string Description,
        [property: JsonProperty("parameters")] ToolParameters Parameters
    );
}
