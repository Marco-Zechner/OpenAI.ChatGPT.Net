using OpenAI.ChatGPT.Net.DataModels;
using OpenAI.ChatGPT.Net.Exeptions;

namespace OpenAI.ChatGPT.Net.IntegrationTests
{
    public class ConversationWithCustomHandlers
    {
        public static async Task Run()
        {
            static string PrintPayloadHandler(string payload)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Payload:");
                Console.WriteLine(payload);
                Console.ResetColor();
                return payload;
            }

            static string PrintResponseHandler(string response)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Response:");
                Console.WriteLine(response);
                Console.ResetColor();
                return response;
            }

            GPTModel model = new("f", APIKey.KEY)
            {
                PayloadHandler = PrintPayloadHandler,
                ResponseHandler = PrintResponseHandler,
                Logprobs = true,
                TopLogprobs = 2
            };

            /*======================================================================\\
            || The following code is the same as the SimpleConversationTest.cs file ||
            || But as an example the try-catch was moved inside and the catch won't ||
            || stop the conversation.                                               ||
            \\======================================================================*/

            Console.Write($"{ChatRole.User}: ");
            ChatMessage initialMessage = new(ChatRole.User, Console.ReadLine());
            List<ChatMessage> messageHistory = [initialMessage];

            while (true)
            {
                ChatResponse response = await model.Complete(messageHistory);
                ChatMessage message;
                try {
                    message = (ChatMessage)response; // This will throw an exception if the response is an error

                    Console.WriteLine($"{message.Role}: {message.Content}");
                    messageHistory.Add(message);
                } catch (GPTAPIResponseException ex) {
                    Console.WriteLine(ex.Message);
                    // You don't need to stop here, you can try to recover from the error.
                    model.Model = "gpt-4o"; // Change to a different model since we know this is the error
                    Console.WriteLine("Switching to model \"gpt-4o\"");

                    // continue; 
                    // be carful with skipping user input, this could lead to infinit recalling of the model.
                    // a better approach is to let the user reenter his message.
                    messageHistory.RemoveAt(messageHistory.Count - 1);
                }

                Console.Write($"{ChatRole.User}: ");
                ChatMessage nextMessage = new(ChatRole.User, Console.ReadLine());
                messageHistory.Add(nextMessage);
            }
        }
    }
}
