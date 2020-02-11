
const kubemq = require('kubemq-nodejs');
let storePub = new kubemq.StorePublisher('localhost', '50000', 'ratesstest', 'ratesstore');

let queue = new kubemq.Queue('localhost:50000', 'rates-Queue', 'test-queue-client');

function myLoop () {
     setTimeout(function () {
          queue.receiveQueueMessages(2, 1).then(res => {
               if (res.Error) {
                   console.log('Message enqueue error, error:' + res.message);
               } else {
                   if (res.MessagesReceived) {
                       console.log('Received: ' + res.MessagesReceived);
                       res.Messages.forEach(element => {
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
     }, 1000)
  }

  myLoop();