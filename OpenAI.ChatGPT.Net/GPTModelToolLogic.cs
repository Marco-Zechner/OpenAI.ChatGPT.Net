using OpenAI.ChatGPT.Net.Attributes;
using OpenAI.ChatGPT.Net.Enums;
using OpenAI.ChatGPT.Net.InstanceTools;
using System.Linq.Expressions;
using System.Reflection;

namespace OpenAI.ChatGPT.Net.DataModels
{
    public partial class GPTModel
    {
        private const string OVERLOAD_METHOD_SEPERATOR = "_-_";
        public bool InstanceDestructorEnabledPerDefault = false;

        #region ToolCall TODO
        public (List<ToolCallResponse> toolResponses, List<int> skippedIndices) CallTools(ToolCallSummary toolCallSummary)
        {
            if (toolCallSummary?.ToolCalls == null || toolCallSummary.ToolCalls.Count == 0)
                throw new ArgumentException("ToolCallSummary is null or empty.");

            List<ToolCallResponse> toolResponses = [];
            List<int> skippedIndices = [];

            for (int index = 0; index < toolCallSummary.ToolCalls.Count; index++)
            {
                ToolCall? toolCall = toolCallSummary.ToolCalls[index];
                var toolResponse = CallTool(toolCall, index);

                if (toolResponse == null)
                {
                    skippedIndices.Add(index);
                    continue;
                }

                string content = toolResponse.Content ?? "Content null. The tool call was probably rejected without a denial reason";
                toolResponse = toolResponse with { Content = toolCallHandler?.OnToolResponse(index, content) ?? content };
                toolResponses.Add(toolResponse);
            }

            return (toolResponses, skippedIndices);
        }

        private ToolCallResponse? CallTool(ToolCall toolCall, int index)
        {
            return new ToolCallResponse(toolCall.Id, toolCall.Function.Name, ChatRole.Tool, "Tool call not implemented.");
        }

        #endregion

        #region Add/RemoveTool DONE

        public GPTModel AddTool<T>(Expression<Func<T, Delegate>> function) => ProcessTool(function, AddToolIntern);
        public GPTModel AddTool(Expression<Func<Delegate>> function) => ProcessTool(function, AddToolIntern);

        public GPTModel RemoveTool<T>(Expression<Func<T, Delegate>> function) => ProcessTool(function, RemoveToolIntern);
        public GPTModel RemoveTool(Expression<Func<Delegate>> function) => ProcessTool(function, RemoveToolIntern);

        private static GPTModel ProcessTool<T>(Expression<Func<T, Delegate>> function, Func<MethodInfo, bool, GPTModel> toolAction)
        {
            if (function is not LambdaExpression lambda)
                throw new ArgumentException("Lambda expression expected", nameof(function));

            MethodInfo methodInfo = ExtractMethodInfo(lambda);
            return toolAction(methodInfo, false);
        }
        private static GPTModel ProcessTool(Expression<Func<Delegate>> function, Func<MethodInfo, bool, GPTModel> toolAction)
        {
            if (function is not LambdaExpression lambda)
                throw new ArgumentException("Lambda expression expected", nameof(function));

            MethodInfo methodInfo = ExtractMethodInfo(lambda);
            return toolAction(methodInfo, false);
        }

        private static MethodInfo ExtractMethodInfo(LambdaExpression lambda)
        {
            if (lambda.Body is not UnaryExpression unaryExpression ||
                unaryExpression.Operand is not MethodCallExpression methodCallExpression ||
                methodCallExpression.Object is not ConstantExpression constantExpression ||
                constantExpression.Value is not MethodInfo methodInfo)
            {
                throw new ArgumentException("Invalid lambda expression or method not found.");
            }

            return methodInfo;
        }


        #endregion

        #region Add/RemoveToolClass DONE
        public GPTModel AddToolClass<T>(MethodAccessType methodAccessType = MethodAccessType.StaticOnly) => ProcessTools<T>(methodAccessType, AddToolIntern);

        public GPTModel RemoveToolClass<T>(MethodAccessType methodAccessType = MethodAccessType.StaticOnly) => ProcessTools<T>(methodAccessType, RemoveToolIntern);

        private GPTModel ProcessTools<T>(MethodAccessType methodAccessType, Func<MethodInfo, bool, GPTModel> toolAction)
        {
            Type declaringType = typeof(T);

            BindingFlags bindingFlags = GetBindingFlags(methodAccessType);

            MethodInfo[] methods = declaringType.GetMethods(bindingFlags);

            ValidateClassAndMethods(declaringType, methods, methodAccessType);
            
            foreach (var method in methods)
            {
                if (IsDefaultMethod(method.Name))
                    continue;

                toolAction(method, true);
            }

            return this;
        }

