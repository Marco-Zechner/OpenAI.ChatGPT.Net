using Newtonsoft.Json;
using OpenAI.ChatGPT.Net.Interfaces;
using System.Text;

namespace OpenAI.ChatGPT.Net.DataModels
{
    public delegate string JsonPayloadHandler(string jsonPayload);
    public delegate string JsonResponseHandler(string jsonResponse);
    public partial class GPTModel
    {
        public JsonPayloadHandler PayloadHandler { get; set; } = payload => payload;
        public JsonResponseHandler ResponseHandler { get; set; } = response => response;

        private static ChatResponse GenerateResponse(string jsonResponse)
        {
            var response = JsonConvert.DeserializeObject<ChatResponse>(jsonResponse) ?? throw new JsonSerializationException("Deserialization failed.");
            response = response with
            {
                Timestamp = DateTime.Now
            };
            return response;
        }

        private string GenerateJsonPayload(List<IMessage> messages, bool useTools = true)
        {
            ChatGPTRequest requestBody = new(
                Messages: messages,
                Model: Model,
                FrequencyPenalty: FrequencyPenalty,
                LogitBias: LogitBias,
                Logprobs: Logprobs,
                TopLogprobs: TopLogprobs,
                MaxTokens: MaxTokens,
                N: N,
                PresencePenalty: PresencePenalty,
                Stop: Stop,
                Stream: Stream,
                StreamOptions: StreamOptions,
                Temperature: Temperature,
                TopP: TopP,
                ToolChoice: toolChoice,
                ParallelToolCalls: ParallelToolCalls,
                User: User
            )
            {
                Tools = useTools == false
                    ? []
                    : toolCallHandler != null
                        ? toolCallHandler.OnGetAvailableTools(tools) 
                        : tools
            };

            return JsonConvert.SerializeObject(requestBody, Formatting.Indented);
        }

        private async Task<string> SendAndReceiveJsonAsync(string jsonPayload)
        {
            jsonPayload = PayloadHandler(jsonPayload);

            using var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl)
            {
                Headers = { { "Authorization", $"Bearer {_apiKey}" } },
                Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
            };
            var response = await httpClient.SendAsync(request);

            string jsonResponse = await response.Content.ReadAsStringAsync();
            jsonResponse = ResponseHandler(jsonResponse);
            return jsonResponse;
        }
    }

}
