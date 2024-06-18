using Newtonsoft.Json;

namespace OpenAI.ChatGPT.Net.DataModels
{
    public record ToolCallDetail(
        [property: JsonProperty("id")] string ID,
        [property: JsonProperty("type")] string Type,
        [property: JsonProperty("function")] FunctionCallDetail Function
    );

}
