namespace OpenAI.ChatGPT.Net.IntegrationTests.Handlers
{
    public class JsonDebugHandlers
    {
        public static string PrintPayloadHandler(string payload)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Payload:");
            Console.WriteLine(payload);
            Console.ResetColor();
            return payload;
        }

        public static string PrintResponseHandler(string response)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Response:");
            Console.WriteLine(response);
            Console.ResetColor();
            return response;
        }
    }
}
