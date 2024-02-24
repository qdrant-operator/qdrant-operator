namespace QdrantOperator
{
    public class Constants
    {
        public const string ManagedByLabel = "app.kubernetes.io/managed-by";
        public const string ManagedBy = "qdrant-operator";
        public const string QdrantContainerName = "qdrant";
        public const string QdrantConfig = "qdrant-config";
        public const string QdrantSnapshots = "qdrant-snapshots";
        public const string QdrantInit = "qdrant-init";
        public const string QdrantStorage = "qdrant-storage";
        public const string QdrantStoragePath = "/qdrant/storage";
        public const string OwnershipInitContainerName = "ensure-storage-dir-ownership";
        public const string ShortDescription = "The Qdrant Operator for Kubernetes is an operator for managing Qdrant Clusters in " +
            "a Kubernetes Cluster.";
        public const string FullDescription = $"The Qdrant Kubernetes Operator automates the deployment and management of Qdrant " +
            $"clusters on Kubernetes. The operator allows you to deploy clusters, create collections, define vectors and manage indexes.";
        public const string QdrantClusterDescription = $"QdrantCluster resources describe a Qdrant cluster";
        public const string QdrantCollectionDescription = $"QdrantCollection resources describe a collection. " +
            $"Each cluster can have many collections.";
        public const string QdrantCollectionFieldIndexDescription = $"A QdrantFieldIndex describes a field index for a collection. " +
            $"Each collection can have many field indexes.";


        public static string HeadlessServiceName(string resourceName)
        {
            return $"{resourceName}-headless";
        }
    }
}
