using Newtonsoft.Json;

namespace OpenAI.ChatGPT.Net.DataModels
{
    public record ChatResponse(
        [property: JsonProperty("id")] string Id,
        [property: JsonProperty("object")] string Object,
        [property: JsonProperty("created")] long Created,
        [property: JsonProperty("model")] string Model,
        [property: JsonProperty("usage")] TokenUsage Usage,
        [property: JsonProperty("choices")] List<ChatChoice> Choices,
        [property: JsonProperty("error")] ChatError? Error
    );
}
