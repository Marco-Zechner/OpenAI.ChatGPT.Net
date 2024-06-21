using Newtonsoft.Json;
using OpenAI.ChatGPT.Net.Exeptions;

namespace OpenAI.ChatGPT.Net.DataModels
{
    public record ToolCallSummary(
        [property: JsonProperty("tool_calls")] List<ToolCall>? ToolCalls
    )
    {
        public static explicit operator ToolCallSummary?(ChatResponse response)
        {
            if (response.Error != null)
            {
                throw new GPTAPIResponseException(response.Error.Message);
            }

            if (response == null || response.Choices == null || response.Choices.Count == 0 || response.Choices[0].Message == null)
            {
                throw new ArgumentException("Invalid GPTResponse object: Response, choices, or message is null or empty.");
            }

            if (response.Choices[0].FinishReason != "tool_calls")
            {
                return null;
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

            if (toolCalls == null || toolCalls.Count == 0)
            {
                return null;
            }

            return new ToolCallSummary(toolCalls);
        }
    }

}
