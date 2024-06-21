using OpenAI.ChatGPT.Net.DataModels;
using OpenAI.ChatGPT.Net.Interfaces;

namespace OpenAI.ChatGPT.Net.InstanceTools
{
    public class ToolCallHandlerWrapper : IToolCallHandler
    {
        /// <summary>
        /// Called when the jsonPayload for the API is created and the available tools that should be send along are requested.
        /// <para>registeredTools: All tools can are added on this model instance.</para>
        /// </summary>
        /// <returns>The tools that get send to the API. The tools that GPT can attempt to use.</returns>
        public Func<List<Tool>?, List<Tool>>? GetAvailableTools { get; set; }

        /// <summary>
        /// Called when a tool is about to be called.
        /// <para>toolName: The Class and Method name of the tool. If there is a "this" between then it is an instance Method</para>
        /// <para>toolParameters: The Parameters for the MethodCall, if there are any</para>
        /// <para>toolCallIndex: The how manyth toolCall this is in case of parallel function calling. <see href="https://platform.openai.com/docs/guides/function-calling/parallel-function-calling"/></para>
        /// </summary>
        /// <returns>Whether the tool is allowed to be called or not
        /// <para>denialReason: If you return false and:
        /// <list type="bullet">
        /// <item>add a denialReason here then it will be handled as if the ToolCall returned your denialReason. OnToolResponse will be called</item>
        /// <item>leave the denailResponse null or empty then the ToolCall (and the request for this toolCall) will be removed and OnToolResponse will not be called.</item>
        /// </list></para>
        /// </returns>
        public Func<string, object[]?, int, (bool, string?)>? ToolCall { get; set; }
        /// <summary>
        /// Called when all toolCalls are completed.
        /// </summary>
        public Action? Completion { get; set; }
        /// <summary>
        /// Called when a tool response is received.
        /// You can only change/extract the response.Content here.
        /// You can't change the other properties of the response.
        /// <para>toolCallIndex: The how manyth toolCall this is in case of parallel function calling. <see href="https://platform.openai.com/docs/guides/function-calling/parallel-function-calling"/></para>
        /// <para>content: The response.Content which holdes the data that the toolCall created</para>
        /// </summary>
        /// <returns>The new data for response.Content. If this is null or Empty then the toolCall (and the request for this toolCall) will be removed.</returns>
        public Func<int, string, string>? ToolResponse { get; set; }

        /// <summary>
        /// Called when the jsonPayload for the API is created and the available tools that should be send along are requested.
        /// <para>registeredTools: All tools can are added on this model instance.</para>
        /// </summary>
        /// <returns>The tools that get send to the API. The tools that GPT can attempt to use.</returns>
        public List<Tool>? OnGetAvailableTools(List<Tool>? registeredTools) // TODO: look into this
        {
            return GetAvailableTools?.Invoke(registeredTools) ?? registeredTools;
        }

        /// <summary>
        /// Called when a tool is about to be called.
        /// </summary>
        /// <param name="toolName">The Class and Method name of the tool. If there is a "this" between then it is an instance Method</param>
        /// <param name="toolParameters">The Parameters for the MethodCall, if there are any</param>
        /// <param name="toolCallIndex">The how manyth toolCall this is in case of parallel function calling. <see href="https://platform.openai.com/docs/guides/function-calling/parallel-function-calling"/></param>
        /// <param name="denialReason">If you return false and:
        /// <list type="bullet">
        /// <item>add a denialReason here then it will be handled as if the ToolCall returned your denialReason. OnToolResponse will be called</item>
        /// <item>leave the denailResponse null or empty then the ToolCall (and the request for this toolCall) will be removed and OnToolResponse will not be called.</item>
        /// </list></param>
        /// <returns>Whether the tool is allowed to be called or not</returns>
        public bool OnToolCall(string toolName, object[]? toolParameters, int toolCallIndex, out string? denialReason)
        {
            var tuple = ToolCall?.Invoke(toolName, toolParameters, toolCallIndex);
            if (tuple == null)
            {
                denialReason = null;
                return false;
            }
            (bool allowed, denialReason) = tuple.Value;
            return allowed;
        }
        /// <summary>
        /// Called when all toolCalls are completed.
        /// </summary>
        public void OnCompletion()
        {
            Completion?.Invoke();
        }
        /// <summary>
        /// Called when a tool response is received.
        /// You can only change/extract the response.Content here.
        /// You can't change the other properties of the response.
        /// </summary>
        /// <param name="toolCallIndex">The how manyth toolCall this is in case of parallel function calling. <see href="https://platform.openai.com/docs/guides/function-calling/parallel-function-calling"/></param>
        /// <param name="content">The response.Content which holdes the data that the toolCall created</param>
        /// <returns>The new data for response.Content. If this is null or Empty then the toolCall (and the request for this toolCall) will be removed.</returns>
        public string OnToolResponse(int toolCallIndex, string response)
        {
            return ToolResponse?.Invoke(toolCallIndex, response) ?? response?.ToString() ?? string.Empty;
        }
    }
}
