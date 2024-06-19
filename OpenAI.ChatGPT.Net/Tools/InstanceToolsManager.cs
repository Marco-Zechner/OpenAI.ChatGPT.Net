using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Concurrent;

namespace OpenAI.ChatGPT.Net.Tools
{
    [InstanceDescription("A manager for handling instances of a specific type.")]
    public abstract class InstanceToolsManager<T>(string instanceName) where T : InstanceToolsManager<T>, new()
    {
        [JsonProperty]
        [PropertyDescription("Holds the name of the instance.")]
        protected string InstanceName { get; set; } = instanceName;

        [JsonIgnore]
        protected static readonly JsonSerializerSettings Settings = new()
        {
            DefaultValueHandling = DefaultValueHandling.Include,
            NullValueHandling = NullValueHandling.Include,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        };
        [JsonIgnore] // no parameters because GPT shouldn't see them.
        private static readonly ConcurrentDictionary<long, T> Instances = [];
        [JsonIgnore] // no parameters because GPT shouldn't see them.
        private static readonly IdPool IdPool = new();

        public static T FromJsonString(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentException("JSON string cannot be null or empty.", nameof(json));
            }

            T? result = JsonConvert.DeserializeObject<T>(json);
            return result ?? throw new JsonException("Deserialization resulted in null.");
        }

        public static string ToJsonString(T instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance), "Instance cannot be null.");
            }

            return JsonConvert.SerializeObject(instance, Settings);
        }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this, Settings);
        }

        public static string GetClassDescription(Type type)
        {
            var attr = (InstanceDescriptionAttribute?)Attribute.GetCustomAttribute(type, typeof(InstanceDescriptionAttribute));
            return attr?.Description ?? "No description available.";
        }

        public static string GetPropertyDescriptions(Type type)
        {
            var properties = type.GetProperties();
            return string.Join("\n", properties.Select(prop =>
            {
                var attr = (PropertyDescriptionAttribute?)Attribute.GetCustomAttribute(prop, typeof(PropertyDescriptionAttribute));
                return $"{prop.Name}: {attr?.Description ?? "No description available."}";
            }));
        }

        public static T GetInstance(long instanceId)
        {
            return Instances[instanceId];
        }

        public static bool InstanceExists(long instanceId)
        {
            return Instances.ContainsKey(instanceId);
        }

        public static List<long> GetInstanceIDs()
        {
            return [.. Instances.Keys];
        }

        public static string DeleteInstance(long instanceId)
        {
            if (Instances.Remove(instanceId, out _))
            {
                IdPool.ReleaseId(instanceId);
                return "Instance removed";
            }
            return "Instance not found";
        }

        public static string InstantiateFromJson(string json)
        {
            try
            {
                T newInstance = FromJsonString(json);
                long instanceId = IdPool.GetId();
                if (Instances.TryAdd(instanceId, newInstance))
                {
                    return instanceId.ToString();
                }

                return $"Couldn't add instance with ID {instanceId}. Another thread might have just added an instance with it.";
            }
            catch (Exception ex) when (ex is JsonException || ex is ArgumentException || ex is ArgumentNullException)
            {
                return $"Serialization error: {ex.Message}. Ensure JSON is correct. Example:\n{GetJsonExample()}";
            }
        }

        private static string GetJsonExample()
        {
            T exampleInstance = new();
            return ToJsonString(exampleInstance);
        }
    }

    public class IdPool
    {
        private readonly ConcurrentQueue<long> _availableIds;
        private long _nextId;

        public IdPool()
        {
            _availableIds = new ConcurrentQueue<long>();
            _nextId = 0;
        }

        public long GetId()
        {
            if (_availableIds.TryDequeue(out var id))
            {
                return id;
            }
            return _nextId++;
        }

        public void ReleaseId(long id)
        {
            _availableIds.Enqueue(id);
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class InstanceDescriptionAttribute(string description) : Attribute
    {
        public string Description { get; } = description;
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PropertyDescriptionAttribute(string description) : Attribute
    {
        public string Description { get; } = description;
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class GPTLockMethodAttribute : Attribute
    {
    }    
    
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class GPTTool(string description) : Attribute
    {
        public string Description { get; } = description;
    }      
    
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class GPTParameters(params string[] descriptions) : Attribute
    {
        public string[] Descriptions { get; } = descriptions;
    }    
    
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class GPTLockClassAttribute : Attribute
    {
    }    
    
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class GPTAttributeFiltered : Attribute
    {
    }
}
