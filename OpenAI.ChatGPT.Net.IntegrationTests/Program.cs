using OpenAI.ChatGPT.Net.IntegrationTests;
using OpenAI.ChatGPT.Net.IntegrationTests.Tools;

//await SingleCompletionTest.TotalMin();

//await SingleCompletionTest.Run();

//await SimpleConversationTest.Run();

//await ConversationWithCustomHandlers.Run();

await AddTools.Run();




//string jsonFromGPT = "{\"function\":\"InstanceToolCar.InstantiateFromJson\"{\"carName\":\"AudiR8\",\"horsePower\":602,\"producer\":\"Audi\"}}";
//string content = "{\"carName\":\"AudiR8\",\"horsePower\":602,\"producer\":\"Audi\"}";
//Console.WriteLine(jsonFromGPT);
//string creationStatus = InstanceToolCar.InstantiateFromJson(content);
//Console.WriteLine(creationStatus);

//string toolCallFromGPT = "{\"function\":\"InstanceToolCar.this.FuelUp\"{\"instanceId\": 0,\"fuelAmount\": 100}}";
//Console.WriteLine(toolCallFromGPT);
//long instanceId = 0;
//int fuelAmount = 100;

//if (!InstanceToolCar.InstanceExists(instanceId))
//{
//    Console.WriteLine("Instance not found");
//    return;
//}

//InstanceToolCar instance = InstanceToolCar.GetInstance(instanceId);
//Console.WriteLine(instance.FuelUp(fuelAmount));

