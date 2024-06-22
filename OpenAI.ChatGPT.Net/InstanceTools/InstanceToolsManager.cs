using OpenAI.ChatGPT.Net.Attributes;
using OpenAI.ChatGPT.Net.DataModels;
using System.Reflection;

namespace OpenAI.ChatGPT.Net.InstanceTools
{
    public static class InstanceToolsManager
    {
        private static bool InstanceMethodsInitComplete = false;

        private const string INSTANCE_TOOL_MANAGER = "InstanceToolsBase";
        private const string INSTANCE_TOOLS_MANAGER_DESTRUCTOR_NAME = "Destruct";

        public const string INSTANCE_METHOD_TAG = "InstanceTool";

        private const string INSTANCE_CLASS_NAME = "instanceClassName";


        #region InstanceManagementTools REFACTOR

        public static void AddInstanceManagerMethods(Type instanceType, ref List<Tool> tools)
        {

            var managerType = typeof(InstanceToolsBase<>).MakeGenericType(instanceType);

            if (InstanceMethodsInitComplete)
                return;

            var methodNames = managerType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                      .Where(m => m.GetCustomAttributes(typeof(GPT_Tool), false).Length != 0);

            foreach (var method in methodNames)
            {
                var methodName = method.Name;
                if (tools != null)
                {
                    var instanceManagerTool = tools.FirstOrDefault(t => t?.Function.Name == $"{INSTANCE_TOOL_MANAGER}-{methodName}", null);
                    if (instanceManagerTool != null && methodName != INSTANCE_TOOLS_MANAGER_DESTRUCTOR_NAME)
                    {
                        instanceManagerTool.Function.Parameters.Properties.First(x => x.Key == INSTANCE_CLASS_NAME).Value.Enum.Add(instanceType.Name);
                        return;
                    }
                }

                // Add tool method with additional 'instanceClassName' parameter
                var parameters = CreateToolParametersWithInstanceClassName(method);

                if (methodName != INSTANCE_TOOLS_MANAGER_DESTRUCTOR_NAME && !parameters.Properties[INSTANCE_CLASS_NAME].Enum.Contains(instanceType.Name))
                    parameters.Properties[INSTANCE_CLASS_NAME].Enum.Add(instanceType.Name);

                var descriptionAttribute = method.GetCustomAttribute<GPT_Description>();

                var toolFunction = new ToolFunction($"{INSTANCE_TOOL_MANAGER}-{methodName}", $"Method for InstanceMethods marked by \"-{INSTANCE_METHOD_TAG}\" at the end of the name\n" + descriptionAttribute?.DescriptionForAPI, parameters);

                var tool = new Tool("function", toolFunction);
                tools ??= [];
                tools.Add(tool);
            }

            InstanceMethodsInitComplete = true;
        }

        public static void RemoveInstanceManagerMethods(Type instanceType, ref List<Tool> tools)
        {

            var managerType = typeof(InstanceToolsBase<>).MakeGenericType(instanceType);

            var methodNames = managerType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                      .Where(m => m.GetCustomAttributes(typeof(GPT_Tool), false).Length != 0);

            foreach (var method in methodNames)
            {
                var methodName = method.Name;
                if (tools != null)
                {
                    var instanceManagerTool = tools.FirstOrDefault(t => t?.Function.Name == $"{INSTANCE_TOOL_MANAGER}-{methodName}", null);
                    if (instanceManagerTool != null && methodName != INSTANCE_TOOLS_MANAGER_DESTRUCTOR_NAME)
                    {
                        instanceManagerTool.Function.Parameters.Properties.First(x => x.Key == INSTANCE_CLASS_NAME).Value.Enum.Add(instanceType.Name);
                        return;
                    }
                }

                // Add tool method with additional 'instanceClassName' parameter
                var parameters = CreateToolParametersWithInstanceClassName(method);

                if (methodName != INSTANCE_TOOLS_MANAGER_DESTRUCTOR_NAME && !parameters.Properties[INSTANCE_CLASS_NAME].Enum.Contains(instanceType.Name))
                    parameters.Properties[INSTANCE_CLASS_NAME].Enum.Add(instanceType.Name);

                var descriptionAttribute = method.GetCustomAttribute<GPT_Description>();

                var toolFunction = new ToolFunction($"{INSTANCE_TOOL_MANAGER}-{methodName}", $"Method for InstanceMethods marked by \"-{INSTANCE_METHOD_TAG}\" at the end of the name\n" + descriptionAttribute?.DescriptionForAPI, parameters);

                var tool = new Tool("function", toolFunction);
                tools ??= [];
                tools.Add(tool);
            }

            InstanceMethodsInitComplete = false;
        }

        private static ToolParameters CreateToolParametersWithInstanceClassName(MethodInfo methodInfo)
        {
            var gptParametersAttribute = methodInfo.GetCustomAttribute<GPT_Parameters>();

            var parameterDescriptions = gptParametersAttribute?.DescriptionsForAPI;

            ParameterInfo[] parameterInfos = methodInfo.GetParameters();
            Dictionary<string, ParameterDetail> properties = [];
            List<string> required = [];

            properties.Add(INSTANCE_CLASS_NAME, new ParameterDetail("string", "The class name of the instance", []));
            required.Add(INSTANCE_CLASS_NAME);

            for (int i = 0; i < parameterInfos.Length; i++)
            {
                ParameterInfo paramInfo = parameterInfos[i];
                string paramName = paramInfo.Name ?? $"param{i}";
                string paramType = GPTToolLogicHelpers.ConvertToValidJsonType(paramInfo.ParameterType);
                string paramDescription = parameterDescriptions != null && i < parameterDescriptions.Length
                ? parameterDescriptions[i]
                : string.Empty;

                properties.Add(paramName, new ParameterDetail(paramType, paramDescription, []));

                if (!paramInfo.IsOptional)
                    required.Add(paramName);
            }
            return new ToolParameters("object", properties, required);
        }

        #endregion

    }
}
