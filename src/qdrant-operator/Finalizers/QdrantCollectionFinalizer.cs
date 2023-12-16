using System;
using System.Linq;
using System.Threading.Tasks;

using k8s;
using k8s.Models;

using Microsoft.Extensions.Logging;

using Neon.K8s;
using Neon.Operator.Finalizers;
using Neon.Tasks;

using Qdrant.Client;

namespace QdrantOperator
{
    public class QdrantCollectionFinalizer: ResourceFinalizerBase<V1QdrantCollection>
    {
        private readonly IKubernetes k8s;
        private readonly ILogger<QdrantCollectionController> logger;
        public QdrantCollectionFinalizer(
            IKubernetes k8s,
            ILogger<QdrantCollectionController> logger)
        {
            this.k8s = k8s;
            this.logger = logger;
        }

        public override async Task FinalizeAsync(V1QdrantCollection resource)
        {
            await SyncContext.Clear;

            logger.LogInformation($"FINALIZED: {resource.Name()}");

            var clusters = (await k8s.CustomObjects.ListNamespacedCustomObjectAsync<V1QdrantCluster>(resource.Metadata.NamespaceProperty))
                .Items.Where(c => c.Metadata.Name == resource.Spec.Cluster);

            if (clusters.Count() == 0)
            {
                return;
            }

            var cluster = clusters.First();
            var qdrantClient = new QdrantClient(
                host: $"{cluster.Metadata.Name}.{cluster.Metadata.NamespaceProperty}",
                port: 6334,
            https: false);

            await FinalizeCollectionAsync(qdrantClient, resource);
        }

        public async Task FinalizeCollectionAsync(QdrantClient qdrantClient, V1QdrantCollection resource)
        {
            try
            {
                Qdrant.Client.Grpc.CollectionInfo collectionInfo = null;
                collectionInfo = await qdrantClient.GetCollectionInfoAsync(resource.Metadata.Name);
                if (collectionInfo != null)
                {
                    await qdrantClient.DeleteCollectionAsync(resource.Metadata.Name);

                }
            }
            catch (Exception)
            {
                // doesn't exist
            }
        }
    }
}
