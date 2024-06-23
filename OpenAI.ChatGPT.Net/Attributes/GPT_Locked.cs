namespace OpenAI.ChatGPT.Net.Tools
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class GPT_Locked : Attribute
    {
    }
}
