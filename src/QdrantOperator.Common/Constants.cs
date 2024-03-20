namespace QdrantOperator
{

  /// <summary>
    /// Constants used in the QdrantOperator namespace.
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// The version of the Qdrant Operator.
        /// </summary>
        public const string Version = "1.0.0";

        /// <summary>
        /// The minimum version of Kubernetes required by the Qdrant Operator.
        /// </summary>
        public const string MinKubeVersion = "1.24.0";

        /// <summary>
        /// The label used to indicate that a resource is managed by the Qdrant Operator.
        /// </summary>
        public const string ManagedByLabel = "app.kubernetes.io/managed-by";

        /// <summary>
        /// The value of the managed by label for resources managed by the Qdrant Operator.
        /// </summary>
        public const string ManagedBy = "qdrant-operator";

        /// <summary>
        /// The name of the Qdrant container.
        /// </summary>
        public const string QdrantContainerName = "qdrant";

        /// <summary>
        /// The name of the Qdrant config.
        /// </summary>
        public const string QdrantConfig = "qdrant-config";

        /// <summary>
        /// The name of the Qdrant snapshots.
        /// </summary>
        public const string QdrantSnapshots = "qdrant-snapshots";

        /// <summary>
        /// The name of the Qdrant init container.
        /// </summary>
        public const string QdrantInit = "qdrant-init";

        /// <summary>
        /// The name of the Qdrant storage.
        /// </summary>
        public const string QdrantStorage = "qdrant-storage";

        /// <summary>
        /// The path to the Qdrant storage.
        /// </summary>
        public const string QdrantStoragePath = "/qdrant/storage";

        /// <summary>
        /// The name of the init container used to ensure ownership of the storage directory.
        /// </summary>
        public const string OwnershipInitContainerName = "ensure-storage-dir-ownership";

        /// <summary>
        /// The maturity level of the Qdrant Operator.
        /// </summary>
        public const string Maturity = "beta";

        /// <summary>
        /// The default channel for the Qdrant Operator.
        /// </summary>
        public const string DefaultChannel = "beta";

        /// <summary>
        /// The name of the environment variable used to specify the API key for the Qdrant service.
        /// </summary>
        public const string ApiKeyEnvName = "QDRANT__SERVICE__API_KEY";

        /// <summary>
        /// The name of the environment variable used to specify the read-only API key for the Qdrant service.
        /// </summary>
        public const string ReadApiKeyEnvName = "QDRANT__SERVICE__READ_ONLY_API_KEY";

        /// <summary>
        /// The secret key used to store the API key.
        /// </summary>
        public const string ApiKeySecretKey = "apiKey";

        /// <summary>
        /// The secret key used to store the read-only API key.
        /// </summary>
        public const string ReadApiKeySecretKey = "readApiKey";

        /// <summary>
        /// The group used for Kubernetes resources created by the Qdrant Operator.
        /// </summary>
        public const string KubernetesGroup = "qdrantoperator.io";

        /// <summary>
        /// The name of the HTTP port.
        /// </summary>
        public const string HttpPortName = "http";

        /// <summary>
        /// The name of the gRPC port.
        /// </summary>
        public const string GrpcPortName = "grpc";

        /// <summary>
        /// The name of the P2P port.
        /// </summary>
        public const string P2pPortName = "tcp-p2p";

        /// <summary>
        /// The user ID used to run the Qdrant Operator.
        /// </summary>
        public const int RunAsUser = 1000;

        /// <summary>
        /// The group ID used to run the Qdrant Operator.
        /// </summary>
        public const int RunAsGroup = 2000;

        /// <summary>
        /// The file system group ID used by the Qdrant Operator.
        /// </summary>
        public const int FsGroup = 3000;

        /// <summary>
        /// The default HTTP port number.
        /// </summary>
        public const int HttpPort = 6333;

        /// <summary>
        /// The default gRPC port number.
        /// </summary>
        public const int GrpcPort = 6334;

        /// <summary>
        /// The default P2P port number.
        /// </summary>
        public const int P2pPort = 6335;

        /// <summary>
        /// The short description of the Qdrant Operator.
        /// </summary>
        public const string ShortDescription = "The Qdrant Operator for Kubernetes is an operator for managing Qdrant Clusters in a Kubernetes Cluster.";

        /// <summary>
        /// The full description of the Qdrant Operator.
        /// </summary>
        public const string FullDescription = "The Qdrant Kubernetes Operator automates the deployment and management of Qdrant clusters on Kubernetes. The operator allows you to deploy clusters, create collections, define vectors and manage indexes.";

        /// <summary>
        /// The description of a QdrantCluster resource.
        /// </summary>
        public const string QdrantClusterDescription = "QdrantCluster resources describe a Qdrant cluster";

        /// <summary>
        /// The description of a QdrantCollection resource.
        /// </summary>
        public const string QdrantCollectionDescription = "QdrantCollection resources describe a collection. Each cluster can have many collections.";

        /// <summary>
        /// The description of a QdrantFieldIndex resource.
        /// </summary>
        public const string QdrantCollectionFieldIndexDescription = "A QdrantFieldIndex describes a field index for a collection. Each collection can have many field indexes.";

        /// <summary>
        /// An example of a QdrantCluster resource.
        /// </summary>
        public const string QdrantClusterExample = @"apiVersion: qdrantoperator.io/v1alpha1
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

        /// <summary>
        /// An example of a QdrantCollection resource.
        /// </summary>
        public const string QdrantCollectionExample = @"apiVersion: qdrantoperator.io/v1alpha1
    kind: QdrantCollection
    metadata:
      name: my-collection
    spec:
      cluster: my-cluster
      replicationFactor: 1
      vectorSpec:
        size: 5
        onDisk: true";

        /// <summary>
        /// An example of a QdrantFieldIndex resource.
        /// </summary>
        public const string QdrantFieldIndexExample = @"apiVersion: qdrantoperator.io/v1alpha1
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

        /// <summary>
        /// Generates the name of the headless service for a given resource name.
        /// </summary>
        /// <param name="resourceName">The name of the resource.</param>
        /// <returns>The name of the headless service.</returns>
        public static string HeadlessServiceName(string resourceName)
        {
            return $"{resourceName}-headless";
        }
    }
}
