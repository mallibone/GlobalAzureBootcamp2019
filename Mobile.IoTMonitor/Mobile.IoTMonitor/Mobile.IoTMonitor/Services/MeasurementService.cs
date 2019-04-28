using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace Mobile.IoTMonitor
{
    class MeasurementService : IMeasurementService
    {
        private HubConnection _connection;
        private const string backendUrl = "https://iotdemofunction.azurewebsites.net/api/";
        //private const string backendUrl = "http://localhost:7071/api/";
        //public event EventHandler<Measurement> NewMeasurement;
        private Subject<Measurement> _newMeasurement = new Subject<Measurement>();

        public MeasurementService()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(backendUrl)
                .Build();
        }

        public IObservable<Measurement> NewMeasurement => _newMeasurement.AsObservable();

        public async Task Connect()
        {
            if (_connection.State == HubConnectionState.Connected) return;

            _connection.On<string>("measurement", (messageString) =>
            {
                var message = JsonConvert.DeserializeObject<Measurement>(messageString);
                _newMeasurement.OnNext(message);
                Debug.WriteLine(messageString);
            });

            try
            {
                await _connection.StartAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public async Task Disconnect()
        {
            await _connection.DisposeAsync();

            _connection = new HubConnectionBuilder()
                .WithUrl(backendUrl)
                .Build();
        }
    }
}
