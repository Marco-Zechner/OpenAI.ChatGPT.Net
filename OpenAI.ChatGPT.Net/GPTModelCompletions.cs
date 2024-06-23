using OpenAI.ChatGPT.Net.Interfaces;

namespace OpenAI.ChatGPT.Net.DataModels
{
    public partial class GPTModel
    {
        public IToolCallHandler? toolCallHandler;

        /// <summary>
        /// This overload does <b>NOT</b> handle any kind of chat history.
        /// <para>You need to implement that yourself and parse a List of messages.</para>
        /// </summary>
        /// <param name="singleMessage"></param>
        /// <returns></returns>
        public async Task<ChatResponse> CompletionAsync(ChatMessage singleMessage, bool useTools = true) => await CompletionAsync([singleMessage], useTools);
        public async Task<ChatResponse> CompletionAsync(IEnumerable<IMessage> messageHistory, bool useTools = true)
        {
            var chatHistory = messageHistory.ToList();
            var jsonPayload = GenerateJsonPayload(chatHistory, useTools);
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
        
        public async IAsyncEnumerable<ChatResponse> CompletionsAsync(ChatMessage singleMessage)
        {
            await foreach (var response in CompletionsAsync([singleMessage]))
            {
                yield return response;
            }
        }

        public async IAsyncEnumerable<ChatResponse> CompletionsAsync(IEnumerable<IMessage> messageHistory)
        {
            var chatHistory = messageHistory.ToList();
            var jsonPayload = GenerateJsonPayload(chatHistory);
            var jsonResponse = await SendAndReceiveJsonAsync(jsonPayload);
            var response = GenerateResponse(jsonResponse);

            yield return response;

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
                yield return toolResult;
            }
        }

        public async IAsyncEnumerable<ChatResponseChunk> StreamCompletionAsync(ChatMessage singleMessage, bool useTools = true)
        {
            await foreach (var chunk in StreamCompletionAsync([singleMessage], useTools))
            {
                yield return chunk;
            }
        }

        public async IAsyncEnumerable<ChatResponseChunk> StreamCompletionAsync(IEnumerable<IMessage> messageHistory, bool useTools = true)
        {
            yield return null;
        }

        public async IAsyncEnumerable<ChatResponseChunk> StreamCompletionsAsync(ChatMessage singleMessage)
        {
            await foreach (var chunk in StreamCompletionsAsync([singleMessage]))
            {
                yield return chunk;
            }
        }

        public async IAsyncEnumerable<ChatResponseChunk> StreamCompletionsAsync(IEnumerable<IMessage> messageHistory)
        {
            yield return null;
        }


        private async Task<ChatResponse> CompleteToolCalls(List<IMessage> messageHistory)
        {
            var jsonPayload = GenerateJsonPayload(messageHistory);
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
                    messageHistory.Add(toolRequest);
                }

                foreach (var toolResponse in toolResponses)
                {
                    messageHistory.Add(toolResponse);
                }

                return await CompleteToolCalls(messageHistory);
            }

            return response;
        }
    }
}
