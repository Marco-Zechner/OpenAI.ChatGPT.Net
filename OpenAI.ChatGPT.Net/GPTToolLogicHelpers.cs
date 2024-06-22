namespace OpenAI.ChatGPT.Net
{
    public class GPTToolLogicHelpers
    {
        public static bool StringCollectionEqual(IEnumerable<string> collection1, IEnumerable<string> collection2)
        {
            string[] array1 = collection1.ToArray();
            string[] array2 = collection2.ToArray();

            if (array1.Length != array2.Length)
                return false;

            for (int i = 0; i < array1.Length; i++)
                if (array1[i] != array2[i])
                    return false;

            return true;
        }

        public static string ConvertToValidJsonType(Type type)
        {
            // Mapping .NET types to JSON Schema types
            if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte))
                return "integer";
            if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
                return "number";
            if (type == typeof(bool))
                return "boolean";
            if (type == typeof(string) || type.IsEnum)
                return "string";
            //if (type.IsArray || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            //    return "array";
            //if (type.IsClass)
            //    return "object";

            // For unsupported types, throw an exception
            throw new NotSupportedException($"Type '{type}' is not supported for tool parameters.");
        }

        public static bool IsDefaultJsonType(Type type) 
            => (type == typeof(int) || type == typeof(float) || type == typeof(bool) || type == typeof(string));
    }
}
