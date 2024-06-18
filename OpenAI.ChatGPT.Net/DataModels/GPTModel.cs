namespace OpenAI.ChatGPT.Net.DataModels
{
    // https://platform.openai.com/docs/api-reference/chat/create
    public partial class GPTModel(string model, string apiKey)
    {
        public string BaseUrl { get; set; } = "https://api.openai.com/v1/chat/completions";
        private readonly string _apiKey = !string.IsNullOrWhiteSpace(apiKey) ? apiKey : throw new ArgumentException("API key cannot be null or empty", nameof(apiKey));

        public string Model { get; set; } = !string.IsNullOrWhiteSpace(model) ? model : throw new ArgumentException("Model cannot be null or empty", nameof(model));
        public double? FrequencyPenalty { get; set; } = 0.0;
        public Dictionary<int, int>? LogitBias { get; set; } = null;
        public bool? Logprobs { get; set; } = null;
        public int? TopLogprobs { get; set; } = null;
        public long? MaxTokens { get; set; } = 256;
        public long? N { get; set; } = 1;
        public double? PresencePenalty { get; set; } = 0.0;
        public string[]? Stop { get; set; } = null;
        public bool? Stream { get; set; } = false;
        public StreamOptions? StreamOptions { get; set; } = null;
        public double? Temperature { get; set; } = 0.7;
        public double? TopP { get; set; } = 0.9;
        private readonly List<Tool>? tools = null;
        private readonly ToolChoice? toolChoice = null;
        public bool? ParallelToolCalls { get; set; } = true;
        public string? User { get; set; } = null;
    }
}
