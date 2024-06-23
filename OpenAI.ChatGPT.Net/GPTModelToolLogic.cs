using Newtonsoft.Json;
using OpenAI.ChatGPT.Net.Tools;
using OpenAI.ChatGPT.Net.InstanceTools;
using System.Linq.Expressions;
using System.Reflection;

namespace OpenAI.ChatGPT.Net.DataModels
{
    public partial class GPTModel
    {
        private const string PARAMETER_METHOD_SEPERATOR = "_-_";
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
            var tool = (tools?.FirstOrDefault(t => t.Function.Name == toolCall.Function.Name))
                ?? throw new InvalidOperationException($"Tool '{toolCall.Function.Name}' not found.");

            string parameterInfo = "";
            var parts = toolCall.Function.Name.Split(PARAMETER_METHOD_SEPERATOR);
            string toolName = parts[0];
            if (parts.Length == 2)
                parameterInfo = parts[1];

            var functionNameParts = toolName.Split('-');
            if (functionNameParts.Length != 2)
            {
                throw new ArgumentException($"Invalid tool name format: {toolCall.Function.Name}");
            }
            var className = functionNameParts[0];
            var methodName = functionNameParts[1];

            var arguments = JsonConvert.DeserializeObject<Dictionary<string, object>>(toolCall.Function.Arguments);
            //var argumentTypes = arguments?.Select(arg => GPTToolLogicHelpers.ToDefaultType(arg.Value.GetType())).ToList();
            //string[] parameterInfos = parameterInfo.Split('_', StringSplitOptions.RemoveEmptyEntries);
            List<Type> argumentTypes = GPTToolLogicHelpers.GenerateTypesFromSimplifiedTypeString(parameterInfo);

            //if (argumentTypes != null && argumentTypes.Count != parameterInfoTypes.Length)
            //{
            //    throw new ArgumentException($"Called Tool with ParameterInfo, but the count of ParameterInfos and provided Tool arguements doesn't match. {string.Join(',', parameterInfoTypes)} != {string.Join(',', argumentTypes)}");
            //}

            var toolType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name == className)
                ?? throw new InvalidOperationException($"Tool class '{className}' not found.");

            MethodInfo[] methods;
            MethodInfo? targetMethod = null;
            if (true) // go in here if tool is static
            {
                methods = toolType.GetMethods(BindingFlags.Public | BindingFlags.Static);
                targetMethod = FindMatchingMethod(className, methodName, argumentTypes, methods);
                return InvokeMethod(null, targetMethod, arguments, toolCall, index);
            }

