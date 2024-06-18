using Newtonsoft.Json;

namespace OpenAI.ChatGPT.Net.DataModels
{
    public record ToolCallSummary(
        [property: JsonProperty("tool_calls", DefaultValueHandling = DefaultValueHandling.Ignore)] List<ToolCallDetail> ToolCalls
    ) : ChatMessage(ChatRole.Assistant, null);

}
