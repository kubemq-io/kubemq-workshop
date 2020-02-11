using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace kubemq_workshop_csharp
{
    class Program
    {
        public static Random rnd;
        private static KubeMQ.SDK.csharp.Events.Channel channel;
        static void Main(string[] args)
        {
            rnd = new Random();

            string QueueName = "rates-Queue";
            string ClientID = "test-queue-client-id";
            string KubeMQServerAddress = "localhost:50000";
            Console.WriteLine($"Started workshop");
            channel = new KubeMQ.SDK.csharp.Events.Channel(new KubeMQ.SDK.csharp.Events.ChannelParameters
            {
                ChannelName = "ratesstore",
                ClientID = "ratesstest",
                Store = true,
                KubeMQAddress = "localhost:50000"
            }); ;

            KubeMQ.SDK.csharp.Queue.Queue queue = null;
            try
            {
                queue = new KubeMQ.SDK.csharp.Queue.Queue(QueueName, ClientID, KubeMQServerAddress);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            while (true) {
                try
                {
                    KubeMQ.SDK.csharp.Queue.ReceiveMessagesResponse msg = queue.ReceiveQueueMessages(1000);
                    if (msg.IsError)
                    {
                        Console.WriteLine($"message dequeue error, error:{msg.Error}");
                        return;
                    }
                    Console.WriteLine($"Received {msg.MessagesReceived} Messages:");

                    foreach (KubeMQ.SDK.csharp.Queue.Message item in msg.Messages)
                    {
                        Console.WriteLine($"MessageID: {item.MessageID}, Body:{Encoding.UTF8.GetString(item.Body)}");
                        channel.SendEvent(new KubeMQ.SDK.csharp.Events.Event
                        {
                            Body = item.Body,
                            Metadata = "Rate message json encoded in UTF8",
                            EventID = rnd.Next(1000).ToString()
                        }); ;
                        Console.WriteLine("Rate was sent");
                        Thread.Sleep(500);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }

}
