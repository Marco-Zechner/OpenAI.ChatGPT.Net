using OpenAI.ChatGPT.Net.Attributes;
using OpenAI.ChatGPT.Net.InstanceTools;

namespace OpenAI.ChatGPT.Net.IntegrationTests.InstanceTools
{
    [GPT_Description("Instance of a Car")]
    public class CarInstance(string carName, int horsePower, string producer) : InstanceToolsBase<CarInstance>(carName)
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
}
