using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI.ChatGPT.Net.DataModels;

namespace OpenAI.ChatGPT.Net.JsonConverters
{
    public class ChatGPTRequestConverter : JsonConverter<ChatGPTRequest>
    {
        private JToken GetOrDefaultToNull<T>(T value, T defaultToNull) => value == null || EqualityComparer<T>.Default.Equals(value, defaultToNull) ? JValue.CreateNull() : JToken.FromObject(value, serializer);

        private JToken GetOrDefaultToNull<T>(T value, Func<T, bool> nullCondition) => value == null || nullCondition(value) ? JValue.CreateNull() : JToken.FromObject(value, serializer);

        private JsonSerializer serializer = new();
        
        public override void WriteJson(JsonWriter writer, ChatGPTRequest? value, JsonSerializer serializer)
        {
            if (value == null) throw new JsonWriterException(nameof(value) + " to write is null");

            this.serializer = serializer;

            var jsonObject = new JObject
            {
                ["messages"]            = JArray.FromObject(value.Messages ?? []),
                ["model"]               = value.Model ?? "no model set",
                ["frequency_penalty"]   = GetOrDefaultToNull(value.FrequencyPenalty, 0.0),
                ["logit_bias"]          = GetOrDefaultToNull(value.LogitBias, lb => !(lb?.Count > 0)),
                ["logprobs"]            = GetOrDefaultToNull(value.Logprobs, false),
                ["top_logprobs"] = (value.Logprobs == null || value.Logprobs == false) ? JValue.CreateNull()
                                        : GetOrDefaultToNull(value.TopLogprobs, 0),
                ["max_tokens"]          = GetOrDefaultToNull(value.MaxTokens, 0),
                ["n"]                   = GetOrDefaultToNull(value.N, 1),
                ["presence_penalty"]    = GetOrDefaultToNull(value.PresencePenalty, 0),
                ["stop"]                = GetOrDefaultToNull(value.Stop, s => !(s?.Length > 0)),
                ["stream"]              = GetOrDefaultToNull(value.Stream, false),
                ["stream_options"]      = (value.Stream == null || value.Stream == false) ? JValue.CreateNull() 
                                        : GetOrDefaultToNull(value.StreamOptions, so => so == null || !so.IncludeUsage)
                ["temperature"]         = GetOrDefaultToNull(value.Temperature, 1),
                ["top_p"]               = GetOrDefaultToNull(value.TopP, 1),
                ["tools"]               = GetOrDefaultToNull(value.Tools, t => !(t?.Count > 0)),
                ["tool_choice"]         = !(value.Tools?.Count > 0) ? JValue.CreateNull() 
                                        : GetOrDefaultToNull(value.ToolChoice, tc => tc == null)
                ["parallel_tool_calls"] = !(value.Tools?.Count > 0) ? JValue.CreateNull() 
                                        : GetOrDefaultToNull(value.ParallelToolCalls, true),
                ["user"]                = GetOrDefaultToNull(value.User, string.IsNullOrEmpty)
            };
            

            // Handle Tools and ToolChoice NOT TESTED!!!
            if (value.Tools == null || value.Tools.Count == 0 || value.ToolChoice == null || value.ToolChoice.Choice == "none")
            {
                jsonObject.Remove("tools");
                jsonObject.Remove("tool_choice");
            }
            else
            {
                if (value.Tools.Count > 128)
                {
                    throw new ArgumentException("Tools count cannot exceed 128.");
                }

                if (value.Tools.Count > 0 && value.ToolChoice.Choice == "auto")
                {
                    jsonObject.Remove("tool_choice");
                }

                if (value.ToolChoice.Tool != null)
                {
                    // yes I actually remove all other tools, but this is just cheaper for the same functionality
                    // if I where to send all tools AND the tool_choice, it would take far more token to tell gpt to use only one specific tool
                    // in this way I only give gpt 1 tool and tell him that he must at least use 1 tool. That should be the same.
                    var toolName = value.ToolChoice.Tool.Function.Name;
                    var tool = value.Tools.FirstOrDefault(t => t.Function.Name == toolName) 
                        ?? throw new ArgumentException($"Tool '{toolName}' specified in ToolChoice is not in the Tools list.");
                    jsonObject["tools"] = new JArray { JToken.FromObject(tool, serializer) };
                    jsonObject["tool_choice"] = "required";
                }
            }

            RemoveNullProperties(jsonObject);

            jsonObject.WriteTo(writer);
        }
 
