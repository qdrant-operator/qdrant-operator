namespace QdrantOperator
{
    internal class Constants
    {
        public const string Version                    = "0.0.4";
        public const string MinKubeVersion             = "1.24.0";
        public const string ManagedByLabel             = "app.kubernetes.io/managed-by";
        public const string ManagedBy                  = "qdrant-operator";
        public const string QdrantContainerName        = "qdrant";
        public const string QdrantConfig               = "qdrant-config";
        public const string QdrantSnapshots            = "qdrant-snapshots";
        public const string QdrantInit                 = "qdrant-init";
        public const string QdrantStorage              = "qdrant-storage";
        public const string QdrantStoragePath          = "/qdrant/storage";
        public const string OwnershipInitContainerName = "ensure-storage-dir-ownership";
        public const string Maturity                   = "beta";
        public const string DefaultChannel             = "beta";
        public const string ApiKeyEnvName              = "QDRANT__SERVICE__API_KEY";
        public const string ReadApiKeyEnvName          = "QDRANT__SERVICE__READ_ONLY_API_KEY";
        public const string ApiKeySecretKey            = "apiKey";
        public const string ReadApiKeySecretKey        = "readApiKey";


        public const string HttpPortName               = "http";
        public const string GrpcPortName               = "grpc";
        public const string P2pPortName                = "tcp-p2p";

        public const int RunAsUser  = 1000;
        public const int RunAsGroup = 2000;
        public const int FsGroup    = 3000;

        public const int HttpPort = 6333;
        public const int GrpcPort = 6334;
        public const int P2pPort = 6335;

        public const string ShortDescription = "The Qdrant Operator for Kubernetes is an operator for managing Qdrant Clusters in " +
            "a Kubernetes Cluster.";
        public const string FullDescription = $"The Qdrant Kubernetes Operator automates the deployment and management of Qdrant " +
            $"clusters on Kubernetes. The operator allows you to deploy clusters, create collections, define vectors and manage indexes.";
        public const string QdrantClusterDescription = $"QdrantCluster resources describe a Qdrant cluster";
        public const string QdrantCollectionDescription = $"QdrantCollection resources describe a collection. " +
            $"Each cluster can have many collections.";
        public const string QdrantCollectionFieldIndexDescription = $"A QdrantFieldIndex describes a field index for a collection. " +
            $"Each collection can have many field indexes.";

        public const string QdrantClusterExample = $@"apiVersion: qdrant.io/v1alpha1
kind: QdrantCluster
metadata:
  name: my-cluster
spec:
  image:
    repository: qdrant/qdrant
    pullPolicy: Always
    tag: v1.8.1
  persistence:
    size: 1Gi
    storageClassName: default
  replicas: 1";

        public const string QdrantCollectionExample = $@"apiVersion: qdrant.io/v1alpha1
kind: QdrantCollection
metadata:
  name: my-collection
spec:
  cluster: my-cluster
  replicationFactor: 1
  vectorSpec:
    size: 5
    onDisk: true";

        public const string QdrantFieldIndexExample = $@"apiVersion: qdrant.io/v1alpha1
kind: QdrantCollectionFieldIndex
metadata:
  name: my-collection-field-index
spec:
  cluster: my-cluster
  collection: my-collection
  fieldName: my-field
  type: text
  textIndexType:
    tokenizer: word
    minTokenLen: 1
    maxTokenLen: 10
    loweracase: true";

        public static string HeadlessServiceName(string resourceName)
        {
            return $"{resourceName}-headless";
        }
    }
}
