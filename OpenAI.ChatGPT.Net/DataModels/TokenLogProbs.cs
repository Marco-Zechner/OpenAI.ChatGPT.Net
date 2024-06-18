using Newtonsoft.Json;

namespace OpenAI.ChatGPT.Net.DataModels
{
    public record TokenLogProbs(
        [property: JsonProperty("token")] string Token,
        [property: JsonProperty("logprobs")] double Logprobs,
        [property: JsonProperty("bytes")] List<int> Bytes,
        [property: JsonProperty("top_logprobs")] List<TopLogProbs> TopLogProbs
    );
}
