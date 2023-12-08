using Microsoft.AspNetCore.Routing;

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

        public static string HeadlessServiceName(string resourceName)
        {
            return $"{resourceName}-headless";
        }
    }
}
