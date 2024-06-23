using Newtonsoft.Json;
using OpenAI.ChatGPT.Net.JsonConverters;

namespace OpenAI.ChatGPT.Net.Tools
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
