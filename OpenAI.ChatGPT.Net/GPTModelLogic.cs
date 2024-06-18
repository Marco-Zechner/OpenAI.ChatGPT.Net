namespace OpenAI.ChatGPT.Net.DataModels 
{
    public partial class GPTModel
    {
        public async Task<ChatResponse> Complete(ChatMessage initialMessage) => await Complete([initialMessage]);
        public async Task<ChatResponse> Complete(List<ChatMessage> chatHistory)
        {
            var jsonPayload = GenerateJsonPayload(chatHistory);
            var jsonResponse = await SendAndReceiveJsonAsync(jsonPayload);
            return GenerateResponse(jsonResponse);
        }
    }
}
