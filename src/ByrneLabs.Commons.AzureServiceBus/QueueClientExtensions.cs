using System;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ByrneLabs.Commons.AzureServiceBus
{
    [PublicAPI]
    public static class QueueClientExtensions
    {
        public static Task SendJsonAsync(this IQueueClient client, object messageBody)
        {
            var json = JToken.FromObject(messageBody).ToString();
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            var message = new Message(jsonBytes) { ContentType = "application/json" };
            return client.SendAsync(message);
        }

        public static Task ProcessJsonQueue<T>(this IQueueClient queueClient, MessageHandlerOptions messageHandlerOptions, Action<T> processor)
        {
            queueClient.RegisterMessageHandler(async (message, token) =>
            {
                var messageBodyText = Encoding.UTF8.GetString(message.Body);
                var messageBodyObject = JToken.Parse(messageBodyText).ToObject<T>();
                
                processor(messageBodyObject);

                await queueClient.CompleteAsync(message.SystemProperties.LockToken).ConfigureAwait(false);                
            }, messageHandlerOptions);

            return queueClient.CloseAsync();
        }
    }
}
