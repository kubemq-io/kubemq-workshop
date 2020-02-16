from kubemq.queue.message_queue import MessageQueue
from kubemq.queue.message import Message
from kubemq.grpc import QueueMessagePolicy
from kubemq.events.channel_parameters import ChannelParameters
from kubemq.events.channel import Channel
from kubemq.events.event import Event
from kubemq.commandquery.lowlevel.initiator import Initiator
from kubemq.commandquery.lowlevel.request import Request
from kubemq.commandquery.request_type import RequestType
import time


# 7.Optional - start sending rates command
def activeAllRates(kubeAdd):
    initiator = Initiator(kubeAdd)
    request  = Request(
        body="start".encode('UTF-8'),
        metadata="start", 
        channel="rateCMD",
        client_id="start",
        timeout=1000,
        request_type=RequestType.Command,
    )
    try:
        response = initiator.send_request(request)
        print('Response Received:%s Executed at::%s'  % (
            response.request_id,
            response.timestamp
                    ))
    except Exception as err :
        print('command error::%s'  % (
            err
                    ))


# 1.Create Channel Parameters with store active.
params = ChannelParameters(     
        channel_name="ratesstore", #channel_name is the event store pulished to be consumed by the GUI client.
        client_id="ratesstest", #client_id is the Id logged in kubemq.
        store=True,
        return_result=False,       
        kubemq_address="localhost:50000" #kubemq_address is the kubemq address cluster Proxy.
    )

# 2. Initiate an event store channel to publish rate events using the <params>.
channel = Channel(params=params)


queue_name = "rates-Queue" # queue_name filled by rate-generator.
client_id = "Queue-test" # client_id is the Id logged in kubemq.
kube_add = "localhost:50000" #kube_add is the kubemq address cluster Proxy.

# Optional
# activeAllRates(kube_add)

# 3. Create a kubeMQ Queue to dequeue rates from the rateprovide container that will dequeue 32 messages from the queue with a wait of 1 second.
queue=MessageQueue(queue_name, client_id, kube_add,32,1)

while True:

# 4. Receiving up to 32 messages.
    res=queue.receive_queue_messages()
# 5. For each dequeued message will send a single event to store on the kubemq
    for message in res.messages:
# 6. Create and publish the events to kubemq store.
        # message.Body is a Json encoded by the workshop-rate-generator container.
        event = Event(body=message.Body, metadata="rate")
        channel.send_event(event)
        print("rate was sent")
        time.sleep(0.5)
        pass
    # Sleep between dequeue.
    pass
