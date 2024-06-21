using Newtonsoft.Json;

namespace OpenAI.ChatGPT.Net.DataModels
{
    public record ToolChoice(
        [property: JsonProperty("tool_choice")] string? Choice, 
        [property: JsonProperty("function", DefaultValueHandling = DefaultValueHandling.Ignore)] Tool? Tool
    )
    {
        public ToolChoice() : this(null, null) { }

        public ToolChoice(string choice) : this(choice, null) { }

        public ToolChoice(Tool tool) : this(null, tool) { }
    }
}
