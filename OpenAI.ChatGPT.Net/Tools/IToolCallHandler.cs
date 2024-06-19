namespace OpenAI.ChatGPT.Net.Tools
{
    public interface IToolCallHandler
    {
        List<string> OnGetAvailableTools(List<string> registeredTools);
        bool OnToolCall(string toolName, object[] toolParameters, int toolCallIndex);
        void OnCompletion();

        // Optional method for handling tool responses
        string OnToolResponse(int toolCallIndex, object response)
        {
            return response?.ToString() ?? string.Empty;
        }
    }
}
