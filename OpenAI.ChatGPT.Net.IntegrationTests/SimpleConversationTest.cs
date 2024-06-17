using OpenAI.ChatGPT.Net;

namespace OpenAI.ChatGPT.Net.IntegrtionTests
{
    internal class SimpleConversationTest
    {
        public static async void Run()
        {
            //GPTModel model = new GPTModel("gpt-4o", "key");

            //GPTMessage initialMessage = new GPTMessage(GPTRole.User, Console.ReadLine());
            //List<GPTMessage> messageHistory = [initialMessage];

            //do
            //{
            //    GPTResponse response = await model.Complete(messageHistory);

            //    if (response is GPTError error)
            //    {
            //        Console.WriteLine($"Error: {error.Message}");
            //        break; // or try to generate it again.
            //    }

            //    GPTMessage message = (GPTMessage)response; // or: GPTMessage messag = response as GPTMessage
            //    Console.WriteLine(message.Role + ": " + message.Message);
            //    messageHistory.Add(message);

            //    GPTMessage nextMessage = new GPTMessage(GPTRole.User, Console.ReadLine());
            //    messageHistory.Add(nextMessage);
            //}
            //while (true);
        }
    }
}
