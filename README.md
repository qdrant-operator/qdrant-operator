<p style="text-align:center;" align="center">
  <img src="https://github.com/qdrant-operator/qdrant-operator/assets/11889627/b10e2e8e-ac37-416b-a7ed-64f7f2ac27b9" width="175" height="175"/></a>  
</p>

# Qdrant Operator for Kubernetes


[![.NET](https://github.com/qdrant-operator/qdrant-operator/actions/workflows/main.yml/badge.svg)](https://github.com/qdrant-operator/qdrant-operator/actions/workflows/main.yml)

## Introduction
The Qdrant Operator for Kubernetes is an operator for managing Qdrant Clusters in a Kubernetes Cluster. 

## Installation
The included helm chart contains the necessary permissions and configuration to run the operator in a Kubernetes cluster.

To install the operator, run the following command:
```sh
$> helm install qdrant-operator ./charts/qdrant-operator
```

Then deploy a QdrantCluster resource:
```yaml
apiVersion: qdrant.io/v1alpha1
kind: QdrantCluster
metadata:
  name: my-cluster
spec:
  image:
    repository: qdrant/qdrant
    pullPolicy: Always
    tag: v1.6.1
  persistence:
    size: 1Gi
    storageClassName: default
  replicas: 1
```

You can also add Collections to the Cluster by adding a QdrantCollection resource:
```yaml
apiVersion: qdrant.io/v1alpha1
kind: QdrantCollection
metadata:
  name: my-collection
spec:
  cluster: my-cluster
  replicationFactor: 1
  vectorSpec:
    size: 5
    onDisk: true
```

## More Information

For any questions, please join our Slack channel:

[![Slack](https://img.shields.io/badge/Slack-4A154B?style=for-the-badge&logo=slack&logoColor=white)](https://communityinviter.com/apps/qdrantoperator/invite)
