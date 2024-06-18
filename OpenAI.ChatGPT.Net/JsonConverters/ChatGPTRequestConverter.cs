using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI.ChatGPT.Net.DataModels;

namespace OpenAI.ChatGPT.Net.JsonConverters
{
    public class ChatGPTRequestConverter : JsonConverter<ChatGPTRequest>
    {
        public override void WriteJson(JsonWriter writer, ChatGPTRequest value, JsonSerializer serializer)
        {
            var jsonObject = new JObject
            {
                ["messages"] = JArray.FromObject(value.Messages ?? [], serializer),
                ["model"] = value.Model,
                ["frequency_penalty"] = value.FrequencyPenalty == 0 ? (JToken)null : value.FrequencyPenalty,
                ["max_tokens"] = value.MaxTokens <= 0 ? (JToken)null : value.MaxTokens,
                ["n"] = value.N == 1 ? (JToken)null : value.N,
                ["presence_penalty"] = value.PresencePenalty == 0 ? (JToken)null : value.PresencePenalty,
                ["stop"] = value.Stop == null ? (JToken)null : JArray.FromObject(value.Stop),
                ["stream"] = value.Stream == false ? (JToken)null : value.Stream,
                ["stream_options"] = value.Stream == false ? null : JToken.FromObject(value.StreamOptions, serializer),
                ["temperature"] = value.Temperature == 1 ? (JToken)null : value.Temperature,
                ["top_p"] = value.TopP == 1 ? (JToken)null : value.TopP
            };
            

            // Handle Tools and ToolChoice NOT TESTED!!!
            if (value.Tools == null || value.Tools.Count == 0 || value.ToolChoice.Choice == "none")
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
                    var toolName = value.ToolChoice.Tool.Function.Name;
                    var tool = value.Tools.FirstOrDefault(t => t.Function.Name == toolName);

                    if (tool == null)
                    {
                        throw new ArgumentException($"Tool '{toolName}' specified in ToolChoice is not in the Tools list.");
                    }

                    jsonObject["tools"] = new JArray { JToken.FromObject(tool, serializer) };
                }
            }

            RemoveNullProperties(jsonObject);

            jsonObject.WriteTo(writer);
        }

        public override ChatGPTRequest ReadJson(JsonReader reader, Type objectType, ChatGPTRequest existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);

            var messages = jsonObject["messages"]?.ToObject<List<ChatMessage>>(serializer) ?? new List<ChatMessage>();
            var model = jsonObject["model"]?.ToObject<string>(serializer) ?? throw new JsonSerializationException("Model is required.");
            var frequencyPenalty = jsonObject["frequency_penalty"]?.ToObject<double>(serializer) ?? 0.0;
            var maxTokens = jsonObject["max_tokens"]?.ToObject<long>(serializer) ?? 0;
            var n = jsonObject["n"]?.ToObject<long>(serializer) ?? 1;
            var presencePenalty = jsonObject["presence_penalty"]?.ToObject<double>(serializer) ?? 0.0;
            var stop = jsonObject["stop"]?.ToObject<string[]>(serializer);
            var stream = jsonObject["stream"]?.ToObject<bool>(serializer) ?? false;
            var streamOptions = jsonObject["stream_options"]?.ToObject<StreamOptions>(serializer);
            var temperature = jsonObject["temperature"]?.ToObject<double>(serializer) ?? 1.0;
            var topP = jsonObject["top_p"]?.ToObject<double>(serializer) ?? 1.0;
            var tools = jsonObject["tools"]?.ToObject<List<Tool>>(serializer) ?? new List<Tool>();

            ToolChoice toolChoice = null;
            if (jsonObject.TryGetValue("tool_choice", out JToken toolChoiceToken))
            {
                if (toolChoiceToken.Type == JTokenType.String)
                {
                    toolChoice = new ToolChoice(toolChoiceToken.ToString());
                }
                else if (toolChoiceToken.Type == JTokenType.Object)
                {
                    var tool = toolChoiceToken["function"]?.ToObject<SimpleTool>(serializer);
                    if (tool != null)
                    {
                        toolChoice = new ToolChoice(tool);
                    }
                }
            }

            return new ChatGPTRequest(
                Messages: messages,
                Model: model,
                FrequencyPenalty: frequencyPenalty,
                MaxTokens: maxTokens,
                N: n,
                PresencePenalty: presencePenalty,
                Stop: stop,
                Stream: stream,
                StreamOptions: streamOptions,
                Temperature: temperature,
                TopP: topP,
                Tools: tools,
                ToolChoice: toolChoice ?? new ToolChoice("none")
            );
        }

        private static void RemoveNullProperties(JObject jsonObject)
        {
            var properties = jsonObject.Properties().Where(p => p.Value.Type == JTokenType.Null).ToList();
            foreach (var prop in properties)
            {
                jsonObject.Remove(prop.Name);
            }
        }
    }
}
