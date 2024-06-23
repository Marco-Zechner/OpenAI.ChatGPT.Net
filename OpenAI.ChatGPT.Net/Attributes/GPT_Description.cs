namespace OpenAI.ChatGPT.Net.Tools
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class GPT_Description(string descriptionForAPI) : Attribute
    {
        public string DescriptionForAPI { get; } = descriptionForAPI;
    }
}
