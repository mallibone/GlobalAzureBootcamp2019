using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.EventHubs;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace Server.IoTMonitor
{
    public static class IoTPlaygroundLogic
    {
        [FunctionName("ProcessIoTHubEvent")]
        public static async Task ProcessIoTHubEvent(
            [IoTHubTrigger("messages/events", Connection = "ConnectionString")]EventData message, 
            [SignalR(HubName = "IoTPlayground")] IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log)
        {
            var payload = Encoding.UTF8.GetString(message.Body.Array);
            log.LogInformation($"C# IoT Hub trigger function processed a message: {payload}");
            await signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "measurement",
                    Arguments = new[] { payload }
                });

            return;
        }

        [FunctionName("Negotiate")]
        public static SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous)]HttpRequest req,
            [SignalRConnectionInfo(HubName = "IoTPlayground")] SignalRConnectionInfo connectionInfo)
        {
            // connectionInfo contains an access key token with a name identifier claim set to the authenticated user
            return connectionInfo;
        }
    }
}