package io.kubemq.workshop;

import java.io.IOException;

import javax.net.ssl.SSLException;

import io.kubemq.sdk.basic.ServerAddressNotSuppliedException;

import io.kubemq.sdk.event.Event;
import io.kubemq.sdk.queue.Message;
import io.kubemq.sdk.queue.Queue;
import io.kubemq.sdk.queue.ReceiveMessagesResponse;
import io.kubemq.sdk.tools.Converter;

/**
 * Hello world!
 *
 */
public class App {
    public static void main(String[] args) throws IOException, ServerAddressNotSuppliedException, InterruptedException {
        System.out.println("Hello World!");
       
        //QueueName filled by rate-generator,  ClientID is the Id logged in kubemq, KubeMQServerAddress is the kubemq address cluster Proxy.
        String QueueName = "rates-Queue", ClientID = "test-queue-client-id", KubeMQServerAddress = "localhost:50000";

        System.out.println("Started rate_generate");
        // 1. First create a kubeMQ Queue to dequeue rates from the workshop-rate-generator container.
        Queue queue = new Queue(QueueName, ClientID, KubeMQServerAddress);
        // 2. Initiate an event store channel to publish rate events.
        io.kubemq.sdk.event.Channel channel = new io.kubemq.sdk.event.Channel("ratesstore", ClientID, true,
                KubeMQServerAddress);

        while (true) {
        // 3. Dequeue 32 messages from the queue with a wait of 1 second.
            ReceiveMessagesResponse resRec = queue.ReceiveQueueMessages(32, 1);
            if (resRec.getIsError()) {
                System.out.printf("Message dequeue error, error: %s", resRec.getError());
                continue;
            }

            System.out.printf("Received Messages :%s\n", resRec.getMessagesReceived());

        // 4. For each dequeued message will send a single event to store on the kubemq local storage.
            for (Message msg : resRec.getMessages()) {
                // Message body is byte[] of a Json encoded utf-8 by the workshop-rate-generator container.
                System.out.printf("MessageID: %s, Body:%s", msg.getMessageID(), new String(msg.getBody()), "UTF_8");
        // 5. Create a new event and fill the event body.
                Event event = new Event();
                event.setBody(msg.getBody());
                try {
        // 6. Publish event to store to be consumed by the GUI client.
                    channel.SendEvent(event);
                } catch (SSLException e) {
                    System.out.printf("SSLException: %s", e.getMessage());
                    e.printStackTrace();
                } catch (ServerAddressNotSuppliedException e) {
                    System.out.printf("ServerAddressNotSuppliedException: %s", e.getMessage());
                    e.printStackTrace();
                }
            }
            // Sleep between dequeue.
            Thread.sleep(500);
        }
    }

    // 7. Optional - start sending rates command
    private static void activeAllRates(String KubemqServerAddress) throws IOException, ServerAddressNotSuppliedException {
        io.kubemq.sdk.commandquery.ChannelParameters channelParameters = new io.kubemq.sdk.commandquery.ChannelParameters();
        channelParameters.setChannelName("rateCMD");
        channelParameters.setClientID("start");
        channelParameters.setKubeMQAddress(KubemqServerAddress);
        channelParameters.setRequestType(io.kubemq.sdk.commandquery.RequestType.Command);
        channelParameters.setTimeout(1000);
        io.kubemq.sdk.commandquery.Channel channel = new io.kubemq.sdk.commandquery.Channel(channelParameters);
        io.kubemq.sdk.commandquery.Request request = new  io.kubemq.sdk.commandquery.Request();
        request.setBody(Converter.ToByteArray("start"));
        io.kubemq.sdk.commandquery.Response result = channel.SendRequest(request);
        if (!result.isExecuted()) {
            System.out.printf("Response error: %s", result.getError());
            return;
        }
        System.out.printf("Response Received: %s, ExecutedAt: %s", result.getRequestID(), result.getTimestamp());
    }
}
