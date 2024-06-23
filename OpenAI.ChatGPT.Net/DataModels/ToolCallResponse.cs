using Newtonsoft.Json;
using OpenAI.ChatGPT.Net.Tools;
using OpenAI.ChatGPT.Net.Interfaces;

namespace OpenAI.ChatGPT.Net.DataModels
{
    public record ToolCallResponse(
        [property: JsonProperty("tool_call_id")] string ToolCallId,
        [property: JsonProperty("name")] string Name,
        [property: JsonProperty("role")] ChatRole Role,
        [property: JsonProperty("content")] string? Content
    ) : IMessage;
}
