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
    /// <summary>
    /// Finalizes qdrant field index resources.
    /// </summary>
    public class QdrantCollectionFieldIndexFinalizer : ResourceFinalizerBase<V1QdrantCollectionFieldIndex>
    {
        private readonly IKubernetes k8s;
        private readonly ILogger<QdrantCollectionFieldIndexController> logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="k8s"></param>
        /// <param name="logger"></param>
        public QdrantCollectionFieldIndexFinalizer(
            IKubernetes k8s,
            ILogger<QdrantCollectionFieldIndexController> logger)
        {
            this.k8s = k8s;
            this.logger = logger;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>

        public override async Task FinalizeAsync(V1QdrantCollectionFieldIndex resource)
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

            await FinalizeCollectionFieldIndexAsync(qdrantClient, resource);
        }

        /// <summary>
        /// Finalize the index.
        /// </summary>
        /// <param name="qdrantClient"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        public async Task FinalizeCollectionFieldIndexAsync(QdrantClient qdrantClient, V1QdrantCollectionFieldIndex resource)
        {
            try
            {
                Qdrant.Client.Grpc.CollectionInfo collectionInfo = null;
                collectionInfo = await qdrantClient.GetCollectionInfoAsync(resource.Spec.Collection);
                if (collectionInfo != null)
                {
                    await qdrantClient.DeletePayloadIndexAsync(resource.Spec.Collection,resource.Spec.FieldName);

                }
            }
            catch (Exception)
            {
                // doesn't exist
            }
        }
    }
}
