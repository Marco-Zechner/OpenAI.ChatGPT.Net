using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OpenAI.ChatGPT.Net.Tools;
using System.Collections.Concurrent;

namespace OpenAI.ChatGPT.Net.InstanceTools
{
    [GPT_Locked]
    [GPT_Description("A manager for handling instances of a specific type.")]
    public abstract class InstanceToolsBase<InstanceType>(string instanceName) where InstanceType : InstanceToolsBase<InstanceType>, new()
    {
        [GPT_Data]
        [JsonProperty]
        [GPT_Description("Holds the name of the instance.")]
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
        private static readonly ConcurrentDictionary<long, InstanceType> Instances = [];
        [JsonIgnore] // no parameters because GPT shouldn't see them.
        private static readonly InstanceIdPool IdPool = new();

        
        private static string ToJsonString(InstanceType instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance), "Instance cannot be null.");
            }

            return JsonConvert.SerializeObject(instance, Settings);
        }

        public static InstanceType? GetFullInstanceDetail(long instanceId)
        {
            Instances.TryGetValue(instanceId, out InstanceType? instance);
            return instance;
        }

        [GPT_Tool]
        public static bool InstanceExists(long instanceId)
        {
            return Instances.ContainsKey(instanceId);
        }

        [GPT_Tool]
        public static List<(long instanceID, string instanceName)> GetInstances()
        {
            return Instances.Select(pair => (pair.Key, pair.Value.InstanceName)).ToList();
        }

        [GPT_Tool]
        public static string Destruct(long instanceId)
        {
            if (Instances.Remove(instanceId, out _))
            {
                IdPool.ReleaseId(instanceId);
                return "Instance removed";
            }
            return "Instance not found";
        }

        public static string Construct(InstanceType instance)
        {
            long instanceId = IdPool.GetId();
            if (Instances.TryAdd(instanceId, instance))
            {
                var createdInstance = new
                {
                    InstanceID = instanceId,
                    InstanceJsonData = ToJsonString(instance)
                };

                return JsonConvert.SerializeObject(createdInstance, Settings); ;
            }

            return $"Couldn't add instance with ID {instanceId}. Another thread might have just added an instance with it. Instance not created and not added.";
        }
    }
}
