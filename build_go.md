## Build Go

### Get SDK

```
go get -u github.com/kubemq-io/kubemq-go
```


### Create KubeMQ Client

```
package main

import (
	"context"
	"fmt"
	"log"
	"time"

	"github.com/kubemq-io/kubemq-go"
)

func main() {
	// 1. Create kubemq client.
	ctx, cancel := context.WithCancel(context.Background())
	defer cancel()
	client, err := kubemq.NewClient(ctx,
		kubemq.WithAddress("localhost", 50000),
		kubemq.WithClientId("test-command-client-id"),
		kubemq.WithTransportType(kubemq.TransportTypeGRPC))
	if err != nil {
		log.Fatal(err)
	}
	defer client.Close()
	//startSendingRates(ctx, client)
}

```

### Get Rates from Queue

```
package main

import (
	"context"
	"fmt"
	"log"
	"time"

	"github.com/kubemq-io/kubemq-go"
)

func main() {
	// 1. Create kubemq client.
	ctx, cancel := context.WithCancel(context.Background())
	defer cancel()
	client, err := kubemq.NewClient(ctx,
		kubemq.WithAddress("localhost", 50000),
		kubemq.WithClientId("test-command-client-id"),
		kubemq.WithTransportType(kubemq.TransportTypeGRPC))
	if err != nil {
		log.Fatal(err)
	}
	defer client.Close()
	//startSendingRates(ctx, client)
	i := 0
	queueName := "rates-Queue"
	for {
		// 2. Receive queue messages from queue <queueName> with max number of messages of 32 and wait time of 1 second.
		receiveResult, err := client.NewReceiveQueueMessagesRequest().
			SetChannel(queueName).
			SetMaxNumberOfMessages(32).
			SetWaitTimeSeconds(1).
			Send(ctx)
		if err != nil {
			log.Fatal(err)
		}
		log.Printf("Received %d Messages:\n", receiveResult.MessagesReceived)
		//Wait time.
		time.Sleep(500 * time.Millisecond)
	}

}
```

### Send Rates to Events Store

```
package main

import (
	"context"
	"fmt"
	"log"
	"time"

	"github.com/kubemq-io/kubemq-go"
)

func main() {
	// 1. Create kubemq client.
	ctx, cancel := context.WithCancel(context.Background())
	defer cancel()
	client, err := kubemq.NewClient(ctx,
		kubemq.WithAddress("localhost", 50000),
		kubemq.WithClientId("test-command-client-id"),
		kubemq.WithTransportType(kubemq.TransportTypeGRPC))
	if err != nil {
		log.Fatal(err)
	}
	defer client.Close()
	//startSendingRates(ctx, client)
	i := 0
	queueName := "rates-Queue"
	for {
		// 2. Receive queue messages from queue <queueName> with max number of messages of 32 and wait time of 1 second.
		receiveResult, err := client.NewReceiveQueueMessagesRequest().
			SetChannel(queueName).
			SetMaxNumberOfMessages(32).
			SetWaitTimeSeconds(1).
			Send(ctx)
		if err != nil {
			log.Fatal(err)
		}
		log.Printf("Received %d Messages:\n", receiveResult.MessagesReceived)
		// 3.For each dequeued message will send a single event to store on the kubemq
		for _, msg := range receiveResult.Messages {
			i++
			// 4.Create and publish the events to kubemq store.
			result, err := client.ES().
				SetId(fmt.Sprintf("event-store-%d", i)).
				SetChannel("ratesstore").
				SetMetadata("some-metadata").
				SetBody(msg.Body).
				AddTag("seq", fmt.Sprintf("%d", i)).
				Send(ctx)
			if err != nil {
				log.Fatal(err)
			}
			log.Printf("Sending event #%d: Result: %t", i, result.Sent)
		}
		//Wait time.
		time.Sleep(500 * time.Millisecond)
	}

}
```


