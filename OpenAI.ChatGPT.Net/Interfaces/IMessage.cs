using OpenAI.ChatGPT.Net.Enums;

namespace OpenAI.ChatGPT.Net.Interfaces
{
    public interface IMessage
    {
        ChatRole Role { get; }
        string? Content { get; }
    }
}
