using Newtonsoft.Json;

namespace OpenAI.ChatGPT.Net.DataModels
{
    public record LogProbs(
        [property: JsonProperty("content")] List<TokenLogProbs> Content
    );
}
