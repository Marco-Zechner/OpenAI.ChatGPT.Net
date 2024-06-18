﻿using Newtonsoft.Json;

namespace OpenAI.ChatGPT.Net.DataModels
{
    
    public record ChatMessage(
        [property: JsonProperty("role")] ChatRole Role,
        [property: JsonProperty("content")] string? Content
    )
    {
        public static explicit operator ChatMessage(ChatResponse response)
        {
            if (response == null || response.Choices == null || response.Choices.Count == 0 || response.Choices[0].Message == null)
            {
                throw new ArgumentException("Invalid GPTResponse object: Response, choices, or message is null or empty.");
            }

            return new ChatMessage(ChatRole.Assistant, response.Choices[0].Message.Content);
        }
    }
}