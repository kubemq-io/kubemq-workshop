# kubemq-workshop

![alt text](https://i.imgur.com/NBYiMoZ.png)

In this workshop you will build an application that will receive messages from kubemq-queue and send them to event store.

You will need to listen to queue, take the body (a list of Rate struct , see at #Data) and send them to kubemq event-store .

From there the "Client" side will pick up the rates using kubemq-rest-websocket and will show them.

# Data

Queue Name to receive Messages from = "rates-Queue"

Store name to send the Messages to = "ratesstore"

### Proxy
kubemq-cluster:8080  - 127.0.0.1:8080

kubemq-cluster:9090  - 127.0.0.1:9090

kubemq-cluster:50000 - 127.0.0.1:50000

### Rate struct:

``{
  "id": 0,
  "name": "",
  "ask": "",
  "bid": ""
}``

### Client image:
workshop-client:v1.0

use port forwarding:
docker run -d -p 82:80 kubemq/workshop-client:v1.0.0


# Advance (Command)


![alt text](https://i.imgur.com/nWgTGol.png)


Can use Command request to start rates when AutoStart is set to false.
Need to send to Channel name :"rateCMD".
No special body or metadata required .


### Rate Generator Image:
kubemq/workshop-rate-generator:v1.0.0

# Rate Generator Env var :

KubemqAddress - Kubemq Address *Default will be - localhost:50000.

KubemqClient  - The name of the client that the program will signed under *Default will be - ratesstest.
  
KubemqQueue   - The name of the queue the rates will publish to *Default will be - rates-Queue.

CMDChannel    - The name of the channel to sign under for commands to start/stop *Default will be - rateCMD.

AutoStart     - Advance - if needed to send rate on startup *Default will be - true.

RateInterval  - The interval that rates will be sent under *Default will be - 500.

