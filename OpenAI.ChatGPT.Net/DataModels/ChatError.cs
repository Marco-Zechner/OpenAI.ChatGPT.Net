using Newtonsoft.Json;

namespace OpenAI.ChatGPT.Net.DataModels
{
    public record ChatError(
        [property: JsonProperty("message")] string Message,
        [property: JsonProperty("type")] string Type,
        [property: JsonProperty("param")] object Param,
        [property: JsonProperty("code")] string Code
    );
}
