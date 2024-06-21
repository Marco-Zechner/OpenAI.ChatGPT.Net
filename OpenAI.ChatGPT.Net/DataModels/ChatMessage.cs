using Newtonsoft.Json;
using OpenAI.ChatGPT.Net.Enums;
using OpenAI.ChatGPT.Net.Exeptions;
using OpenAI.ChatGPT.Net.Interfaces;

namespace OpenAI.ChatGPT.Net.DataModels
{

    public record ChatMessage(
        [property: JsonProperty("role")] ChatRole Role,
        [property: JsonProperty("content")] string? Content,
        [property: JsonProperty("tool_calls")] List<ToolCall>? ToolCalls = null
    ) : IMessage
    {
        public static explicit operator ChatMessage(ChatResponse response)
        {
            if (response.Error != null)
            {
                throw new GPTAPIResponseException(response.Error.Message);
            }

            if (response == null || response.Choices == null || response.Choices.Count == 0 || response.Choices[0].Message == null)
            {
                throw new ArgumentException("Invalid GPTResponse object: Response, choices, or message is null or empty.");
            }


            var message = response.Choices[0].Message;

            // Extract tool calls if present
            List<ToolCall>? toolCalls = null;
            if (message.ToolCalls != null && message.ToolCalls.Count > 0)
            {
                toolCalls = message.ToolCalls.Select(tc => new ToolCall(
                    tc.Id,
                    tc.Type,
                    new ToolCallFunction(
                        tc.Function.Name,
                        tc.Function.Arguments
                    )
                )).ToList();
            }

            return new ChatMessage(ChatRole.Assistant, message.Content, toolCalls);
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Content))
                return $"{Role}: {Content}";
            else if (ToolCalls != null && ToolCalls.Count > 0)
                return $"{Role}: Tool Calls: {string.Join("\n", ToolCalls)}";
            else
                return $"{Role}: <Empty Message>";
        }
    }
}
