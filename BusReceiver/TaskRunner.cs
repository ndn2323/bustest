using Azure.Messaging.ServiceBus;
using System.Text;

namespace BusReceiver
{
    public class TaskRunner
    {
        public TaskRunner() { }
        public async Task Run() {            
            const string DLQPATH = "/$deadletterqueue";
            var maxMsgCount = 50;
            var connectionString = "[ConnectionString]";
            var topicName = "testtopic1";
            var subscriberName = "testsub1";
            var subscriberDlqName = subscriberName + DLQPATH;
            var client = new ServiceBusClient(connectionString);
            var options = new ServiceBusReceiverOptions();
            options.ReceiveMode = ServiceBusReceiveMode.PeekLock;
            var receiver = client.CreateReceiver(topicName, subscriberName, options);
            var receiverDlq = client.CreateReceiver(topicName, subscriberDlqName, options);

            Log("Starting receive from regular queue");
            var msgList = await receiver.ReceiveMessagesAsync(maxMsgCount, TimeSpan.FromMilliseconds(500));
            Log(msgList.Count.ToString() + " messages found");
            foreach (var msg in msgList)
            {
                await receiver.DeadLetterMessageAsync(msg);
            }

            Log("Starting receive from dead letter queue");
            var msgListDlq = await receiverDlq.ReceiveMessagesAsync(maxMsgCount, TimeSpan.FromMilliseconds(500));            
            Log(msgListDlq.Count.ToString() + " messages found in dlq");
            foreach (var msg in msgListDlq) {
                Log("MessageId: " + msg.MessageId + " Body: " + Encoding.ASCII.GetString(msg.Body));
                // if some condition, archieve message to some data store, else abandon it to be picked up again
                // for this test I'm abandoning all messages                
                await receiverDlq.AbandonMessageAsync(msg);
            }

            await receiver.CloseAsync();
            await receiverDlq.CloseAsync();
        }

        private void Log(string msg) {
            Console.WriteLine(DateTime.Now.ToString() + ": " + msg);
        }
    }
}
