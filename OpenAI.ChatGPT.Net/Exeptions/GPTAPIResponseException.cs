namespace OpenAI.ChatGPT.Net.Exeptions
{
    public class GPTAPIResponseException : Exception
    {
        public GPTAPIResponseException() { }

        public GPTAPIResponseException(string message) : base(message) { }

        public GPTAPIResponseException(string message, Exception inner) : base(message, inner) { }

        public override string ToString() => $"GPT-API-ResponseException: {Message}\nStack Trace: {StackTrace}";
    }
}
