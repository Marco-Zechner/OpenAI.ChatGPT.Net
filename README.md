
# OpenAI.ChatGPT.Net - .NET 8.0 Library for ChatGPT

# This is just the goal for now. NOT READY TO USE

```
Placeholder
[![NuGet Package](https://img.shields.io/nuget/v/OpenAI.ChatGPT.Net?logo=NuGet&color=004880&label=NuGet)](https://www.nuget.org/packages/OpenAI.ChatGPT.Net)
[![NuGet Package](https://img.shields.io/nuget/dt/OpenAI.ChatGPT.Net?color=%23004880&label=Downloads&logo=NuGet)](https://www.nuget.org/packages/OpenAI.ChatGPT.Net)
[![GitHub issues](https://img.shields.io/github/issues/yourusername/OpenAI.ChatGPT.Net?label=Issues&logo=GitHub)](https://github.com/yourusername/OpenAI.ChatGPT.Net/issues)
[![GitHub forks](https://img.shields.io/github/forks/yourusername/OpenAI.ChatGPT.Net?label=Forks&logo=GitHub)](https://github.com/yourusername/OpenAI.ChatGPT.Net/network)
[![GitHub stars](https://img.shields.io/github/stars/yourusername/OpenAI.ChatGPT.Net?label=Stars&logo=GitHub)](https://github.com/yourusername/OpenAI.ChatGPT.Net/stargazers)
[![GitHub license](https://img.shields.io/github/license/yourusername/OpenAI.ChatGPT.Net?label=License&logo=GitHub)](https://github.com/yourusername/OpenAI.ChatGPT.Net/blob/main/LICENSE)
```

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

OpenAI.ChatGPT.Net allows for dynamic tool registration using reflection, making it easy to add, remove, or manage tools. Tools are static methods that can be invoked by the GPT model based on user input or system needs.

### Registering Tools

You can register individual tools or entire classes of tools. Here’s how to add a specific tool:

```csharp
var model = new GPTModelBuilder("gpt-4o", "YOUR-API-KEY")
    .AddTool(MyTools.GetTime)
    .Build();
```

### Adding Tools from Classes

To add all tools from a class:

```csharp
model.AddToolClass<MyToolClass>();
```

To exclude specific methods from being added:

```csharp
model.AddToolClass<MyToolClass>()
     .RemoveTool(MyToolClass.RemovedMethod);
```

To add only methods with specific attributes from a class:

```csharp
model.AddToolClass<MyToolClass2>(onlyAttributes: true);
```

### Custom Attributes for Tools

Use custom attributes to provide metadata or control the inclusion of tools. Here are the attributes you can use:

- **`[GPTTool]`**: Marks a method as a tool with optional description.
- **`[GPTHideMethod]`**: Marks a method to be excluded unless specifically added.

Example of tool class with attributes:

```csharp
public static class MyTools
{
    [GPTTool("Returns the current time")]
    public static string GetTime() => DateTime.Now.ToString();

    [GPTHideMethod]
    public static string HiddenTool() => "This tool is hidden by default";
}

public static class MyToolClass2
{
    [GPTTool("This is Tool1")]
    public static string Tool1() => "Tool1 executed";

    [GPTTool("This is Tool2 with parameters")]
    public static string Tool2(int number) => $"Tool2 executed with number {number}";
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
