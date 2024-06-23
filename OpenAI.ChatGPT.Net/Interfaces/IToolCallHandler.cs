using OpenAI.ChatGPT.Net.DataModels;

namespace OpenAI.ChatGPT.Net.Interfaces
{
    public interface IToolCallHandler
    {
        /// <summary>
        /// Called when the jsonPayload for the API is created and the available tools that should be send along are requested.
        /// </summary>
        /// <param name="registeredTools">All tools can are added on this model instance.</param>
        /// <returns>The tools that get send to the API. The tools that GPT can attempt to use.</returns>
        List<Tool> OnGetAvailableTools(List<Tool>? registeredTools);

        /// <summary>
        /// Called when a tool is about to be called.
        /// </summary>
        /// <param name="toolName">The Class and Method name of the tool. If there is a "this" between then it is an instance Method</param>
        /// <param name="toolParameters">The Parameters for the MethodCall, if there are any</param>
        /// <param name="toolCallIndex">The how manyth toolCall this is in case of parallel function calling. <see href="https://platform.openai.com/docs/guides/function-calling/parallel-function-calling"/></param>
        /// <param name="denialResponse">If you return false and:
        /// <list type="bullet">
        /// <item>add a denialResponse here then it will be handled as if the ToolCall returned your denialResponse. OnToolResponse will be called</item>
        /// <item>leave the denailResponse null or empty then the ToolCall (and the request for this toolCall) will be removed and OnToolResponse will not be called.</item>
        /// </list></param>
        /// <returns>Whether the tool is allowed to be called or not</returns>
        bool OnToolCall(string toolName, object[]? toolParameters, int toolCallIndex, out string? denialResponse);

        /// <summary>
        /// Called when all toolCalls are completed.
        /// </summary>
        void OnCompletion();

        /// <summary>
        /// Called when a tool response is received.
        /// You can only change/extract the response.Content here.
        /// You can't change the other properties of the response.
        /// </summary>
        /// <param name="toolCallIndex">The how manyth toolCall this is in case of parallel function calling. <see href="https://platform.openai.com/docs/guides/function-calling/parallel-function-calling"/></param>
        /// <param name="content">The response.Content which holdes the data that the toolCall created</param>
        /// <returns>The new data for response.Content. If this is null or Empty then the toolCall (and the request for this toolCall) will be removed.</returns>
        string OnToolResponse(int toolCallIndex, string content)
        {
            return content?.ToString() ?? string.Empty;
        }
    }
}
