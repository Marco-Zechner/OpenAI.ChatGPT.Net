using Newtonsoft.Json;
using OpenAI.ChatGPT.Net.JsonConverters;

namespace OpenAI.ChatGPT.Net.DataModels
{
    /// <summary>
    /// Represents a request to the ChatGPT API.
    /// <see href="https://platform.openai.com/docs/api-reference/chat/create"/>
    /// </summary>
    [JsonConverter(typeof(ChatGPTRequestConverter))]
    public record ChatGPTRequest(
                [property: JsonProperty("messages")] List<ChatMessage> Messages,
                [property: JsonProperty("model")] string Model,
                [property: JsonProperty("frequency_penalty")] double? FrequencyPenalty = null,
                [property: JsonProperty("max_tokens")] long? MaxTokens = null,
                [property: JsonProperty("n")] long? N = null,
                [property: JsonProperty("presence_penalty")] double? PresencePenalty = null,
                [property: JsonProperty("stop")] string[]? Stop = null,
                [property: JsonProperty("stream")] bool? Stream = null,
                [property: JsonProperty("stream_options")] StreamOptions? StreamOptions = null,
                [property: JsonProperty("temperature")] double? Temperature = null,
                [property: JsonProperty("top_p")] double? TopP = null,
                [property: JsonProperty("tools")] List<Tool>? Tools = null,
                [property: JsonProperty("tool_choice")] ToolChoice? ToolChoice = null,
                [property: JsonProperty("parallel_tool_calls")] bool? ParallelToolCalls = null,
                [property: JsonProperty("user")] string? User = null
            )
    {
        public override int GetHashCode()
        {
            // Use a hash code combining all properties
            var one = HashCode.Combine(Messages, Model, FrequencyPenalty, MaxTokens, N, PresencePenalty);
            var two = HashCode.Combine(Stop, Stream, StreamOptions, Temperature, TopP, Tools, ToolChoice);
            return HashCode.Combine(one, two);
        }
    }

    public record StreamOptions(
        [property: JsonProperty("include_usage")] bool IncludeUsage
    );

    public record ToolChoice(
        [property: JsonProperty("tool_choice")] string? Choice, 
        [property: JsonProperty("function", DefaultValueHandling = DefaultValueHandling.Ignore)] SimpleTool? Tool
    )
    {
        public ToolChoice() : this(null, null) { }

        public ToolChoice(string choice) : this(choice, null)
        {
        }

        public ToolChoice(SimpleTool tool) : this(null, tool)
        {
        }
    }

    public record SimpleTool(
        [property: JsonProperty("type")] string Type,
         [property: JsonProperty("function")] SimpleToolFunction Function
    );

    public record SimpleToolFunction(
        [property: JsonProperty("name")] string Name
    );
}
