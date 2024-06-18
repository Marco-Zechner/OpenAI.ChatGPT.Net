using Newtonsoft.Json;
using System.Text;

namespace OpenAI.ChatGPT.Net.DataModels
{
    // https://platform.openai.com/docs/api-reference/chat/create
    public class GPTModel(string model, string apiKey)
    {
        public string BaseUrl { get; set; } = "https://api.openai.com/v1/chat/completions";
        private readonly string _apiKey = !string.IsNullOrWhiteSpace(apiKey) ? apiKey : throw new ArgumentException("API key cannot be null or empty", nameof(apiKey));

        public string Model { get; } = !string.IsNullOrWhiteSpace(model) ? model : throw new ArgumentException("Model cannot be null or empty", nameof(model));
        public double FrequencyPenalty { get; set; } = 0.0;
        public long MaxTokens { get; set; } = 256;
        public long N { get; set; } = 1;
        public double PresencePenalty { get; set; } = 0.0;
        public string[]? Stop { get; set; } = null;
        public bool Stream { get; set; } = false;
        public StreamOptions? StreamOptions { get; set; } = null;
        public double Temperature { get; set; } = 0.7;
        public double TopP { get; set; } = 0.9;
        private readonly List<Tool>? tools = null;
        private readonly ToolChoice? toolChoice = null;
        public bool ParallelToolCalls { get; set; } = true;
        public string? User = null;


        public async Task<ChatResponse> Complete(ChatMessage initialMessage) => await Complete([initialMessage]);
        public async Task<ChatResponse> Complete(List<ChatMessage> chatHistory)
        {
            var jsonPayload = GenerateJsonPayload(chatHistory);
            var jsonResponse = await SendAndReceiveJsonAsync(jsonPayload);
            return GenerateResponse(jsonResponse);
        }

        private static ChatResponse GenerateResponse(string jsonResponse)
        {
            return JsonConvert.DeserializeObject<ChatResponse>(jsonResponse) ?? throw new JsonSerializationException("Deserialization failed.");
        }

        private string GenerateJsonPayload(List<ChatMessage> messages)
        {
            ChatGPTRequest requestBody = new(
                Messages: messages,
                Model: Model,
                FrequencyPenalty: FrequencyPenalty,
                MaxTokens: MaxTokens,
                N: N,
                PresencePenalty: PresencePenalty,
                Stop: Stop,
                Stream: Stream,
                StreamOptions: StreamOptions,
                Temperature: Temperature,
                TopP: TopP,
                Tools: tools,
                ToolChoice: toolChoice,
                ParallelToolCalls: ParallelToolCalls,
                User: User
            );

            return JsonConvert.SerializeObject(requestBody, Formatting.Indented);
        }
        
        private async Task<string> SendAndReceiveJsonAsync(string jsonPayload)
        {
            Console.WriteLine(jsonPayload);
            using var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl)
            {
                Headers = { { "Authorization", $"Bearer {_apiKey}" } },
                Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
            };
            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
