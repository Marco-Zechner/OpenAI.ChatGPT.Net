
# OpenAI.ChatGPT.Net - .NET 8.0 Library for ChatGPT

# This is just the goal for now. NOT READY TO USE

```
Placeholder
[![NuGet Package](https://img.shields.io/nuget/v/OpenAI.ChatGPT.Net?logo=NuGet&color=004880&label=NuGet)](https://www.nuget.org/packages/OpenAI.ChatGPT.Net)
[![NuGet Package](https://img.shields.io/nuget/dt/OpenAI.ChatGPT.Net?color=%23004880&label=Downloads&logo=NuGet)](https://www.nuget.org/packages/OpenAI.ChatGPT.Net)
```
[![GitHub issues](https://img.shields.io/github/issues/Marco-Zechner/OpenAI.ChatGPT.Net?label=Issues&logo=GitHub)](https://github.com/Marco-Zechner/OpenAI.ChatGPT.Net/issues)
[![GitHub forks](https://img.shields.io/github/forks/Marco-Zechner/OpenAI.ChatGPT.Net?label=Forks&logo=GitHub)](https://github.com/Marco-Zechner/OpenAI.ChatGPT.Net/network)
[![GitHub stars](https://img.shields.io/github/stars/Marco-Zechner/OpenAI.ChatGPT.Net?label=Stars&logo=GitHub)](https://github.com/Marco-Zechner/OpenAI.ChatGPT.Net/stargazers)
[![GitHub license](https://img.shields.io/github/license/Marco-Zechner/OpenAI.ChatGPT.Net?label=License&logo=GitHub)](https://github.com/Marco-Zechner/OpenAI.ChatGPT.Net/blob/main/LICENSE)


OpenAI.ChatGPT.Net is a C# library built on .NET 8.0 that provides an easy-to-use interface for integrating with the OpenAI ChatGPT API. This library allows developers to manage chat histories, integrate dynamic tools, and generate responses efficiently.

## Features

- **Simple API Integration**: Easily connect to the OpenAI ChatGPT API.
- **Dynamic Tool Invocation**: Register and manage tools dynamically using reflection.
- **Tool Class Management**: Add all or specific tools from a class with flexible inclusion and exclusion.
- **Static Tool Methods**: All tools are static, ensuring secure and consistent tool access.
- **Support for .NET 8.0**: Leverages the latest features of .NET 8.0 for optimal performance.
- **JSON Integration**: Utilizes Newtonsoft.Json for easy JSON handling.

## Table of Contents

- [Getting Started](#getting-started)
  - [Installation](#installation)
  - [Basic Usage](#basic-usage)
- [Tools](#tools)
  - [Registering Tools](#registering-tools)
  - [Adding Tools from Classes](#adding-tools-from-classes)
  - [Custom Attributes for Tools](#custom-attributes-for-tools)
- [Contributing](#contributing)
- [License](#license)

## Getting Started

### Installation

Install the library via NuGet:

```bash
NOT READY!!! Install-Package OpenAI.ChatGPT.Net
```

Or using .NET CLI:

```bash
NOT READY!!! dotnet add package OpenAI.ChatGPT.Net
```

### Basic Usage

Below is an example of using OpenAI.ChatGPT.Net without any tools:

```csharp
using OpenAI.ChatGPT.Net.DataModels;

string? input = Console.ReadLine();

GPTModel model = new("gpt-4o", APIKey.KEY);
ChatMessage initialMessage = new(ChatRole.User, input);
ChatResponse response = await model.Complete(initialMessage);
      
Console.WriteLine((ChatMessage)response);
```

You can also specify all the other parameters on the model. For more information on the parameters see the [OpenAI Docu](https://platform.openai.com/docs/api-reference/chat/create)
```csharp
GPTModel model = new("gpt-4o", APIKey.KEY)
{
   MaxTokens = 500
};
```

If the API responds with an Error you can either handle it directly or catch the error when trying to parse the response to a message
```csharp
GPTModel model = new("invalidModel", APIKey.KEY);
ChatMessage initialMessage = new(ChatRole.User, "This will produce an error because \"invalidModel\" is not a valid model");
ChatResponse response = await model.Complete(initialMessage);

if (response.Error != null)
{
    Console.WriteLine($"Error: {response.Error.Message}");
    return;
}

Console.WriteLine((ChatMessage)response);
```
```csharp
GPTModel model = new("invalidModel", APIKey.KEY);
ChatMessage initialMessage = new(ChatRole.User, "This will produce an error because \"invalidModel\" is not a valid model");
ChatResponse response = await model.Complete(initialMessage);

try {
  Console.WriteLine((ChatMessage)response);
} catch (GPTAPIResponseException ex) {
  Console.WriteLine(ex.Message);
}
```

You also can set 2 JsonHandlers
- payload: the JSON that gets sent to the API
- response: the JSON that you receive back from the API  

Here is an example where we print it out to the console to inspect (REALLY USEFUL if you get an error back from the API)
But you can also overwrite it to add some check/filter or something in between.
```csharp
static string PrintPayloadHandler(string payload)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Payload:");
    Console.WriteLine(payload);
    Console.ResetColor();
    return payload;
}

static string PrintResponseHandler(string response)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Response:");
    Console.WriteLine(response);
    Console.ResetColor();
    return response;
}

GPTModel model = new("gpt-4o", APIKey.KEY)
{
   PayloadHandler = PrintPayloadHandler,
   ResponseHandler = PrintResponseHandler
};
```

## Tools (CONCEPT PHASE, NOT YET IMPLEMENTED)

OpenAI.ChatGPT.Net allows for dynamic tool registration, making adding, removing, and managing tools easy. 
Tools are public methods that can be invoked by the GPT model based on user input or system needs.


### Registering Tools

You can register individual tools or entire classes of tools in your `GPTModel`. Below are examples of how to add various types of methods and properties as tools.

#### Adding Specific Methods

To add specific public static or instance methods as tools, use the following syntax:

```csharp
var model = new GPTModel("gpt-4o", "YOUR-API-KEY") // Initializes the model
    .AddTool(() => MyTools.GetTime)               // Adding all overloads of a static tool
    .AddTool<InstanceToolCar>(tc => tc.TurnOn);   // Adding all overloads of an instance tool
```

#### Handling Methods with Multiple Overloads

If a method has multiple overloads, specify the desired method signature:

```csharp
model.AddTool(() => new Func<int, bool, string>(MyTools.GetTime))      // Adding a specific overload
    .AddTool<InstanceToolCar>(tc => new Func<double, int>(tc.FuelUp));   // Adding another overload
```

### Adding Tools from Entire Classes

To add all tools from a class:

```csharp
model.AddToolClass<MyToolClass>(); // Adds all tools from the specified class
```

To exclude specific methods, you can selectively remove them:

```csharp
model.AddToolClass<MyToolClass>()                           // Adds all tools from the class
    .RemoveTool<MyToolClass>(_ => MyToolClass.RemovedTool); // Removes specific tools
```

### Custom Attributes for Tools

Utilize custom attributes to add metadata or control the inclusion of tools. Below are the attributes you can use:

- **`[GPTTool]`**: Marks a method available for GPT, OVERRIDES `[GPTLocked]` on the class
- **`[GPTParameters]`**: Adds descriptions for method parameters, providing details about each expected parameter.
- **`[GPTDescription]`**: Adds a description to a class, method, or property, providing context for GPT.
- **`[GPTData]`**: Defines access for a property, specifying whether it’s read-only, write-only, or both.
- **`[GPTLocked]`**: Marks a method or class as locked, preventing it from being used by GPT.

### Example of Tool Class with Attributes

Refer to the following examples in 
"OpenAI.ChatGPT.Net.IntegrationTests/Tools/" and
"OpenAI.ChatGPT.Net.IntegrationTests/InstanceTools/"
for detailed implementations:

```csharp
[GPTDescription("This class provides time-related utilities.")]
public class MyTools
{
    /// <summary>
    /// Example of a simple tool that returns the current time in a specific timezone.
    /// </summary>
    /// <param name="timeZoneOffset"></param>
    /// <param name="use24h"></param>
    /// <returns></returns>
    public static string GetTime(int timeZoneOffset, bool use24h)
    {
        var time = DateTime.UtcNow.AddHours(timeZoneOffset);
        return use24h ? time.ToString("HH:mm") : time.ToString("hh:mm tt");
    }

    [GPTLocked]
    public static void LockedMethod()
    {
        // Method implementation that should not be accessible by GPT
    }
}
```

Classes that have InstancTools need to inherit from `InstanceToolsManager<ClassName>(instanceName)` and parse a `instanceName` along
They also need to implement an Empty constructor.
See the implementation Example below:

```csharp
[Description("Instance of a Car")]
public class CarInstance(string carName, int horsePower, string producer) : InstanceToolsManager<CarInstance>(carName)
{
    public int horsePower = horsePower;
    public string producer = producer;
    public int fuel = 0;
    public bool isOn { get; set; } = false;

    public CarInstance() : this("CarInstanceExample", 0, "ExampleProducer") { }

    public CarInstance(int horsePower) : this("TestCar", horsePower, "TestProducer") { }

    public int FuelUp(int fuelAmount)
    {
        return fuel += fuelAmount;
    }

    public int FuelUp(double fuelAmount)
    {
        return fuel += (int)fuelAmount;
    }

    public bool TurnOn(bool setOn)
    {
        return isOn = setOn;
    }

    public override string ToString()
    {
        return $"Car: {InstanceName}\nHorsePower: {horsePower}\nProducer: {producer}\nFuel: {fuel}";
    }
}
```

### Full Integration Example

Below is a comprehensive example of how to register and manage tools, handle responses, and manage tool calls using `GPTModel`:

```csharp
public static async Task Run()
{
    GPTModel model = new("gpt-4o", APIKey.KEY)
    {
        ResponseHandler = JsonDebugHandlers.PrintResponseHandler,
        PayloadHandler = JsonDebugHandlers.PrintPayloadHandler,
        ToolCallHandler = new MyToolHandler(),
        MaxTokens = 1000
    };

    // Register static tools
    model.AddTool(() => MyTools.GetTime)
         .AddTool(() => new Func<int, bool, string>(MyTools.GetTime))
         .RemoveTool(() => MyTools.GetTime);

    // Register properties as tools
    model.AddProperty(() => MyToolClass.MyProperty)
         .AddProperty(() => MyToolClass.MyProperty, PropertyAccess.Getter)
         .RemoveProperty(() => MyToolClass.MyProperty);

    // Register all tools from a class
    model.AddToolClass<MyToolClass>()
         .AddToolClass<MyToolClassWithAttributes>();

    // Register instance tools and properties
    model.AddConstructor<CarInstance>()
         .AddTool<CarInstance>(ci => ci.TurnOn)
         .AddProperty<CarInstance>(ci => ci.isOn);

    // Set up a custom tool call handler
    model.SetToolCallHandler(new ToolCallHandlerWrapper
    {
        ToolCall = (toolName, toolParameters, toolCallIndex) =>
        {
            Console.WriteLine($"GPT wants to call {toolName} with parameters: {string.Join(", ", toolParameters)}");
            return (true, "");
        },
        ToolResponse = (toolCallIndex, response) =>
        {
            Console.WriteLine($"Tool response for call #{toolCallIndex}: {response}");
            return $"Tool response for call #{toolCallIndex}: {response}";
        }
    });

    // Simple conversation code
    Console.Write($"{ChatRole.User}: ");
    var initialMessage = new ChatMessage(ChatRole.User, Console.ReadLine());
    var messageHistory = new List<IMessage> { initialMessage };

    while (true)
    {
        var response = await model.Complete(messageHistory);
        var message = response as ChatMessage;
        if (message != null)
        {
            Console.WriteLine($"{message.Role}: {message.Content}");
            messageHistory.Add(message);
        }
        else
        {
            Console.WriteLine("Error processing response.");
            messageHistory.RemoveAt(messageHistory.Count - 1);
        }

        Console.Write($"{ChatRole.User}: ");
        var nextMessage = new ChatMessage(ChatRole.User, Console.ReadLine());
        messageHistory.Add(nextMessage);
    }
}

// Tool call handler implementation
public class MyToolHandler : IToolCallHandler
{
    private readonly List<string> lockedTools = new();
    private string lastToolName = "";
    private object[]? lastToolParameters;
    private readonly int maxToolCalls;
    private int toolCallsCounter;

    public MyToolHandler(int maxToolCalls = 0)
    {
        this.maxToolCalls = maxToolCalls;
    }

    public List<Tool> OnGetAvailableTools(List<Tool> registeredTools)
    {
        return maxToolCalls != 0 && toolCallsCounter >= maxToolCalls
            ? new List<Tool>()
            : registeredTools.Where(t => !lockedTools.Contains(t.Function.Name)).ToList();
    }

    public bool OnToolCall(string toolName, object[]? toolParameters, int toolCallIndex, out string? denialReason)
    {
        if (maxToolCalls != 0 && toolCallsCounter >= maxToolCalls)
        {
            denialReason = "Max tool calls reached.";
            return false;
        }

        Console.WriteLine($"GPT wants to call {toolName} with parameters: {string.Join(", ", toolParameters ?? Array.Empty<object>())}");
        var input = Console.ReadLine();
        toolCallsCounter++;
        lockedTools.Add(toolName);
        denialReason = input?.Split(' ', 2).ElementAtOrDefault(1);
        return input?.StartsWith("y") ?? false;
    }

    public string OnToolResponse(int toolCallIndex, string response)
    {
        Console.WriteLine($"Tool response for call #{toolCallIndex}: {response}");
        return response;
    }

    public void OnCompletion()
    {
        toolCallsCounter = 0;
        lockedTools.Clear();
        Console.WriteLine("Tool calling session completed.");
    }
}
```

## Contributing

We welcome contributions! Here’s how you can get involved:

1. Fork the repository.
2. Create a feature branch (`git checkout -b feature-branch`).
3. Commit your changes (`git commit -am 'Add new feature'`).
4. Push to the branch (`git push origin feature-branch`).
5. Create a new Pull Request.

Please ensure your code is well-tested and follows the existing coding style.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
