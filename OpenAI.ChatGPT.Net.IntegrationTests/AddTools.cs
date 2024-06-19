using OpenAI.ChatGPT.Net.DataModels;
using OpenAI.ChatGPT.Net.IntegrationTests.Tools;
using OpenAI.ChatGPT.Net.Tools;

namespace OpenAI.ChatGPT.Net.IntegrationTests
{

    internal class AddTools
    {
        public static async Task Run()
        {
            GPTModel model = new GPTModel("gpt-4o", APIKey.KEY) // Fluid API, allows chaining and supports:
            {
                MaxTokens = 1000,
                toolCallHandler = new MyToolHandler() // can be added her or with "model.SetToolCallHandler(...)"

            }.AddTool<MyTools>(_ => MyTools.GetTime) // Adding static Tools
                .AddTool<InstanceToolCar>(tc => tc.TurnOn) // Adding Instance Tools
                .AddTool<InstanceToolCar>(tc => new Func<int, int>(tc.FuelUp)) // with multiple Overloads
                .AddTool<InstanceToolCar>(tc => new Func<double, int>(tc.FuelUp)) // with multiple Overloads
                .AddToolClass<MyToolClass>() // Adding all Tools from a Class
                .RemoveTool<MyToolClass>(_ => MyToolClass.RemovedTool);// And filtering some out again.
            

            // This will not add the MyToolClassWithAttributes.MethodWithoutAttribute because the class has the [GPTAttributeFiltered] attribute
            model.AddToolClass<MyToolClassWithAttributes>();

            // This Method will not be added because it has the [GPTLockMethod] attribute
            model.AddTool<MyToolClass>(_ => MyToolClass.LockedMethod);

            // Methods of this Class will not be added because the class has the [GPTLockClass] attribute
            model.AddToolClass<NonToolMethods>();

            int maxToolCalls = 3;
            // You can also parse values to the ToolHandler
            model.SetToolCallHandler(new MyToolHandler(maxToolCalls));

            // You can also define a simpler version with the Wrapper
            model.SetToolCallHandler(new ToolCallHandlerWrapper()
            {
                ToolCall = (toolName, toolParameters, toolCallIndex) =>
                {
                    Console.WriteLine($"GPT wants to call {toolName} with parameters: {string.Join(", ", toolParameters)}");
                    return true;
                },
                ToolResponse = (toolCallIndex, response) =>
                {
                    Console.WriteLine($"Lambda: Tool response for call #{toolCallIndex}: {response}");
                    return $"Lambda: Tool response for call #{toolCallIndex}: {response}";
                }
            });

            ChatMessage initialMessage = new(ChatRole.User, "Tell me the current Time");

            ChatResponse response = await model.Complete(initialMessage);

            if (response.Error != null)
            {
                Console.WriteLine($"Error: {response.Error.Message}");
                return;
            }
            
            var message = (ChatMessage)response;
            Console.WriteLine(message.Role + ": " + message.Content);
        }
    }

    public class MyToolHandler(int maxToolCalls = 0) : IToolCallHandler
    {
        private readonly List<string> lockedTools = [];
        private string lastToolName = "";
        private object[]? lastToolParameters = null;
        private readonly int maxToolCalls = maxToolCalls;
        private int toolCallsCounter = 0;

        public List<string> OnGetAvailableTools(List<string> registeredTools)
        {
            if (maxToolCalls != 0 && toolCallsCounter >= maxToolCalls)
                return [];
            
            return registeredTools.Except(lockedTools).ToList();
        }

        /// <param name="toolCallIndex">The how manyth Tool Call it is since the users prompt</param>
        public bool OnToolCall(string toolName, object[] toolParameters, int toolCallIndex)
        {
            Console.WriteLine($"GPT wants to call {toolName} with parameters: {string.Join(", ", toolParameters)}");

            if (lastToolName == toolName && lastToolParameters == toolParameters)
            {
                // Prevent GPT from calling the same tool over and over again.
                lockedTools.Add(toolName);
                return false;
            }

            lastToolName = toolName;
            lastToolParameters = toolParameters;
            toolCallsCounter++;
            return true;
        }

        public string OnToolResponse(int toolCallIndex, object response)
        {
            Console.WriteLine($"Tool response received for call #{toolCallIndex}");
            return response?.ToString() ?? string.Empty;
        }

        public void OnCompletion()
        {
            // Don't forget to unlock the tools again for your next prompt!
            toolCallsCounter = 0;
            lockedTools.Clear();
            Console.WriteLine("GPT has finished calling tools.");
        }
    }
}