        private static BindingFlags GetBindingFlags(MethodAccessType methodAccessType)
        {
            return BindingFlags.Public | methodAccessType switch
            {
                MethodAccessType.StaticOnly => BindingFlags.Static,
                MethodAccessType.InstanceOnly => BindingFlags.Instance,
                MethodAccessType.StaticAndInstance => BindingFlags.Static | BindingFlags.Instance,
                _ => throw new ArgumentOutOfRangeException(nameof(methodAccessType), methodAccessType, "Invalid AccessType")
            };
        }

        private static void ValidateClassAndMethods(Type declaringType, MethodInfo[] methods, MethodAccessType methodAccessType)
        {
            bool hasGPTToolMethods = methods.Any(m => m.GetCustomAttribute<GPT_Tool>() != null);
            bool allMethodsLocked = methods.All(m => m.GetCustomAttribute<GPT_Locked>() != null);

            if (declaringType.GetCustomAttribute<GPT_Locked>() != null && !hasGPTToolMethods)
            {
                string methodTypeDescription = GetMethodTypeDescription(methodAccessType);
                throw new InvalidOperationException($"The class '{declaringType.Name}' is locked and has no specific GPTTool methods. Unlock the class or add GPTTool attributes to at least one {methodTypeDescription} method.");
            }

            if (allMethodsLocked)
            {
                string methodTypeDescription = GetMethodTypeDescription(methodAccessType);
                throw new InvalidOperationException($"All methods in the class '{declaringType.Name}' are locked. Please unlock at least one {methodTypeDescription} method.");
            }
        }

        private static string GetMethodTypeDescription(MethodAccessType methodAccessType)
        {
            return methodAccessType switch
            {
                MethodAccessType.StaticOnly => "static",
                MethodAccessType.InstanceOnly => "instance",
                MethodAccessType.StaticAndInstance => "static or instance",
                _ => throw new ArgumentOutOfRangeException(nameof(methodAccessType), methodAccessType, "Invalid AccessType")
            };
        }

        private static bool IsDefaultMethod(string methodName)
        {
            HashSet<string> defaultMethods = new() { "GetHashCode", "Equals", "GetType", "ToString" };
            return defaultMethods.Contains(methodName);
        }


        #endregion

        #region InternalToolHandling TEST

        private GPTModel AddToolIntern(MethodInfo methodInfo, bool fromClass)
        {
            var declaringType = GetDeclaringTypeOfAllowedTool(methodInfo, fromClass);
            if (declaringType == null) return this;

            Dictionary<string, ParameterDetail> properties = [];
            List<string> required = [];

            if (!methodInfo.IsStatic)
            {
                properties.Add("instanceId", new ParameterDetail("integer", "The instance ID of the object.", []));
                required.Add("instanceId");
            }

            AddParameterDetails(methodInfo, ref properties, ref required);

            tools ??= [];
            var toolName = GenerateToolName(methodInfo, methodInfo.IsStatic);
            
            if (tools.Any(t => t.Function.Name == toolName && GPTToolLogicHelpers.StringCollectionEqual(t.Function.Parameters.Properties.Values.Select(p => p.Type), properties.Values.Select(p => p.Type))))
                return fromClass ? this : throw new InvalidOperationException($"The tool '{toolName}' already exists.");

            var parameters = new ToolParameters("object", properties, required);
            var methodDescription = methodInfo.GetCustomAttribute<GPT_Description>()?.DescriptionForAPI ?? string.Empty;

            var toolFunction = new ToolFunction(toolName, methodDescription, parameters);
            var tool = new Tool("function", toolFunction);

            if (!methodInfo.IsStatic)
                InstanceToolsManager.AddInstanceManagerMethods(declaringType, ref tools);

            tools.Add(tool);

            return this;
        }
        
