using Newtonsoft.Json;

namespace OpenAI.ChatGPT.Net.DataModels
{
    public record Tool(
     [property: JsonProperty("type")] string Type,
     [property: JsonProperty("function")] ToolFunction Function
    );
}