        private static void RemoveNullProperties(JObject jsonObject)
        {
            var properties = jsonObject.Properties().Where(p => p.Value.Type == JTokenType.Null).ToList();
            foreach (var prop in properties)
            {
                jsonObject.Remove(prop.Name);
            }
        }

        public override ChatGPTRequest ReadJson(JsonReader reader, Type objectType, ChatGPTRequest? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);

            var messages            = jsonObject["messages"]?           .ToObject<List<ChatMessage>>(serializer) ?? [];
            var model               = jsonObject["model"]?              .ToObject<string>(serializer) ?? throw new JsonSerializationException("Model is required.");
            var frequencyPenalty    = jsonObject["frequency_penalty"]?  .ToObject<double>(serializer) ?? 0.0;
            var logitBias           = jsonObject["logit_bias"]?         .ToObject<Dictionary<int, int>>(serializer);
            var logprobs            = jsonObject["logprobs"]?           .ToObject<bool>(serializer) ?? false;
            var topLogprobs         = jsonObject["top_logprobs"]?       .ToObject<int>(serializer) ?? 0;
            var maxTokens           = jsonObject["max_tokens"]?         .ToObject<long>(serializer) ?? 0;
            var n                   = jsonObject["n"]?                  .ToObject<long>(serializer) ?? 1;
            var presencePenalty     = jsonObject["presence_penalty"]?   .ToObject<double>(serializer) ?? 0.0;
            var stop                = jsonObject["stop"]?               .ToObject<string[]>(serializer);
            var stream              = jsonObject["stream"]?             .ToObject<bool>(serializer) ?? false;
            var streamOptions       = jsonObject["stream_options"]?     .ToObject<StreamOptions>(serializer);
            var temperature         = jsonObject["temperature"]?        .ToObject<double>(serializer) ?? 1.0;
            var topP                = jsonObject["top_p"]?              .ToObject<double>(serializer) ?? 1.0;
            var tools               = jsonObject["tools"]?              .ToObject<List<Tool>>(serializer) ?? [];

            ToolChoice? toolChoice = null;
            if (jsonObject.TryGetValue("tool_choice", out JToken? toolChoiceToken))
            {
                if (toolChoiceToken.Type == JTokenType.String)
                {
                    toolChoice = new ToolChoice(toolChoiceToken.ToString());
                }
                else if (toolChoiceToken.Type == JTokenType.Object)
                {
                    var tool = toolChoiceToken["function"]?.ToObject<Tool>(serializer);
                    if (tool != null)
                    {
                        toolChoice = new ToolChoice(tool);
                    }
                }
            }

            var parallelToolCalls = jsonObject["parallel_tool_calls"]?.ToObject<bool>(serializer) ?? true;
            var user = jsonObject["user"]?.ToObject<string>(serializer) ?? null;

            return new ChatGPTRequest(
                Messages: messages,
                Model: model,
                FrequencyPenalty: frequencyPenalty,
                LogitBias: logitBias,
                Logprobs: logprobs,
                TopLogprobs: topLogprobs,
                MaxTokens: maxTokens,
                N: n,
                PresencePenalty: presencePenalty,
                Stop: stop,
                Stream: stream,
                StreamOptions: streamOptions,
                Temperature: temperature,
                TopP: topP,
                Tools: tools,
                ToolChoice: toolChoice ?? new ToolChoice("none"),
                ParallelToolCalls: parallelToolCalls,
                User: user
            );
        }
    }
}
