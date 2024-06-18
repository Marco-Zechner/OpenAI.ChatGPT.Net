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

            Console.WriteLine((ChatMessage)response);
        }

        public static async Task TotalMin()
        {
            string? input = Console.ReadLine();

            GPTModel model = new("gpt-4o", APIKey.KEY);
            ChatMessage initialMessage = new(ChatRole.User, input);
            ChatResponse response = await model.Complete(initialMessage);
      
            Console.WriteLine((ChatMessage)response);
        }
    }
}
