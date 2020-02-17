## Build csharp

### Get SDK

```
 Install-Package KubeMQ.SDK.csharp -Version 1.0.8
 
```




### Get Rates from Queue

```
using System;

namespace kubemq_workshop_csharp {
    class Program {

        private static KubeMQ.SDK.csharp.Events.Channel channel;
        static void Main (string[] args) {
            string QueueName = "rates-Queue";
            string ClientID = "test-queue-client-id";
            string KubeMQServerAddress = "localhost:50000";

            KubeMQ.SDK.csharp.Queue.Queue queue = null;
            try {
                queue = new KubeMQ.SDK.csharp.Queue.Queue (QueueName, ClientID, 32, 1, KubeMQServerAddress);
            } catch (Exception ex) {
                Console.WriteLine (ex.Message);
            }

            KubeMQ.SDK.csharp.Queue.ReceiveMessagesResponse msg = queue.ReceiveQueueMessages ();
            if (msg.IsError) {
                Console.WriteLine ($"message dequeue error, error:{msg.Error}");
                return;
            }
            Console.WriteLine ($"Received {msg.MessagesReceived} Messages:");
        }
    }

}

```

### Send Rates to Events Store

```
using System;

namespace kubemq_workshop_csharp {
    class Program {

        private static KubeMQ.SDK.csharp.Events.Channel channel;
        static void Main (string[] args) {

            string ClientID = "test-queue-client-id";
            string KubeMQServerAddress = "localhost:50000";

            channel = new KubeMQ.SDK.csharp.Events.Channel (new KubeMQ.SDK.csharp.Events.ChannelParameters {
                ChannelName = "ratesstore",
                    ClientID = ClientID,
                    Store = true,
                    KubeMQAddress = KubeMQServerAddress
            });

            channel.SendEvent (new KubeMQ.SDK.csharp.Events.Event {
                Body = System.Text.UTF8Encoding.UTF8.GetBytes ("I amd body"),
                    Metadata = "Rate message json encoded in UTF8"
            });
            Console.WriteLine ("Rate was sent");

        }
    }
}

```


