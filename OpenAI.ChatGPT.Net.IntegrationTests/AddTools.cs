using OpenAI.ChatGPT.Net.DataModels;
using OpenAI.ChatGPT.Net.Enums;
using OpenAI.ChatGPT.Net.Exeptions;
using OpenAI.ChatGPT.Net.IntegrationTests.Handlers;
using OpenAI.ChatGPT.Net.IntegrationTests.InstanceTools;
using OpenAI.ChatGPT.Net.IntegrationTests.Tools;
using OpenAI.ChatGPT.Net.Interfaces;
using OpenAI.ChatGPT.Net.InstanceTools;

namespace OpenAI.ChatGPT.Net.IntegrationTests
{

    internal class AddTools
    {
        public static async Task Run()
        {

            GPTModel model = new("gpt-4o", APIKey.KEY)
            {
                ResponseHandler = JsonDebugHandlers.PrintResponseHandler,
                PayloadHandler = JsonDebugHandlers.PrintPayloadHandler,
                toolCallHandler = new MyToolHandler(),
                MaxTokens = 1000
            };

            //====== STATIC TOOLS ======\\

            // Adding a public static method as a Tool.
            model.AddTool(() => MyTools.GetTime)

            // In case of overloads, you can specify the method signature
            .AddTool(() => new Func<int, bool, string>(MyTools.GetTime))

            // Removing a specific Tool
            .RemoveTool(() => MyTools.GetTime)
            .RemoveTool(() => new Func<int, bool, string>(MyTools.GetTime));

            // Adding a public static properties GETTER and SETTER as a Tool.
            model.AddProperty(() => MyToolClass.MyProperty)
            /* NOTE:
            Please keep in mind that the getter or setter must be public to be added.
            If only the getter is public, then only the getter will be added.
             */

            // You can also filter to only use the GETTER or SETTER of a property
            .AddProperty(() => MyToolClass.MyProperty, PropertyAccess.Getter);
            /* NOTE:
            Trying to add a the Getter specifically while it is private will throw an error.
             */

            //Same rules for removing properties
            model.RemoveProperty(() => MyToolClass.MyProperty)
            .RemoveProperty(() => MyToolClass.MyProperty, PropertyAccess.Getter);

            // Adding all
            // - public static methods
            // - GETTER/SETTER from static properties
            // of a class as Tools. Excludes methods & properties with the [GPT_Locked] attribute
            model.AddToolClass<MyToolClass>()

            // This class has the [GPT_Locked] attribute
            // Only methods with the [GPT_Tool] attribute will be added
            .AddToolClass<MyToolClassWithAttributes>();

            //--!!-- INVALID USAGE --!!--\\
            // Trying to Add a Method with the [GPT_Locked] attribute will throw an Error
            //model.AddTool(() => MyToolClass.LockedMethod)
            // Trying to Add a Class with the [GPT_Locked] attribute, where no methods have the [GPT_Tool] attribute will throw an Error
            //.AddToolClass<LockedClass>()
            // Trying to Add a Class where all methods have the [GPT_Locked] attribute will throw an Error
            //.AddToolClass<AllLockedMethods>()
            // Trying to Add a Field as a Property will throw an Error
            //.AddProperty(() => MyToolClass.MyField);



            //====== INSTANCE TOOLS ======\\
            /* NOTE:
            (valid): To be able to add instance methods, the class must inherit from InstanceToolsManager<T>
             */

            /* NOTE: 
            When you add the 1. instance Tool/Property of any (valid) class,
            there will be 4 additional tools from InstanceToolsManager<InstanceType> added:
            
            + List<(long instanceID, string instanceName) GetInstances()
            + bool InstanceExists(long instanceID)
            + InstanceType? GetFullInstanceDetail(long instanceID)
            and this Class will be added as a Valid usecase
            
            + string Destruct(long instanceID)
            but this Class will per default NOT be added as a Valid usecase. ==> Must be added specifically with 'AddDestructor'
            You can change the default with 'model.InstanceDestructorEnabledPerDefault = true'
            SUBNOTE: xD
            > Changing the default afterwards will not change the already added classes.
            
            To save Tokens and thus also Money, these 4 Tools exist only once, and are designed so that the API can use them for all Subclasses.
            The Constructor can't be done in this why, since each Constructor can have different parameters.
            */

            // Adding a Tool that can destroy an instance of the class
            model.AddDestructor<CarInstance>();
            // Removing the Destructor Tool
            model.RemoveDestructor<CarInstance>();

            // Adding all constructors of this class as Tools
            model.AddConstructor<CarInstance>();

            // Adding specific constructors of this class as Tools
            model.AddConstructor<CarInstance>([typeof(string), typeof(int), typeof(string)]);
            model.AddConstructor<CarInstance>([]); // empty constructor
            model.AddConstructor<CarInstance>([typeof(int)]);

            // Removing a specific constructor 
            model.RemoveConstructor<CarInstance>([typeof(int)]);
            model.RemoveConstructor<CarInstance>([]);


            // Adding a public instance method as a Tool.
            model.AddTool<CarInstance>(ci => ci.TurnOn)
 
            .AddTool<CarInstance>(ci => new Func<int, int>(ci.FuelUp))
            .AddTool<CarInstance>(ci => new Func<double, int>(ci.FuelUp));
            model.RemoveTool<CarInstance>(ci => new Func<int, int>(ci.FuelUp));

            model.AddProperty<CarInstance>(ci => ci.isOn);
            model.RemoveProperty<CarInstance>(ci => ci.isOn);

            // Adding all
            // - public instance methods
            // - GETTER/SETTER from instance properties
            // of a class as Tools. Excludes methods & properties with the [GPT_Locked] attribute
            model.AddToolClass<CarInstance>(MethodAccessType.InstanceOnly);
            /* Note:
            MethodAccessType.StaticAndInstance
            will add ALL public methods and the GETTER/SETTER of properties
             */

            //--!!-- INVALID USAGE --!!--\\
            // Same rules as the Static Tools
            // Additonally:
            // Trying to use a Class that does not inherit from InstanceToolsManager<T> as a InstanceClass will throw an Error
            //model.AddTool<MyToolClass>(tc => tc.NonStaticMethod);
            //model.AddToolClass<MyToolClass>(MethodAccessType.StaticAndInstance);


            int maxToolCalls = 3;
            // You can also parse values to the ToolHandler
            model.SetToolCallHandler(new MyToolHandler(maxToolCalls));

            // You can also define a simpler version with the Wrapper
            model.SetToolCallHandler(new ToolCallHandlerWrapper()
            {
                ToolCall = (toolName, toolParameters, toolCallIndex) =>
                {
                    Console.WriteLine($"GPT wants to call {toolName} with parameters: {string.Join(", ", toolParameters)}");
                    return (true, "");
                },
                ToolResponse = (toolCallIndex, response) =>
                {
                    Console.WriteLine($"Lambda: Tool response for call #{toolCallIndex}: {response}");
                    return $"Lambda: Tool response for call #{toolCallIndex}: {response}";
                }
            });

            
            // Simple Conversation Code

            Console.Write($"{ChatRole.User}: ");
            ChatMessage initialMessage = new(ChatRole.User, Console.ReadLine());
            List<IMessage> messageHistory = [initialMessage];
            while (true)
            {
                ChatResponse response = await model.Complete(messageHistory);
                ChatMessage message;
                try
                {
                    message = (ChatMessage)response; // This will throw an exception if the response is an error

                    Console.WriteLine($"{message.Role}: {message.Content}");
                    messageHistory.Add(message);
                }
                catch (GPTAPIResponseException ex)
                {
                    Console.WriteLine(ex.Message);
                    // You don't need to stop here, you can try to recover from the error.

                    Console.WriteLine("Removing last message from history.");
                    messageHistory.RemoveAt(messageHistory.Count - 1);
                }

                Console.Write($"{ChatRole.User}: ");
                ChatMessage nextMessage = new(ChatRole.User, Console.ReadLine());
                messageHistory.Add(nextMessage);
            }
        }
    }

