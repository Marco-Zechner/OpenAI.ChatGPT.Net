using OpenAI.ChatGPT.Net.Tools;
using System.Linq.Expressions;

namespace OpenAI.ChatGPT.Net.DataModels 
{
    public partial class GPTModel
    {
        public IToolCallHandler toolCallHandler;

        public async Task<ChatResponse> Complete(ChatMessage initialMessage) => await Complete([initialMessage]);
        public async Task<ChatResponse> Complete(List<ChatMessage> chatHistory)
        {
            var jsonPayload = GenerateJsonPayload(chatHistory);
            var jsonResponse = await SendAndReceiveJsonAsync(jsonPayload);
            return GenerateResponse(jsonResponse);
        }

        
        public GPTModel AddTool<T>(Expression<Func<T, Delegate>> function)
        {
            //code
            return this;
        }        
        
        public GPTModel RemoveTool<T>(Expression<Func<T, Delegate>> function)
        {
            //code
            return this;
        }

        public GPTModel AddToolClass<T>()
        {
            //code
            return this;
        }

        public GPTModel SetToolCallHandler(IToolCallHandler toolCallHandler)
        {
            this.toolCallHandler = toolCallHandler;
            return this;
        }
    }
}
