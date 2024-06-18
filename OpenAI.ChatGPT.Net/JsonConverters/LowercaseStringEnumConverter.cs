using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace OpenAI.ChatGPT.Net.JsonConverters
{
    public class LowercaseStringEnumConverter : StringEnumConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is Enum enumValue)
            {
                var enumString = enumValue.ToString().ToLowerInvariant();
                writer.WriteValue(enumString);
            }
            else
            {
                base.WriteJson(writer, value, serializer);
            }
        }
    }
}
