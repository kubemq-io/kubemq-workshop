## Build Java

### Get SDK

```
add dependency to pom.xml
 	<dependency>
      <groupId>io.kubemq.sdk</groupId>
      <artifactId>kubemq-sdk-Java</artifactId>
      <version>1.0.2</version>
	</dependency>
```




### Get Rates from Queue

```
package io.kubemq.workshop;

import javax.net.ssl.SSLException;

import io.kubemq.sdk.basic.ServerAddressNotSuppliedException;

public class Get_Rates_from_Queue {
    public static void main(final String[] args) throws SSLException, ServerAddressNotSuppliedException {
        String QueueName = "rates-Queue", ClientID = "test-queue-client-id", KubeMQServerAddress = "localhost:50000";
        io.kubemq.sdk.queue.Queue queue = new io.kubemq.sdk.queue.Queue(QueueName, ClientID, KubeMQServerAddress);
        io.kubemq.sdk.queue.ReceiveMessagesResponse resRec = queue.ReceiveQueueMessages(32, 1);
        if (resRec.getIsError()) {
            System.out.printf("Message dequeue error, error: %s", resRec.getError());
            return;
        }
        System.out.printf("Received Messages :%s\n", resRec.getMessagesReceived());
    }
}

```

### Send Rates to Events Store

```
package io.kubemq.workshop;

import java.io.UnsupportedEncodingException;

import javax.net.ssl.SSLException;

import io.kubemq.sdk.basic.ServerAddressNotSuppliedException;

public class Send_Rates_to_Events_Store {
    public static void main(final String[] args) throws UnsupportedEncodingException {
        String ClientID = "test-queue-client-id", KubeMQServerAddress = "localhost:50000";

        io.kubemq.sdk.event.Channel channel = new io.kubemq.sdk.event.Channel("ratesstore", ClientID, true,
                KubeMQServerAddress);

        io.kubemq.sdk.event.Event event = new io.kubemq.sdk.event.Event();
        event.setBody("I am body".getBytes("UTF8"));
        try {
            channel.SendEvent(event);
        } catch (SSLException e) {
            System.out.printf("SSLException: %s", e.getMessage());
            e.printStackTrace();
        } catch (ServerAddressNotSuppliedException e) {
            System.out.printf("ServerAddressNotSuppliedException: %s", e.getMessage());
            e.printStackTrace();
        }
    }
}

```


