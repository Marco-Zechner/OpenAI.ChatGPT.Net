using OpenAI.ChatGPT.Net.Tools;

namespace OpenAI.ChatGPT.Net.IntegrationTests.Tools
{
    [InstanceDescription("Instance of a Car")]
    public class InstanceToolCar(string carName, int horsePower, string producer) : InstanceToolsManager<InstanceToolCar>(carName)
    {
        public int horsePower = horsePower;
        public string producer = producer;
        public int fuel = 0;
        public bool isOn = false;

        public InstanceToolCar() : this("CarInstanceExample", 0, "ExampleProducer") { }

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
