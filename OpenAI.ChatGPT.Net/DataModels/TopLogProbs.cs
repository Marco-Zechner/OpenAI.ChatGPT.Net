using Newtonsoft.Json;

namespace OpenAI.ChatGPT.Net.DataModels
{
    public record TopLogProbs(
        [property: JsonProperty("token")] string Token,
        [property: JsonProperty("logprobs")] double Logprobs,
        [property: JsonProperty("bytes")] List<int> Bytes
    );
}
