using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI.ChatGPT.Net.DataModels;
using OpenAI.ChatGPT.Net.Interfaces;

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
                ["messages"]            = new JArray(value.Messages.Select(msg => msg is ChatMessage chatMsg ?
                                            JObject.FromObject(chatMsg, serializer) :
                                            JObject.FromObject((ToolCallResponse)msg, serializer))),
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
                ["parallel_tool_calls"] = !(value.Tools?.Count > 0) ? JValue.CreateNull() 
                                        : GetOrDefaultToNull(value.ParallelToolCalls, true),
                ["user"]                = GetOrDefaultToNull(value.User, string.IsNullOrEmpty)
            };


            if (value.Tools != null && value.Tools.Count > 0)
            {
                if (value.Tools.Count > 128)
                {
                    throw new ArgumentException("Tools count cannot exceed 128.");
                }

                if (value.ToolChoice != null)
                {
                    if (!string.IsNullOrEmpty(value.ToolChoice.Choice))
                    {
                        var toolChoiceObject = new JObject
                        {
                            ["tool_choice"] = value.ToolChoice.Choice
                        };
                        jsonObject["tool_choice"] = toolChoiceObject;
                    }
                    else if (value.ToolChoice.Tool != null)
                    {
                        var toolChoiceObject = new JObject
                        {

                            ["function"] = value.ToolChoice.Tool != null ? new JObject
                            {
                                ["type"] = value.ToolChoice.Tool.Type,
                                ["function"] = new JObject
                                {
                                    ["name"] = value.ToolChoice.Tool.Function.Name
                                }
                            } : JValue.CreateNull()
                        };
                        jsonObject["tool_choice"] = toolChoiceObject;
                    }
                }

                var toolsArray = new JArray();
                foreach (var tool in value.Tools)
                {
                    var toolObject = new JObject
                    {
                        ["type"] = tool.Type,
                        ["function"] = new JObject
                        {
                            ["name"] = tool.Function.Name,
                            ["description"] = tool.Function.Description,
                            ["parameters"] = new JObject
                            {
                                ["type"] = tool.Function.Parameters.Type,
                                ["properties"] = new JObject(
                                    tool.Function.Parameters.Properties.Select(p =>
                                        new JProperty(p.Key, new JObject
                                        {
                                            ["type"] = p.Value.Type,
                                            ["description"] = p.Value.Description,
                                            ["enum"] = p.Value.Enum != null ? JArray.FromObject(p.Value.Enum) : null
                                        })
                                    )
                                ),
                                ["required"] = JArray.FromObject(tool.Function.Parameters.Required)
                            }
                        }
                    };
                    toolsArray.Add(toolObject);
                }
                jsonObject["tools"] = toolsArray;
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

            var messages = jsonObject["messages"]?.Children().Select(token =>
            {
                var role = token["role"]?.ToString();
                return role switch
                {
                    "user" or "assistant" => token.ToObject<ChatMessage>(serializer) as IMessage,
                    "tool" => token.ToObject<ToolCallResponse>(serializer) as IMessage,
                    _ => throw new JsonSerializationException($"Unknown message role: {role}")
                };
            }).ToList() ?? []; var model               = jsonObject["model"]?              .ToObject<string>(serializer) ?? throw new JsonSerializationException("Model is required.");
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
