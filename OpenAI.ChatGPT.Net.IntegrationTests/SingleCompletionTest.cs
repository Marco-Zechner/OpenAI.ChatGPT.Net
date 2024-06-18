using OpenAI.ChatGPT.Net;
using OpenAI.ChatGPT.Net.DataModels;

namespace OpenAI.ChatGPT.Net.IntegrationTests
{
    internal class SingleCompletionTest
    {
        public static async Task Run()
        {
            GPTModel model = new("gpt-4o", APIKey.KEY)
            {
                MaxTokens = 500
            };

            ChatMessage initialMessage = new(ChatRole.User, "How are you?");

            ChatResponse response = await model.Complete(initialMessage);

            if (response.Error != null)
            {
                Console.WriteLine($"Error: {response.Error.Message}");
                return;
            }

            var message = (ChatMessage)response;
            Console.WriteLine(message.Role + ": " + message.Content);
        }
    }
}
