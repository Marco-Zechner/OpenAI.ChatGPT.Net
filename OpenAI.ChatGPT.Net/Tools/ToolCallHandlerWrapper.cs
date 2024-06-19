namespace OpenAI.ChatGPT.Net.Tools
{
    public class ToolCallHandlerWrapper : IToolCallHandler
    {
        public Func<List<string>, List<string>>? GetAvailableTools { get; set; }
        public Func<string, object[], int, bool>? ToolCall { get; set; }
        public Action? Completion { get; set; }
        public Func<int, object, string>? ToolResponse { get; set; }

        public List<string> OnGetAvailableTools(List<string> registeredTools)
        {
            return GetAvailableTools?.Invoke(registeredTools) ?? registeredTools;
        }

        public bool OnToolCall(string toolName, object[] toolParameters, int toolCallIndex)
        {
            return ToolCall?.Invoke(toolName, toolParameters, toolCallIndex) ?? true;
        }

        public void OnCompletion()
        {
            Completion?.Invoke();
        }

        public string OnToolResponse(int toolCallIndex, object response)
        {
            return ToolResponse?.Invoke(toolCallIndex, response) ?? response?.ToString() ?? string.Empty;
        }
    }
}
