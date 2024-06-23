namespace OpenAI.ChatGPT.Net.Tools
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class GPT_Data(PropertyAccess propertyAccess = PropertyAccess.Both) : Attribute
    {
        public PropertyAccess PropertyAccess { get; } = propertyAccess;
    }
}
