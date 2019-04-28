using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Generator.IoTMonitor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                Console.WriteLine("Please specify a device connection string.");
                return;
            }

            var device = DeviceClient.CreateFromConnectionString(args[0]);

            Console.WriteLine("Starting simulated measurement device, press enter to exit.");
            var cts = new CancellationTokenSource();
            var generatorTask = RunSimulatedDataGeneratorAsync(device, cts.Token);
            Console.ReadLine();
            cts.Cancel();
            await generatorTask;
        }

        private static async Task RunSimulatedDataGeneratorAsync(DeviceClient device, CancellationToken ct)
        {
            var rand = new Random();

            while (!ct.IsCancellationRequested)
            {
                var measurement = new MeasurementMessage
                {
                    TemperatureUnit = "Temperature",
                    Temperature = (rand.Next(200, 259)/10f),
                    HumidityUnit = "%",
                    Humidity = rand.Next(400, 600)/10f,
                    PressureUnit = "hPa",
                    Pressure = rand.Next(9400,10500)/10f
                };

                Console.WriteLine("Sending measurement...");

                await SendMessage(device, measurement);

                await Task.Delay(1000, ct);
            }
        }

        private static async Task SendMessage(DeviceClient device, MeasurementMessage measurement)
        {
            var json = JsonConvert.SerializeObject(measurement);
            var message = new Message(Encoding.UTF8.GetBytes(json));

            await device.SendEventAsync(message);
        }

        public class MeasurementMessage
        {
            public float Temperature { get; set; }
            public string TemperatureUnit { get; set; }
            public float Humidity { get; set; }
            public string HumidityUnit { get; set; }
            public float Pressure { get; set; }
            public string PressureUnit { get; set; }
        }

    }
}
