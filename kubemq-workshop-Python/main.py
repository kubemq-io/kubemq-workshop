from kubemq.queue.message_queue import MessageQueue
from kubemq.queue.message import Message
from kubemq.grpc import QueueMessagePolicy
from kubemq.events.channel_parameters import ChannelParameters
from kubemq.events.channel import Channel
from kubemq.events.event import Event
import time

params = ChannelParameters(
        channel_name="ratesstore",
        client_id="ratesstest",
        store=True,
        return_result=False,
        kubemq_address="localhost:50000"
    )

channel = Channel(params=params)

queue_name = "rates-Queue"
client_id = "Queue-test"
kube_add = "localhost:50000"
queue=MessageQueue(queue_name, client_id, kube_add)

while True:

    res=queue.receive_queue_messages()

    for message in res.messages:
        event = Event(body=message.Body, metadata="rate")
        channel.send_event(event)
        print("rate was sent")
        pass
    time.sleep(2.0)
    pass
