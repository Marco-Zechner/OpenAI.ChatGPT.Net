using System.Reflection;

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
            if (type == typeof(string))
                return "string";
            //if (type.IsArray || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            //    return "array";
            //if (type.IsClass)
            //    return "object";

            // For unsupported types, throw an exception
            throw new NotSupportedException($"Type '{type}' is not supported for tool parameters.");
        }

        public static string GenerateSimplifiedTypeString(IEnumerable<Type> types)
        {
            string typeInfo = "";
            foreach (var type in types)
            {
                _ = true switch
                {
                    bool _ when type == typeof(int) => typeInfo += "In",
                    bool _ when type == typeof(long) => typeInfo += "Lo",
                    bool _ when type == typeof(short) => typeInfo += "So",
                    bool _ when type == typeof(byte) => typeInfo += "By",
                    bool _ when type == typeof(float) => typeInfo += "Fo",
                    bool _ when type == typeof(double) => typeInfo += "Do",
                    bool _ when type == typeof(decimal) => typeInfo += "De",
                    bool _ when type == typeof(bool) => typeInfo += "Bo",
                    bool _ when type == typeof(string) => typeInfo += "St",
                    _ => throw new NotSupportedException($"Type '{type}' is not supported for tool parameters.")
                };
            }
            return typeInfo;
        }

        public static List<Type> GenerateTypesFromSimplifiedTypeString(string simplifiedTypeString)
        {
            List<Type> types = [];
            while(simplifiedTypeString.Length >= 2)
            {
                string nextPart = simplifiedTypeString[..2];
                simplifiedTypeString = simplifiedTypeString[2..];

                Type foundType = nextPart switch
                {
                    "In" => typeof(int),
                    "Lo" => typeof(long),
                    "So" => typeof(short),
                    "By" => typeof(byte),
                    "Fo" => typeof(float),
                    "Do" => typeof(double),
                    "De" => typeof(decimal),
                    "Bo" => typeof(bool),
                    "St" => typeof(string),
                    _ => throw new NotSupportedException($"Couldn't convert {nextPart} into a valid Type")
                };
                types.Add(foundType);
            }
            return types;
        }

        public static Type ToDefaultType(Type type)
        {
            if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte))
                return typeof(int);
            if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
                return typeof(float);
            if (type == typeof(bool))
                return typeof(bool);
            if (type == typeof(string))
                return typeof(string);

            // For unsupported types, throw an exception
            throw new NotSupportedException($"Type '{type}' is not supported for tool parameters.");
        }


        public static bool IsDefaultMethod(string methodName) 
            => new HashSet<string>() { "GetHashCode", "Equals", "GetType", "ToString" }.Contains(methodName);
    }
}
