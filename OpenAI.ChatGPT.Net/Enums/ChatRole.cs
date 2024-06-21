using Newtonsoft.Json;
using OpenAI.ChatGPT.Net.JsonConverters;

namespace OpenAI.ChatGPT.Net.Enums
{
    [JsonConverter(typeof(LowercaseStringEnumConverter))]
    public enum ChatRole
    {
        System,
        User,
        Assistant,
        Tool
    }
}
