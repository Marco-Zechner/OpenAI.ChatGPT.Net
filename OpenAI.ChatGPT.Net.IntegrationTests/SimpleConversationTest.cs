using OpenAI.ChatGPT.Net.DataModels;
using OpenAI.ChatGPT.Net.Tools;
using OpenAI.ChatGPT.Net.Exeptions;
using OpenAI.ChatGPT.Net.Interfaces;

namespace OpenAI.ChatGPT.Net.IntegrationTests
{
    internal class SimpleConversationTest
    {
        public static async Task Run()
        {
            GPTModel model = new("gpt-4o", APIKey.KEY);

            Console.Write($"{ChatRole.User}: ");
            ChatMessage initialMessage = new(ChatRole.User, Console.ReadLine());
            List<IMessage> messageHistory = [initialMessage];

            try
            {
                while (true)
                {
                    ChatResponse response = await model.CompletionAsync(messageHistory);

                    ChatMessage message = (ChatMessage)response;
                    Console.WriteLine(message);
                    messageHistory.Add(message);

                    Console.Write($"{ChatRole.User}: ");
                    ChatMessage nextMessage = new(ChatRole.User, Console.ReadLine());
                    messageHistory.Add(nextMessage);
                }
            }
            catch (GPTAPIResponseException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
