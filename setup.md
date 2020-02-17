# KubeMQ Workshop

## Setup

### Deploy KubeMQ and support containers

From your workshop repository:
```
kubectl apply -f ./deploy/kubemq.yaml
```
### Kubemqctl
Kubemqctl is the cli tool for kubemq.  
Get it:
- Get it from workshop repository, kubemqctl folder
- Download it (mac / linux):

```
 sudo curl -sL https://get.kubemq.io/install | sudo sh
```

### Check proper installation
```
λ kubemqctl cluster get
Getting KubeMQ Cluster List...
Current Kubernetes cluster context connection: kind-kind
NAME                   DESIRED  RUNNING  READY  IMAGE                 AGE     SERVICES
kubemq/kubemq-cluster  3        3        3      kubemq/kubemq:v2.0.0  51m31s  ClusterIP 10.96.241.62:5228, ClusterIP 10.96.125.94:8080, ClusterIP 10.96.203.116:50000, ClusterIP 10.96.84.253:9090
```

Browse:

```
http://localhost:31000/#/main
```
### Proxy KubeMQ Ports
```
 λ kubemqctl cluster proxy
Current Kubernetes cluster context connection: docker-for-desktop
? Select KubeMQ cluster to Proxy kubemq/kubemq-cluster
Current Kubernetes cluster context connection: docker-for-desktop
Connecting To Kuberenets Cluster... Ok.
Start proxy for kubemq/kubemq-cluster-2. press CTRL C to close.
Kubemq/Kubemq-Cluster-2:8080 -> 127.0.0.1:8080
Kubemq/Kubemq-Cluster-2:9090 -> 127.0.0.1:9090
Kubemq/Kubemq-Cluster-2:50000 -> 127.0.0.1:50000
```
