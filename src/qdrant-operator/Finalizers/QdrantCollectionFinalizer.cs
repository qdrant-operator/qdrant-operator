using System;
using System.Linq;
using System.Threading.Tasks;

using k8s;
using k8s.Models;

using Microsoft.Extensions.Logging;

using Neon.Diagnostics;
using Neon.K8s;
using Neon.Operator.Finalizers;
using Neon.Tasks;

using Qdrant.Client;

namespace QdrantOperator
{
    public class QdrantCollectionFinalizer: ResourceFinalizerBase<V1QdrantCollection>
    {
        private readonly IKubernetes                         k8s;
        private readonly ILogger<QdrantCollectionController> logger;
        private readonly ILoggerFactory                      loggerFactory;
        public QdrantCollectionFinalizer(
            IKubernetes                         k8s,
            ILogger<QdrantCollectionController> logger,
            ILoggerFactory                      loggerFactory)
        {
            this.k8s           = k8s;
            this.logger        = logger;
            this.loggerFactory = loggerFactory;
        }

        public override async Task FinalizeAsync(V1QdrantCollection resource)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            logger.LogInformation($"FINALIZED: {resource.Name()}");

            var clusters = (await k8s.CustomObjects.ListNamespacedCustomObjectAsync<V1QdrantCluster>(resource.Metadata.NamespaceProperty))
                .Items.Where(c => c.Metadata.Name == resource.Spec.Cluster);

            if (clusters.Count() == 0)
            {
                return;
            }

            var cluster = clusters.First();

            var clusterHost = $"{cluster.Metadata.Name}.{resource.Metadata.NamespaceProperty}";
            var clusterPort = 6334;

            logger?.LogInformationEx(() => $"Connecting to cluster: {resource.Spec.Cluster} at: [{clusterHost}:{clusterPort}]");

            var qdrantClient = new QdrantClient(
                host:          clusterHost,
                port:          clusterPort,
                https:         false,
                grpcTimeout:   TimeSpan.FromSeconds(60),
                loggerFactory: loggerFactory);

            await FinalizeCollectionAsync(qdrantClient, resource);
        }

        public async Task FinalizeCollectionAsync(QdrantClient qdrantClient, V1QdrantCollection resource)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

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
