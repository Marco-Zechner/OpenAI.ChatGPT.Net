
# OpenAI.ChatGPT.Net - .NET 8.0 Library for ChatGPT

[![NuGet Package](https://img.shields.io/nuget/v/mz.ChatGPT.Net?logo=NuGet&color=004880&label=NuGet)](https://www.nuget.org/packages/mz.ChatGPT.Net)
[![NuGet Package](https://img.shields.io/nuget/dt/mz.ChatGPT.Net?color=%23004880&label=Downloads&logo=NuGet)](https://www.nuget.org/packages/mz.ChatGPT.Net)
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
ChatMessage singleMessage = new(ChatRole.User, input);
ChatResponse response = await model.CompletionAsync(singleMessage);
      
Console.WriteLine((ChatMessage)response);
// casting is required because a response can contain multiple messages (if the setting is enabled) and tool call information (if tools are added)
// casting will give you the 1. message if there are multiple.
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
ChatMessage singleMessage = new(ChatRole.User, "This will produce an error because \"invalidModel\" is not a valid model");
ChatResponse response = await model.CompletionAsync(singleMessage);

if (response.Error != null)
{
    Console.WriteLine($"Error: {response.Error.Message}");
    return;
}

Console.WriteLine((ChatMessage)response);
```
```csharp
GPTModel model = new("invalidModel", APIKey.KEY);
ChatMessage singleMessage = new(ChatRole.User, "This will produce an error because \"invalidModel\" is not a valid model");
ChatResponse response = await model.CompletionAsync(singleMessage);

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

## Tools (TESTING PHASE, static tools should work)

OpenAI.ChatGPT.Net allows for dynamic tool registration, making adding, removing, and managing tools easy. 
Tools are public methods that can be invoked by the GPT model based on user input or system needs.


### Registering Tools

You can register individual tools or entire classes of tools in your `GPTModel`. Below are examples of how to add various types of methods and properties as tools.

#### Adding Specific Tools and Properties

To add specific public static/instance methods/properties as tools, use the following syntax:

```csharp
var model = new GPTModel("gpt-4o", "YOUR-API-KEY") // Initializes the model
    .AddTool(() => MyTools.GetTime)                // Adding all overloads of a static tool
    .AddTool<InstanceToolCar>(tc => tc.TurnOn)     // Adding all overloads of an instance tool (currently disabled)
    .AddProperty(() => MyToolClass.MyProperty)     // Adding static Getter and Setter as a tool (if both are public)
    .AddProperty<InstanceToolCar>(tc => tc.isOn);  // Adding static Getter and Setter as a tool (if both are public)
```

#### Handling Methods with Multiple Overloads

If a method has multiple overloads, specify the desired method signature:

```csharp
model.AddTool(() => new Func<int, bool, string>(MyTools.GetTime))        // Adding a specific overload
    .AddTool<InstanceToolCar>(tc => new Func<double, int>(tc.FuelUp));   // Adding another overload (currently disabled)
```

#### Handling Properties in special Cases:

```csharp
model.AddProperty(() => MyToolClass.MyProperty, PropertyAccess.Getter);  // Only adds the Getter even if the Setter is also public

[GPT_Data(PropertyAccess.Getter)]            // Only allows the Getter to be added, even tough the setter is also public
public static int MyProperty { get; set; }
```
With the first option you can filter Getter and Setter locally for specific model instances
With the section option you can prevent ANY model instance from accessing the Getter or Setter while still keeping it public for yourself

### Adding Tools from Entire Classes

To add all tools from a class:

```csharp
model.AddToolClass<MyToolClass>(); // Adds all static tools and properties from the specified class 
```

To exclude specific methods, you can selectively remove them:

```csharp
model.AddToolClass<MyToolClass>()                           // Adds all static tools and properties from the class
    .RemoveTool<MyToolClass>(_ => MyToolClass.RemovedTool); // Removes specific tools
```

You can also add all public instance tools and properties or just ALL public tools and properties
```csharp
model.AddToolClass<MyToolClass>(MethodAccessType.InstanceOnly);                           
model.AddToolClass<MyToolClass>(MethodAccessType.StaticAndInstance);                           
```

### Custom Attributes for Tools

Utilize custom attributes to add metadata or control the inclusion of tools. Below are the attributes you can use:

- **`[GPT_Tool]`**: Marks a method available for GPT, OVERRIDES `[GPT_Locked]` on the class
- **`[GPT_Parameters]`**: Adds descriptions for method parameters, providing details about each expected parameter.
- **`[GPT_Description]`**: Adds a description to a class, method, or property, providing context for GPT.
- **`[GPT_Data]`**: Defines access for a property, specifying whether it’s read-only, write-only, or both.
- **`[GPT_Locked]`**: Marks a method or class as locked, preventing it from being used by GPT.

## Contributing

I welcome contributions! Here’s how you can get involved:

1. Fork the repository.
2. Create a feature branch (`git checkout -b feature-branch`).
3. Commit your changes (`git commit -am 'Add new feature'`).
4. Push to the branch (`git push origin feature-branch`).
5. Create a new Pull Request.

Please ensure your code is tested and follows the existing coding style. 
(about what existing code style am I even talking xD)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
