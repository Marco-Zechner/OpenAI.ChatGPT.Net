using Newtonsoft.Json;

namespace OpenAI.ChatGPT.Net.DataModels
{
    public record TokenUsage(
        [property: JsonProperty("prompt_tokens")] long PromptTokens,
        [property: JsonProperty("completion_tokens")] long CompletionTokens,
        [property: JsonProperty("total_tokens")] long TotalTokens
    );
}
