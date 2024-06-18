using Newtonsoft.Json;

namespace OpenAI.ChatGPT.Net.DataModels
{
    public record ChatChoice(
        [property: JsonProperty("message")] ChatMessage Message,
        [property: JsonProperty("finish_reason")] string FinishReason,
        [property: JsonProperty("index")] long Index,
        [property: JsonProperty("logprobs")] LogProbs LogProbs
    );
}
