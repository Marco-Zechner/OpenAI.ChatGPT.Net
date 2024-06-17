using OpenAI.ChatGPT.Net.IntegrtionTests.Tools;

namespace OpenAI.ChatGPT.Net.IntegrtionTests
{
    internal class AddTools
    {
        public static async void Run()
        {
            //GPTModel model = new GPTModel("gpt-4o", "key")
            //    .AddTool(MyTools.GetTime) // Fluid API, allows chaining
            //    .AddToolClass<MyToolClass>()
            //    .RemoveTool(MyToolClass.RemovedTool);

            //// This will not add the MyToolClassWithAttributes.MethodWithoutAttribute because the class has the [GPTAttributeFiltered] attribute
            //model.AddToolClass<MyToolClassWithAttributes>();

            //// This Method will not be added because it has the [GPTLockMethod] attribute
            //model.AddTool(MyToolClass.LockedMethod);

            //// Methods of this Class will not be added because the class has the [GPTLockClass] attribute
            //model.AddToolClass<NonToolMethods>();

            //model.SetToolHandler<MyToolHandler>();
            //// Or
            //// model.SetToolHandler(new MyToolHandler());

            //// also possible as anonymous class
            //int maxToolCalls = 3;
            //model.SetToolHandler(new
            //{
            //    OnToolCall = new Func<string, object[], int, bool>((toolName, toolParameters, toolCallIndex) =>
            //    {
            //        return toolCallIndex <= maxToolCalls;
            //    }),
            //    OnCompletion = new Action(() =>
            //    {
            //        Console.WriteLine("GPT has finished calling tools.");
            //    })

            //    // All 4 Methods can either be implmenter or be left with their default Implementation
            //});

            //GPTMessage initialMessage = new GPTMessage(GPTRole.User, "Call a Tool");

            //GPTResponse response = await model.Complete(initialMessage);

            //if (response is GPTError error)
            //{
            //    Console.WriteLine($"Error: {error.Message}");
            //    return;
            //}

            //message = response as GPTMessage;
            //Console.WriteLine(message.Role + ": " + message.Message);
        }
    }

    //public class MyToolHandler : IGPTToolHandler
    //{
    //    private readonly List<string> lockedTools = [];
    //    private string lastToolName = "";
    //    private object[]? lastToolParameters = null;

    //    public List<string> OnGetAvailableTools(List<string> registeredTools)
    //    {
    //        return registeredTools.Except(lockedTools).ToList();
    //    }
        
    //    /// <param name="toolCallIndex">The how manyth Tool Call it is since the users prompt</param>
    //    public bool OnToolCall(string toolName, object[] toolParameters, int toolCallIndex)
    //    {
    //        Console.WriteLine($"GPT wants to call {toolName} with parameters: {string.Join(", ", toolParameters)}");

    //        if (lastToolName == toolName && lastToolParameters == toolParameters)
    //        {
    //            // Prevent GPT from calling the same tool over and over again.
    //            lockedTools.Add(toolName);
    //            return false;
    //        }
            
    //        lastToolName = toolName;
    //        lastToolParameters = toolParameters;
    //        return true;
    //    }

    //    public string OnToolResponse(int toolCallIndex, object response)
    //    {
    //        Console.WriteLine($"Tool response received for call #{toolCallIndex}");
    //        return response?.ToString() ?? string.Empty;
    //    }

    //    public void OnCompletion()
    //    {
    //        // Don't forget to unlock the tools again for your next prompt!
    //        lockedTools.Clear();
    //        Console.WriteLine("GPT has finished calling tools.");
    //    }
    //}
}
