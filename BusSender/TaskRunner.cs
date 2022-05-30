using Azure.Messaging.ServiceBus;

namespace BusSender
{
    public class TaskRunner
    {
        public TaskRunner() { }
        public async Task Run() {
            var connectionString = "[ConnectionString]";
            var topicName = "testtopic1";
            var client = new ServiceBusClient(connectionString);
            var sender = client.CreateSender(topicName);
            var msg = new ServiceBusMessage("TestMessage");
            await sender.SendMessageAsync(msg);
            await sender.CloseAsync();
        }
    }
}
