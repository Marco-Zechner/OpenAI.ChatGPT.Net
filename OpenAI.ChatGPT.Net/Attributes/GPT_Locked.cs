namespace OpenAI.ChatGPT.Net.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
    public class GPT_Locked : Attribute
    {
    }
}
