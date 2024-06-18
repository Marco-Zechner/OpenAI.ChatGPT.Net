using Newtonsoft.Json;

namespace OpenAI.ChatGPT.Net.DataModels
{
    public record StreamOptions(
        [property: JsonProperty("include_usage")] bool IncludeUsage
    );
}
