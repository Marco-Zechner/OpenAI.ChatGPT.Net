using OpenAI.ChatGPT.Net.Attributes;
using OpenAI.ChatGPT.Net.Enums;
using OpenAI.ChatGPT.Net.Interfaces;
using System.Linq.Expressions;
using System.Reflection;

namespace OpenAI.ChatGPT.Net.DataModels
{
    public partial class GPTModel
    {
        public IToolCallHandler? toolCallHandler;
        public bool InstanceDestructorEnabledPerDefault = false;

        #region ToolCall
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
            return null;
        }

        #endregion

        #region Add/RemoveTool

        public GPTModel AddTool<T>(Expression<Func<T, Delegate>> function)
        {
            if (function is not LambdaExpression lambda)
                throw new ArgumentException("Lambda expression expected", nameof(function));

            if (lambda.Body is not UnaryExpression unaryExpression ||
                unaryExpression.Operand is not MethodCallExpression methodCallExpression ||
                methodCallExpression.Object is not ConstantExpression constantExpression ||
                constantExpression.Value is not MethodInfo methodInfo)
            {
                throw new ArgumentException("Invalid lambda expression or method not found.");
            }

            return AddToolIntern(methodInfo);
        }

        public GPTModel AddTool(Expression<Func<Delegate>> function)
        {
            if (function is not LambdaExpression lambda)
                throw new ArgumentException("Lambda expression expected", nameof(function));

            if (lambda.Body is not UnaryExpression unaryExpression ||
                unaryExpression.Operand is not MethodCallExpression methodCallExpression ||
                methodCallExpression.Object is not ConstantExpression constantExpression ||
                constantExpression.Value is not MethodInfo methodInfo)
            {
                throw new ArgumentException("Invalid lambda expression or method not found.");
            }

            return AddToolIntern(methodInfo);
        }

        public GPTModel AddToolIntern(MethodInfo methodInfo)
        {

            //Code
            return this;
        }

        public GPTModel RemoveTool(Expression<Func<Delegate>> function)
        {
            if (function is not LambdaExpression lambda)
                throw new ArgumentException("Lambda expression expected", nameof(function));

            if (lambda.Body is not UnaryExpression unaryExpression ||
                unaryExpression.Operand is not MethodCallExpression methodCallExpression ||
                methodCallExpression.Object is not ConstantExpression constantExpression ||
                constantExpression.Value is not MethodInfo methodInfo)
            {
                throw new ArgumentException("Invalid lambda expression or method not found.");
            }

            return AddToolIntern(methodInfo);
        }
        public GPTModel RemoveTool<T>(Expression<Func<T, Delegate>> function)
        {
            if (function is not LambdaExpression lambda)
                throw new ArgumentException("Lambda expression expected", nameof(function));

            if (lambda.Body is not UnaryExpression unaryExpression ||
                unaryExpression.Operand is not MethodCallExpression methodCallExpression ||
                methodCallExpression.Object is not ConstantExpression constantExpression ||
                constantExpression.Value is not MethodInfo methodInfo)
            {
                throw new ArgumentException("Invalid lambda expression or method not found.");
            }

            return RemoveToolIntern(methodInfo);
        }
        
        public GPTModel RemoveToolIntern(MethodInfo methodInfo)
        {
            //Code
            return this;
        }

        #endregion Add/RemoveTool

        #region Add/RmoveToolClass
        public GPTModel AddToolClass<T>(MethodAccessType methodAccessType = MethodAccessType.StaticOnly)
        {
            MethodInfo[] methods = GetMethods<T>(methodAccessType);

            ProcessTools<T>(methods, method =>
            {
                AddToolIntern(method);
            });

            return this;
        }

        public GPTModel RemoveToolClass<T>(MethodAccessType methodAccessType = MethodAccessType.StaticOnly)
        {
            MethodInfo[] methods = GetMethods<T>(methodAccessType);

            ProcessTools<T>(methods, method =>
            {
                RemoveToolIntern(method);
            });

            return this;
        }

        private MethodInfo[] GetMethods<T>(MethodAccessType methodAccessType)
        {
            Type declaringType = typeof(T);

            BindingFlags bindingAttribute = BindingFlags.Public | methodAccessType switch
            {
                MethodAccessType.StaticOnly => BindingFlags.Static,
                MethodAccessType.InstanceOnly => BindingFlags.Instance,
                MethodAccessType.StaticAndInstance => BindingFlags.Static | BindingFlags.Instance,
                _ => throw new ArgumentOutOfRangeException(nameof(methodAccessType), methodAccessType, "Invalid AccessType")
            };

            return declaringType.GetMethods(bindingAttribute);
        }

        private void ProcessTools<T>(MethodInfo[] methods, Action<MethodInfo> toolAction)
        {
            Type declaringType = typeof(T);

            bool foundUnlockedMethod = false;
            bool allMethodsLocked = true;

            foreach (MethodInfo method in methods)
            {
                if (method.GetCustomAttribute<GPT_Tool>() != null)
                    foundUnlockedMethod = true;

                if (method.GetCustomAttribute<GPT_Locked>() == null)
                    allMethodsLocked = false;

                toolAction(method);
            }

            if (declaringType.GetCustomAttribute<GPT_Locked>() != null && !foundUnlockedMethod)
                throw new InvalidOperationException($"The class '{declaringType.Name}' is locked and has no specific GPTTool methods. Please unlock the class or add GPTTool attributes to the methods.");

            if (allMethodsLocked)
                throw new InvalidOperationException($"All methods in the class '{declaringType.Name}' are locked. Please unlock at least one method.");
        }

        #endregion Add/RmoveToolClass

        #region Add/RemoveProperty
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

        #endregion Add/RemoveProperty

        #region Add/RemoveConstructor

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

        #endregion Add/RemoveConstructor

        #region Add/RemoveDestructor

        public GPTModel AddDestructor<T>()
        {
            //code
            return this;
        }        
        
        public GPTModel RemoveDestructor<T>()
        {
            //code
            return this;
        }

        #endregion Add/RemoveDestructor

        public GPTModel SetToolCallHandler(IToolCallHandler toolCallHandler)
        {
            this.toolCallHandler = toolCallHandler;
            return this;
        }
    }
}
