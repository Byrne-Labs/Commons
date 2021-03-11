using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json.Linq;

namespace ByrneLabs.Commons.AzureServiceBus
{
    public static class TopicClientExtensions
    {
        public static Task SendJsonAsync(this ITopicClient client, object messageBody)
        {
            var json = JToken.FromObject(messageBody).ToString();
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            var message = new Message(jsonBytes) { ContentType = "application/json" };
            return client.SendAsync(message);
        }
    }
}
