using Newtonsoft.Json;

namespace OpenAI.ChatGPT.Net.DataModels
{
    public record ToolCall(
        [property: JsonProperty("id")] string Id,
        [property: JsonProperty("type")] string Type,
        [property: JsonProperty("function")] ToolCallFunction Function
    );
}
