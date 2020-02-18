## Build Node-js

### Get SDK

```
pip install kubemq
```


### Create the Queue Client

```

from kubemq.queue.message_queue import MessageQueue
from kubemq.queue.message import Message
from kubemq.grpc import QueueMessagePolicy

queue_name = "rates-Queue" # queue_name filled by rate-generator.
client_id = "Queue-test" # client_id is the Id logged in kubemq.
kube_add = "localhost:50000" #kube_add is the kubemq address cluster Proxy.

queue=MessageQueue(queue_name, client_id, kube_add,32,1)


```

### Get Rates from Queue

```

from kubemq.queue.message_queue import MessageQueue
from kubemq.queue.message import Message
from kubemq.grpc import QueueMessagePolicy
import time


queue_name = "rates-Queue" # queue_name filled by rate-generator.
client_id = "Queue-test" # client_id is the Id logged in kubemq.
kube_add = "localhost:50000" #kube_add is the kubemq address cluster Proxy.

queue=MessageQueue(queue_name, client_id, kube_add,32,1)

while True:
    res=queue.receive_queue_messages()
    for message in res.messages:
        # message.Body is a Json encoded by the workshop-rate-generator container.
        print(message.Body)
        pass
    # Sleep between dequeue.
    pass
```

### Send Rates to Events Store

```
from kubemq.queue.message_queue import MessageQueue
from kubemq.queue.message import Message
from kubemq.grpc import QueueMessagePolicy
from kubemq.events.channel_parameters import ChannelParameters
from kubemq.events.channel import Channel
from kubemq.events.event import Event
import time



params = ChannelParameters(     
        channel_name="ratesstore", #channel_name is the event store pulished to be consumed by the GUI client.
        client_id="ratesstest", #client_id is the Id logged in kubemq.
        store=True,
        return_result=False,       
        kubemq_address="localhost:50000" #kubemq_address is the kubemq address cluster Proxy.
    )

channel = Channel(params=params)


queue_name = "rates-Queue" # queue_name filled by rate-generator.
client_id = "Queue-test" # client_id is the Id logged in kubemq.
kube_add = "localhost:50000" #kube_add is the kubemq address cluster Proxy.


queue=MessageQueue(queue_name, client_id, kube_add,32,1)

while True:

    res=queue.receive_queue_messages()
    for message in res.messages:
        # message.Body is a Json encoded by the workshop-rate-generator container.
        event = Event(body=message.Body, metadata="rate")
        channel.send_event(event)
        print("rate was sent")
        time.sleep(0.5)
        pass
    # Sleep between dequeue.
    pass

```