            return new ToolCallResponse(toolCall.Id, toolCall.Function.Name, ChatRole.Tool, "Tool call not implemented.");
        }

        private static MethodInfo FindMatchingMethod(string className, string methodName, List<Type>? argumentTypes, MethodInfo[] methods)
        {
            // Filter methods by name
            var matchingMethods = methods.Where(m => m.Name == methodName).ToArray();
            if (matchingMethods.Length == 0)
            {
                throw new InvalidOperationException($"Method '{methodName}' not found in class '{className}'.");
            }

            // Filter methods by parameters
            MethodInfo? targetMethod = null;
            foreach (var method in matchingMethods)
            {
                var parameters = method.GetParameters();
                if (parameters.Length == argumentTypes?.Count)
                {
                    bool equal = true;
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i].ParameterType != argumentTypes[i])
                        {
                            equal = false;
                            break;
                        }
                    }
                    if (equal)
                    {
                        targetMethod = method;
                        break;
                    }
                }
            }

            if (targetMethod == null)
            {
                throw new InvalidOperationException($"No matching method '{methodName}' with the provided parameters found in class '{className}'.");
            }

            return targetMethod;
        }

        private ToolCallResponse? InvokeMethod(object? instance, MethodInfo targetMethod, Dictionary<string, object>? arguments, ToolCall toolCall, int index)
        {
            // Prepare parameter values for method invocation
            object[]? parameterValues = [];
            try
            {
                parameterValues = targetMethod.GetParameters().Select(p =>
                {
                    if (arguments != null && p.Name != null && arguments.TryGetValue(p.Name, out var arg))
                        return Convert.ChangeType(arg, p.ParameterType);

                    // Do I wanna try to fix faulty calls?
                    //if (p.HasDefaultValue)
                    //    return p.DefaultValue;

                    throw new ArgumentException($"Missing required parameter '{p.Name}' for method '{targetMethod.Name}'.");
                }).ToArray();
            }
            catch (FormatException ex)
            {
                return new ToolCallResponse(
                    ToolCallId: toolCall.Id,
                    Name: toolCall.Function.Name,
                    Role: ChatRole.Tool,
                    Content: $"Invalid argument format: {ex.Message}"
                );
            }

            
            if (toolCallHandler != null && !toolCallHandler.OnToolCall(toolCall.Function.Name, parameterValues, index, out var denailResponse))
            {
                if (string.IsNullOrEmpty(denailResponse)) return null;

                return new ToolCallResponse(
                        ToolCallId: toolCall.Id,
                        Name: toolCall.Function.Name,
                        Role: ChatRole.Tool,
                        Content: denailResponse
                    );
            }

            object? result = targetMethod.Invoke(instance, parameterValues);

            string responseContent = "";
            try
            {
                responseContent = JsonConvert.SerializeObject(result);
            }
            catch (Exception ex)
            {
                responseContent = (result?.ToString() ?? "null" + " SerializeFailed: " + ex.Message);
            }
            return new ToolCallResponse(
                ToolCallId: toolCall.Id,
                Name: toolCall.Function.Name,
                Role: ChatRole.Tool,
                Content: responseContent
            );
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

            List<MethodInfo> methods = [.. declaringType.GetMethods(bindingFlags)];

            PropertyInfo[] properties = declaringType.GetProperties(bindingFlags);

            ValidateClassAndMethods(declaringType, methods, properties, methodAccessType);

            //filter out property methods that are not allowed by the property tag.
            foreach (var property in properties)
            {
                bool propertyLocked = property.GetCustomAttribute<GPT_Locked>() != null;
                PropertyAccess? propertyData = property.GetCustomAttribute<GPT_Data>()?.PropertyAccess;

                for (int i = methods.Count - 1; i >= 0; i--)
                {
                    MethodInfo? method = methods[i];
                    if (property.GetMethod == method && (propertyLocked || propertyData == PropertyAccess.Setter))
                        methods.RemoveAt(i);

                    if (property.SetMethod == method && (propertyLocked || propertyData == PropertyAccess.Getter))
                        methods.RemoveAt(i);
                }
            }

            foreach (var method in methods)
            {
                if (GPTToolLogicHelpers.IsDefaultMethod(method.Name))
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

        private static void ValidateClassAndMethods(Type declaringType, IEnumerable<MethodInfo> methods, IEnumerable<PropertyInfo> properties, MethodAccessType methodAccessType)
        {
            bool hasGPT_ToolMethods = methods.Any(m => m.GetCustomAttribute<GPT_Tool>() != null);
            bool hasGPT_DataProperties = properties.Any(p => p.GetCustomAttribute<GPT_Data>() != null && methods.Any(m => p.GetMethod == m || p.SetMethod == m));

            bool allMethodsLocked = true;
            bool allPropertiesLocked = true;

            foreach (var method in methods)
            {
                bool propertyMethod = false;
                foreach (var property in properties)
                {
                    if (property.GetMethod == method || property.SetMethod == method)
                    {
                        propertyMethod = true;
                        if (property.GetCustomAttribute<GPT_Locked>() == null)
                            allPropertiesLocked = false;
                    }
                }

                if (propertyMethod == false && method.GetCustomAttribute<GPT_Locked>() == null)
                {
                    allMethodsLocked = false;
                }
            }

            if (declaringType.GetCustomAttribute<GPT_Locked>() != null && !hasGPT_ToolMethods && !hasGPT_DataProperties)
            {
                string methodTypeDescription = GetMethodTypeDescription(methodAccessType);
                throw new InvalidOperationException($"The class '{declaringType.Name}' is locked and has no specific [GPT_Tool] methods or [GPT_Data] properties. Unlock the class or add [GPT_Tool]/[GPT_Data] attributes to at least one public {methodTypeDescription} method/property.");
            }

            if (allMethodsLocked && allPropertiesLocked)
            {
                string methodTypeDescription = GetMethodTypeDescription(methodAccessType);
                throw new InvalidOperationException($"All methods and properties in the class '{declaringType.Name}' are locked. Please unlock at least one public {methodTypeDescription} method or property.");
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

        #endregion

        #region InternalToolHandling TEST

        private GPTModel AddToolIntern(MethodInfo methodInfo, bool throughToolClass)
        {
            var declaringType = GetDeclaringTypeOfAllowedTool(methodInfo, throughToolClass);
            if (declaringType == null) return this;

            Dictionary<string, ParameterDetail> properties = [];
            List<string> required = [];


            if (!methodInfo.IsStatic)
            {
                throw new NotImplementedException("Implemtation not ready. Please use static tools for now");

                properties.Add("instanceId", new ParameterDetail("integer", "The instance ID of the object.", []));
                required.Add("instanceId");
            }

            AddParameterDetails(methodInfo, ref properties, ref required);

            tools ??= [];
            var toolName = GenerateToolName(methodInfo, methodInfo.IsStatic);
            
            if (tools.Any(t => t.Function.Name == toolName && GPTToolLogicHelpers.StringCollectionEqual(t.Function.Parameters.Properties.Values.Select(p => p.Type), properties.Values.Select(p => p.Type))))
                return throughToolClass ? this : throw new InvalidOperationException($"The tool '{toolName}' already exists.");

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
            var declaringType = methodInfo.DeclaringType ?? throw new InvalidOperationException($"The method '{methodInfo.Name}' does not have a declaring type.");

            if (methodInfo.GetCustomAttribute<GPT_Locked>() != null)
                return throughToolClass ? null : throw new InvalidOperationException($"The method '{declaringType.Name}.{methodInfo.Name}' is marked as locked");

            BindingFlags bindingFlags = BindingFlags.Public;
            if (methodInfo.IsStatic)
                bindingFlags |= BindingFlags.Static;
            else
                bindingFlags |= BindingFlags.Instance;

            var properties = declaringType.GetProperties(bindingFlags);
            
            PropertyInfo? connectedProperty = properties.FirstOrDefault(p => p.GetMethod == methodInfo || p.SetMethod == methodInfo);
            bool getter = connectedProperty?.GetMethod == methodInfo;
            bool setter = connectedProperty?.SetMethod == methodInfo;
            

            if (declaringType.GetCustomAttribute<GPT_Locked>() != null)
            {
                if (connectedProperty == null)
                {
                    if (methodInfo.GetCustomAttribute<GPT_Tool>() == null)
                        return throughToolClass ? null : throw new InvalidOperationException($"The class '{declaringType.Name}' is marked as locked and the Method '{methodInfo.Name}' is not marked as a tool.");
                }
                else
                {
                    var dataAttribute = connectedProperty.GetCustomAttribute<GPT_Data>();
                    if (dataAttribute == null)
                        return throughToolClass ? null : throw new InvalidOperationException($"The class '{declaringType.Name}' is marked as locked and the Property '{connectedProperty.Name}' is not marked as data");
                    else
                    {
                        if (getter && dataAttribute.PropertyAccess == PropertyAccess.Setter)
                            return throughToolClass ? null : throw new InvalidOperationException($"The Data Attribute of Property '{declaringType.Name}.{connectedProperty.Name}' is set to only provide setter access. Tried to add '{methodInfo.Name}'");

                        if (setter && dataAttribute.PropertyAccess == PropertyAccess.Getter)
                            return throughToolClass ? null : throw new InvalidOperationException($"The Data Attribute of Property '{declaringType.Name}.{connectedProperty.Name}' is set to only provide getter access. Tried to add '{methodInfo.Name}'");
                    }
                }
            }

            return declaringType;
        }

        private static string GenerateToolName(MethodInfo methodInfo, bool isStatic)
        {
            var className = methodInfo.DeclaringType?.Name ?? throw new InvalidOperationException("The method does not have a declaring type.");
            var functionName = methodInfo.Name;
            var baseName = isStatic ? $"{className}-{functionName}" : $"{className}-{functionName}-{InstanceToolsManager.INSTANCE_METHOD_TAG}";

            var parameterInfos = methodInfo.GetParameters();
            if (parameterInfos.Length == 0)
                return baseName;
            
            return $"{baseName}{PARAMETER_METHOD_SEPERATOR}{string.Join('_', GPTToolLogicHelpers.GenerateSimplifiedTypeString(parameterInfos.Select(p => p.ParameterType)))}";
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
        
        public GPTModel AddProperty<T>(Expression<Func<T, object>> property, PropertyAccess access = PropertyAccess.Both) => ProcessProperty(property, access, AddToolIntern);

        public GPTModel AddProperty(Expression<Func<object>> property, PropertyAccess access = PropertyAccess.Both) => ProcessProperty(property, access, AddToolIntern);

        public GPTModel RemoveProperty<T>(Expression<Func<T, object>> property, PropertyAccess access = PropertyAccess.Both) => ProcessProperty(property, access, RemoveToolIntern);

        public GPTModel RemoveProperty(Expression<Func<object>> property, PropertyAccess access = PropertyAccess.Both) => ProcessProperty(property, access, RemoveToolIntern);

        private GPTModel ProcessProperty<T>(Expression<Func<T, object>> property, PropertyAccess access, Func<MethodInfo, bool, GPTModel> toolAction)
        {
            var propertyInfo = ExtractPropertyInfo(property);

            foreach (var method in GetPropertyMethods(propertyInfo, access))
                toolAction(method, false);

            return this;
        }

        private GPTModel ProcessProperty(Expression<Func<object>> property, PropertyAccess access, Func<MethodInfo, bool, GPTModel> toolAction)
        {
            var propertyInfo = ExtractPropertyInfo(property);

            foreach (var method in GetPropertyMethods(propertyInfo, access))
                toolAction(method, false);

            return this;
        }

        private static PropertyInfo ExtractPropertyInfo(LambdaExpression lambda)
        {
            if (lambda.Body is MemberExpression memberExpression && memberExpression.Member is PropertyInfo propertyInfo)
            {
                return propertyInfo;
            }
            else if (lambda.Body is UnaryExpression unaryExpression &&
                     unaryExpression.Operand is MemberExpression unaryMemberExpression &&
                     unaryMemberExpression.Member is PropertyInfo unaryPropertyInfo)
            {
                return unaryPropertyInfo;
            }
            else
            {
                throw new ArgumentException($"Could not extract provided property. Make sure to provide a property and not a field.");
            }
        }

        private static List<MethodInfo> GetPropertyMethods(PropertyInfo propertyInfo, PropertyAccess access)
        {
            List<MethodInfo> propertyMethods = [];
            var getterMethod = propertyInfo.GetMethod;
            var setterMethod = propertyInfo.SetMethod;

            if (propertyInfo.GetCustomAttribute<GPT_Locked>() != null)
                throw new InvalidOperationException($"The Property '{propertyInfo.DeclaringType?.Name}.{propertyInfo.Name}' is locked. Please remove [GPT_Locked] from it to add it.");

            PropertyAccess? propertyAccess = propertyInfo.GetCustomAttribute<GPT_Data>()?.PropertyAccess;

            if (access.HasFlag(PropertyAccess.Getter))
            {
                if (getterMethod == null || !getterMethod.IsPublic)
                    throw new InvalidOperationException($"The Property '{propertyInfo.DeclaringType?.Name}.{propertyInfo.Name}' has no public getter.");
                else if (propertyAccess != null && propertyAccess == PropertyAccess.Setter)
                    throw new InvalidOperationException($"The Property '{propertyInfo.DeclaringType?.Name}.{propertyInfo.Name}' has the [GPT_Data(PropertyAccess.{propertyAccess})] attribute and doesn't provide the Getter.");
                else
                    propertyMethods.Add(getterMethod);
            }

            if (access.HasFlag(PropertyAccess.Setter))
            {
                if (setterMethod == null || !setterMethod.IsPublic)
                    throw new InvalidOperationException($"The Property '{propertyInfo.DeclaringType?.Name}.{propertyInfo.Name}' has no public setter.");
                else if (propertyAccess != null && propertyAccess == PropertyAccess.Getter)
                    throw new InvalidOperationException($"The Property '{propertyInfo.DeclaringType?.Name}.{propertyInfo.Name}' has the [GPT_Data(PropertyAccess.{propertyAccess})] attribute and doesn't provide the Setter.");
                else
                    propertyMethods.Add(setterMethod);
            }

            return propertyMethods;
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
