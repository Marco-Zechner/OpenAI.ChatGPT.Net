using OpenAI.ChatGPT.Net.Enums;

namespace OpenAI.ChatGPT.Net.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class GPT_Data(PropertyAccess propertyAccess = PropertyAccess.Both) : Attribute
    {
        public PropertyAccess PropertyAccess { get; } = propertyAccess;
    }
}
