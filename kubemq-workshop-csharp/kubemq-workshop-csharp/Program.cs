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
     
        private static KubeMQ.SDK.csharp.Events.Channel channel;
        static void Main(string[] args)
        {
            //QueueName filled by rate-generator.
            string QueueName = "rates-Queue";
            //ClientID is the Id logged in kubemq.
            string ClientID = "test-queue-client-id";
            //KubeMQServerAddress is the kubemq address cluster Proxy.
            string KubeMQServerAddress = "localhost:50000";

            //activeAllRates(KubeMQServerAddress);

            Console.WriteLine($"Started workshop");
// 1. Initiate an event store channel to publish rate events.
            channel = new KubeMQ.SDK.csharp.Events.Channel(new KubeMQ.SDK.csharp.Events.ChannelParameters
            {
                ChannelName = "ratesstore",
                ClientID = "ratesstest",
                Store = true,
                KubeMQAddress = "localhost:50000"
            }); ;
// 2. Create a kubeMQ Queue to dequeue rates from the workshop-rate-generator container that will dequeue 32 messages from the queue with a wait of 1 second.
            KubeMQ.SDK.csharp.Queue.Queue queue = null;
            try
            {
                queue = new KubeMQ.SDK.csharp.Queue.Queue(QueueName, ClientID, 32,1,KubeMQServerAddress);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            while (true) {
                try
                {
// 3. Dequeue 32 messages from the queue with a wait of 1 second.
                    KubeMQ.SDK.csharp.Queue.ReceiveMessagesResponse msg = queue.ReceiveQueueMessages();
                    if (msg.IsError)
                    {
                        Console.WriteLine($"message dequeue error, error:{msg.Error}");
                        return;
                    }
                    Console.WriteLine($"Received {msg.MessagesReceived} Messages:");
// 4. For each dequeued message will send a single event to store on the kubemq local storage.
                    foreach (KubeMQ.SDK.csharp.Queue.Message item in msg.Messages)
                    {
                        // Message body is byte[] of a Json encoded utf-8 by the workshop-rate-generator container.
                        Console.WriteLine($"MessageID: {item.MessageID}, Body:{Encoding.UTF8.GetString(item.Body)}");
// 5. Create a new event and fill the event body, then publish event to store to be consumed by the GUI client.
                        channel.SendEvent(new KubeMQ.SDK.csharp.Events.Event
                        {
                            Body = item.Body,
                            Metadata = "Rate message json encoded in UTF8"                     
                        }); ;
                        Console.WriteLine("Rate was sent");
                        // Sleep between dequeue.
                        Thread.Sleep(500);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

// 6. Optional - start sending rates command
        private static void activeAllRates(string KubemqServerAddress)
        {
            KubeMQ.SDK.csharp.CommandQuery.Channel channel = new KubeMQ.SDK.csharp.CommandQuery.Channel(new KubeMQ.SDK.csharp.CommandQuery.ChannelParameters
            {
                RequestsType = KubeMQ.SDK.csharp.CommandQuery.RequestType.Command,
                Timeout = 1000,
                ChannelName = "rateCMD",
                ClientID = "start",
                KubeMQAddress = KubemqServerAddress
            });
            try
            {

                KubeMQ.SDK.csharp.CommandQuery.Response result = channel.SendRequest(new KubeMQ.SDK.csharp.CommandQuery.Request
                {
                    Body = KubeMQ.SDK.csharp.Tools.Converter.ToByteArray("start")
                });

                if (!result.Executed)
                {
                    Console.WriteLine($"Response error:{result.Error}");
                    return;
                }
                Console.WriteLine($"Response Received:{result.RequestID} ExecutedAt:{result.Timestamp}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return;
        }
    }

}
