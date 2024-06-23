using OpenAI.ChatGPT.Net.Tools;

namespace OpenAI.ChatGPT.Net.Interfaces
{
    public interface IMessage
    {
        ChatRole Role { get; }
        string? Content { get; }
    }
}