        private GPTModel RemoveToolIntern(MethodInfo methodInfo, bool throughToolClass)
        {
            var declaringType = GetDeclaringTypeOfAllowedTool(methodInfo, throughToolClass);
            if (declaringType == null) return this;

            var toolName = GenerateToolName(methodInfo, methodInfo.IsStatic);
            var jsonTypes = methodInfo.GetParameters().Select(p => GPTToolLogicHelpers.ConvertToValidJsonType(p.ParameterType)).ToList();
            if (!methodInfo.IsStatic)
                jsonTypes.Insert(0, "integer");

            tools ??= [];

            var foundTool = tools.FirstOrDefault(t => t.Function.Name == toolName && GPTToolLogicHelpers.StringCollectionEqual(t.Function.Parameters.Properties.Values.Select(v => v.Type), jsonTypes));

            if (foundTool != null)
            {
                if (!methodInfo.IsStatic)
                    InstanceToolsManager.RemoveInstanceManagerMethods(declaringType, ref tools);

                tools.Remove(foundTool);
            }

            return this;
        }

        private static Type? GetDeclaringTypeOfAllowedTool(MethodInfo methodInfo, bool throughToolClass)
        {
            if (methodInfo.GetCustomAttribute<GPT_Locked>() != null)
                return throughToolClass ? null : throw new InvalidOperationException($"The Method '{methodInfo.Name}' is marked as locked");

            var declaringType = methodInfo.DeclaringType ?? throw new InvalidOperationException("The method does not have a declaring type.");

            if (declaringType.GetCustomAttribute<GPT_Locked>() != null && methodInfo.GetCustomAttribute<GPT_Tool>() == null)
                return throughToolClass ? null : throw new InvalidOperationException($"The class '{declaringType.Name}' is marked as locked and the Method '{methodInfo.Name}' is not marked as a tool.");

            return declaringType;
        }

        private static string GenerateToolName(MethodInfo methodInfo, bool isStatic)
        {
            var className = methodInfo.DeclaringType?.Name ?? throw new InvalidOperationException("The method does not have a declaring type.");
            var functionName = methodInfo.Name;
            var baseName = isStatic ? $"{className}-{functionName}" : $"{className}-{functionName}-{InstanceToolsManager.INSTANCE_METHOD_TAG}";

            var parameterInfos = methodInfo.GetParameters();
            if (parameterInfos.Length == 0 || parameterInfos.All(p => GPTToolLogicHelpers.IsDefaultJsonType(p.ParameterType)))
                return baseName;
            
            return $"{baseName}{OVERLOAD_METHOD_SEPERATOR}{string.Join('_', parameterInfos.Select(p => p.ParameterType.Name))}";
        }

        private static void AddParameterDetails(MethodInfo methodInfo, ref Dictionary<string, ParameterDetail> properties, ref List<string> required)
        {
            var parameterInfos = methodInfo.GetParameters();
            var parameterDescriptions = methodInfo.GetCustomAttribute<GPT_Parameters>()?.DescriptionsForAPI;

            for (int i = 0; i < parameterInfos.Length; i++)
            {
                var paramInfo = parameterInfos[i];
                var paramName = paramInfo.Name ?? $"param{i}";
                var paramType = GPTToolLogicHelpers.ConvertToValidJsonType(paramInfo.ParameterType);
                var paramDescription = parameterDescriptions != null && i < parameterDescriptions.Length
                    ? parameterDescriptions[i]
                    : string.Empty;
                properties.Add(paramName, new ParameterDetail(paramType, paramDescription, []));

                if (!paramInfo.IsOptional)
                    required.Add(paramName);
            }
        }

        #endregion

        #region Add/RemoveProperty TODO
        public GPTModel AddProperty<T>(Expression<Func<T, object>> property, PropertyAccess access = PropertyAccess.Both)
        {
            //code
            return this;
        }        
        
        public GPTModel AddProperty(Expression<Func<object>> property, PropertyAccess access = PropertyAccess.Both)
        {
            //code
            return this;
        }        
        
        public GPTModel RemoveProperty<T>(Expression<Func<T, object>> property, PropertyAccess access = PropertyAccess.Both)
        {
            //code
            return this;
        }        
        
        public GPTModel RemoveProperty(Expression<Func<object>> property, PropertyAccess access = PropertyAccess.Both)
        {
            //code
            return this;
        }

        #endregion

        #region Add/Remove Con/Destructor TODO

        public GPTModel AddConstructor<T>(Type[]? parameterTypes = null)
        {
            //code
            return this;
        }        
        
        public GPTModel RemoveConstructor<T>(Type[]? parameterTypes = null)
        {
            //code
            return this;
        }


        public GPTModel AddDestructor<T>()
        {
            return this;
        }        
        
        public GPTModel RemoveDestructor<T>()
        {
            return this;
        }

        #endregion


    }
}
