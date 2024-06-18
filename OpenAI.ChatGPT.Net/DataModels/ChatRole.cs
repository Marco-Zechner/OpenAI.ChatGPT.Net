using Newtonsoft.Json;
using OpenAI.ChatGPT.Net.JsonConverters;

namespace OpenAI.ChatGPT.Net.DataModels
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
