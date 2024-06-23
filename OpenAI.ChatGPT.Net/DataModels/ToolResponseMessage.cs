using Newtonsoft.Json;
using OpenAI.ChatGPT.Net.Tools;

namespace OpenAI.ChatGPT.Net.DataModels
{
    public record ToolResponseMessage(
        [param: JsonProperty("role")] ChatRole Role,
        [param: JsonProperty("content")] string Content,
        [property: JsonProperty("tool_call_id")] string ToolCallID,
        [property: JsonProperty("name")] string Name
    ) : ChatMessage(Role, Content);
}
