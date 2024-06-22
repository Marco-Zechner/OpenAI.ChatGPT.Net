using OpenAI.ChatGPT.Net.Interfaces;

namespace OpenAI.ChatGPT.Net.DataModels
{
    public partial class GPTModel
    {
        public IToolCallHandler? toolCallHandler;

        public async Task<ChatResponse> Complete(ChatMessage initialMessage) => await Complete([initialMessage]);
        public async Task<ChatResponse> Complete(List<IMessage> chatHistory)
        {
            var jsonPayload = GenerateJsonPayload(chatHistory);
            var jsonResponse = await SendAndReceiveJsonAsync(jsonPayload);
            var response = GenerateResponse(jsonResponse);

            var toolCall = (ToolCallSummary?)response;
            if (response.Error == null && toolCall != null)
            {
                (List<ToolCallResponse> toolResponses, List<int> skippedIndices) = CallTools(toolCall);

                var toolRequest = (ChatMessage)response;
                foreach (int indice in skippedIndices.OrderByDescending(v => v))
                {
                    toolRequest.ToolCalls?.RemoveAt(indice);
                }

                if (toolRequest.ToolCalls != null && toolRequest.ToolCalls.Count != 0)
                {
                    chatHistory.Add(toolRequest);
                }

                foreach (var toolResponse in toolResponses)
                {
                    chatHistory.Add(toolResponse);
                }

                var toolResult = await CompleteToolCalls(chatHistory);
                toolCallHandler?.OnCompletion();
                return toolResult;
            }

            return response;
        }

        private async Task<ChatResponse> CompleteToolCalls(List<IMessage> chatHistory)
        {
            var jsonPayload = GenerateJsonPayload(chatHistory);
            var jsonResponse = await SendAndReceiveJsonAsync(jsonPayload);
            var response = GenerateResponse(jsonResponse);

            var toolCall = (ToolCallSummary?)response;
            if (response.Error == null && toolCall != null)
            {
                (List<ToolCallResponse> toolResponses, List<int> skippedIndices) = CallTools(toolCall);

                var toolRequest = (ChatMessage)response;
                foreach (int indice in skippedIndices.OrderByDescending(v => v))
                {
                    toolRequest.ToolCalls?.RemoveAt(indice);
                }

                if (toolRequest.ToolCalls != null && toolRequest.ToolCalls.Count != 0)
                {
                    chatHistory.Add(toolRequest);
                }

                foreach (var toolResponse in toolResponses)
                {
                    chatHistory.Add(toolResponse);
                }

                return await CompleteToolCalls(chatHistory);
            }

            return response;
        }
    }
}