    public class MyToolHandler(int maxToolCalls = 0) : IToolCallHandler
    {
        private readonly List<string> lockedTools = [];
        private string lastToolName = "";
        private object[]? lastToolParameters = null;
        private readonly int maxToolCalls = maxToolCalls;
        private int toolCallsCounter = 0;

        public List<Tool> OnGetAvailableTools(List<Tool> registeredTools)
        {
            if (maxToolCalls != 0 && toolCallsCounter >= maxToolCalls)
                return [];

            var allowedTools = registeredTools.Where(t => !lockedTools.Contains(t.Function.Name)).ToList();
            return allowedTools;
        }

        /// <param name="toolCallIndex">The how manyth Tool Call it is since the users prompt</param>
        public bool OnToolCall(string toolName, object[]? toolParameters, int toolCallIndex, out string? denialReason)
        {
            if (maxToolCalls != 0 && toolCallsCounter >= maxToolCalls)
            {
                denialReason = "You have reached the maximum number of tool calls allowed before needing to respond to the user again.";
                Console.WriteLine("GPT reached the maximum number of tool calls in a row");
                return false;
            }

            // TODO: GIVE THEM THE INSTANCE ID IF IT IS A INSTANCE TOOL
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"GPT wants to call {toolName}" + (toolParameters != null ? $" with parameters: {string.Join(", ", toolParameters)}" : "") + "\nAllow toolCall? y/n [optional denialReason]");
            Console.ResetColor();

            string? input = Console.ReadLine();

            lastToolName = toolName;
            lastToolParameters = toolParameters;
            toolCallsCounter++;
            denialReason = null;

            List<string>? inputs = input?.Split(' ').ToList();
            bool allowed = inputs != null && inputs[0] == "y";
            inputs?.RemoveAt(0);
            if (!allowed)
            {
                if (inputs != null && inputs.Count > 0)
                {
                    denialReason = string.Join(' ', inputs);
                }
                lockedTools.Add(toolName);
            }
            return allowed;
        }

        // don't make it static and then wonder why it doesn't work (like me xD)
        public string OnToolResponse(int toolCallIndex, string response)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Tool response received for call #{toolCallIndex}: \n{response}");
            Console.ResetColor();
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
