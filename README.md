
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

OpenAI.ChatGPT.Net allows for dynamic tool registration, making it easy to add, remove, and manage tools. 
Tools are public methods that can be invoked by the GPT model based on user input or system needs.

### Registering Tools

You can register individual tools or entire classes of tools. 

Here’s how to add a specific public static and instance method as a tool:
```csharp
var model = new GPTModel("gpt-4o", "YOUR-API-KEY") // Fluid API, allows chaining
    .AddTool<MyTools>(_ => MyTools.GetTime) // Adding static Tools
    .AddTool<InstanceToolCar>(tc => tc.TurnOn) // Adding Instance Tools
```

Adding a specific Method if it has multiple Overloads
```csharp
model.AddTool<InstanceToolCar>(tc => new Func<int, int>(tc.FuelUp)) // with multiple Overloads
    .AddTool<InstanceToolCar>(tc => new Func<double, int>(tc.FuelUp)) // with multiple Overloads
```
### Adding Tools from Classes

To add all tools from a class:

```csharp
model.AddToolClass<MyToolClass>() // Adding all Tools from a Class
```

To exclude specific methods from being added:

```csharp
model.AddToolClass<MyToolClass>() // Adding all Tools from a Class
    .RemoveTool<MyToolClass>(_ => MyToolClass.RemovedTool);// And filtering some out again.
```

### Custom Attributes for Tools

Use custom attributes to provide metadata or control the inclusion of tools. Here are the attributes you can use:

- **`[GPTTool]`**: Adds a description to the Method for GPT that provides context or details about the tool's functionality.
- **`[GPTParameters]`**: Adds a description for the parameters, providing details about each parameter expected by the method.
- **`[InstanceDescription]`**: Adds a description to a class that will be available to GPT as an Instance, that provides details or context about the class.
- **`[PropertyDescription]`**: Adds a description to a property of a class that will be available to GPT as an Instance, giving additional information or context about the property.
- **`[GPTLockMethod]`**: Marks a Method as specifically NOT available for GPT, if you need that for any reason
- **`[GPTLockClassAttribute]`**: Marks all Methods of a Class as specifically NOT available for GPT, if you need that for any reason
- **`[GPTAttributeFiltered]`**: Marks a class for attribute-based filtering, indicating that if the Class is added as a ToolClass then only methods with Attributes (GPTTool, GPTParameters) will be added as Tools.

Example of tool class with attributes:

```csharp
Will add later. Look at "OpenAI.ChatGPT.Net.IntegrationTests/Tools/"
Different examples are in the different files
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
