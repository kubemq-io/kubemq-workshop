## Build Node-js

### Get SDK

```
npm i kubemq-nodejs
```


### Create the Queue Client

```

const kubemq = require('kubemq-nodejs');

 //QueueName filled by rate-generator,  ClientID is the Id logged in kubemq, KubeMQServerAddress is the kubemq address cluster Proxy.
let QueueName = "rates-Queue", ClientID = "test-queue-client-id", KubeMQServerAddress = "localhost:50000";

//1. Create a kubeMQ Queue to dequeue rates from the workshop-rate-generator container.
let queue = new kubemq.Queue(KubeMQServerAddress, QueueName, ClientID);

```

### Get Rates from Queue

```

const kubemq = require('kubemq-nodejs');

 //QueueName filled by rate-generator,  ClientID is the Id logged in kubemq, KubeMQServerAddress is the kubemq address cluster Proxy.
let QueueName = "rates-Queue", ClientID = "test-queue-client-id", KubeMQServerAddress = "localhost:50000";


let queue = new kubemq.Queue(KubeMQServerAddress, QueueName, ClientID);


//loop function.
function myLoop () {
    //Set Timeout in milliseconds between function run time.
     setTimeout(function () {

          queue.receiveQueueMessages(32, 1).then(res => {
               if (res.Error) {
                   console.log('Message enqueue error, error:' + res.message);
               } else {
                   if (res.MessagesReceived) {                    
                       console.log('Received: ' + res.MessagesReceived);

                       res.Messages.forEach(element => {
                            console.log(element);
                       });
                   } else {
                       console.log('No messages');
                   }
                   myLoop();
               }
           }).catch(
               err => console.log('Error:' + err));
     }, 500)
  }
  myLoop();
```

### Send Rates to Events Store

```
const kubemq = require('kubemq-nodejs');

//QueueName filled by rate-generator,  ClientID is the Id logged in kubemq, KubeMQServerAddress is the kubemq address cluster Proxy.
let QueueName = "rates-Queue", ClientID = "test-queue-client-id", KubeMQServerAddress = "localhost:50000";

//1. Create a kubeMQ Queue to dequeue rates from the workshop-rate-generator container.
let queue = new kubemq.Queue(KubeMQServerAddress, QueueName, ClientID);
//2. Create store publisher for the kubemq store.
let storePub = new kubemq.StorePublisher('localhost', '50000', ClientID, 'ratesstore');

//loop function.
function myLoop () {
    //Set Timeout in milliseconds between function run time.
     setTimeout(function () {
		  // 3. Dequeue 32 messages from the queue with a wait of 1 second.
          queue.receiveQueueMessages(32, 1).then(res => {
               if (res.Error) {
                   console.log('Message enqueue error, error:' + res.message);
               } else {
                   if (res.MessagesReceived) {                    
                       console.log('Received: ' + res.MessagesReceived);
					    // 4. For each dequeued message will send a single event to store on the kubemq local storage.
                       res.Messages.forEach(element => {
						// 5. Create a new event and fill the event body, then Publish event to store to be consumed by the GUI client.
                            // Message body is a Json encoded by the workshop-rate-generator container.
                            let eventStore = new kubemq.StorePublisher.Event(element.Body);                                                                                                                                                                                                                                                                                                                                                                                             eventStore.Metadata = 'test store';
                            storePub.send(eventStore).then(res => {
                                console.log(res);
                            });
                       });
                   } else {
                       console.log('No messages');
                   }
                   myLoop();
               }
           }).catch(
               err => console.log('Error:' + err));
     }, 500)
  }
  //activeAllRates();
  myLoop();
```


