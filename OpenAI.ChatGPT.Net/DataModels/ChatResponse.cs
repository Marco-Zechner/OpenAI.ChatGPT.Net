using Newtonsoft.Json;

namespace OpenAI.ChatGPT.Net.DataModels
{
    public record ChatResponse(
        [property: JsonProperty("id")] string Id,
        [property: JsonProperty("choices")] List<ChatChoice> Choices,
        [property: JsonProperty("created")] long Created,
        [property: JsonProperty("model")] string Model,
        // scale_tier? Can't find info about it: https://platform.openai.com/docs/api-reference/chat/object
        [property: JsonProperty("system_fingerprint")] string SystemFingerprint,
        [property: JsonProperty("object")] string Object, // always = chat.completion
        [property: JsonProperty("usage")] TokenUsage? Usage,
        [property: JsonProperty("error")] ChatError? Error,
        [property: JsonIgnore] DateTime? Timestamp
    );

    // technically I can also just use ChatResponse
    public record ChatResponseChunk(
        [property: JsonProperty("id")] string Id,
        [property: JsonProperty("choices")] List<ChatChoice> Choices,
        [property: JsonProperty("created")] long Created,
        [property: JsonProperty("model")] string Model,
        [property: JsonProperty("system_fingerprint")] string SystemFingerprint,
        [property: JsonProperty("object")] string Object, // always = chat.completion.chunk
        [property: JsonProperty("usage")] TokenUsage Usage,
        [property: JsonProperty("error")] ChatError? Error
    );
}
