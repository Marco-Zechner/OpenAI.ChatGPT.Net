using Newtonsoft.Json;

namespace OpenAI.ChatGPT.Net.DataModels
{
    public record FunctionCallDetail(
        [property: JsonProperty("name")] string Name,
        [property: JsonProperty("arguments")] string Arguments
    );

}
