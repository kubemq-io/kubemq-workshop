package main

import (
	"context"
	"fmt"
	"log"

	"github.com/kubemq-io/kubemq-go"
)

func main() {
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
	i := 0
	channel := "testing_queue_channel"
	for {

		receiveResult, err := client.NewReceiveQueueMessagesRequest().
			SetChannel(channel).
			SetMaxNumberOfMessages(1).
			SetWaitTimeSeconds(1).
			Send(ctx)
		if err != nil {
			log.Fatal(err)
		}
		log.Printf("Received %d Messages:\n", receiveResult.MessagesReceived)
		for _, msg := range receiveResult.Messages {
			i++
			result, err := client.ES().
				SetId(fmt.Sprintf("event-store-%d", i)).
				SetChannel("channelName").
				SetMetadata("some-metadata").
				SetBody(msg.Body).
				AddTag("seq", fmt.Sprintf("%d", i)).
				Send(ctx)
			if err != nil {
				log.Fatal(err)
			}
			log.Printf("Sending event #%d: Result: %t", i, result.Sent)
		}
	}

}
