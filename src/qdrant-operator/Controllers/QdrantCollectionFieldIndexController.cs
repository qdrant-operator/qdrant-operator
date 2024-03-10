using System;
using System.Linq;
using System.Threading.Tasks;

using k8s;

using Microsoft.Extensions.Logging;

using Neon.Common;
using Neon.Diagnostics;
using Neon.K8s;
using Neon.Net;
using Neon.K8s.PortForward;
using Neon.Operator.Attributes;
using Neon.Operator.Controllers;
using Neon.Operator.Finalizers;
using Neon.Operator.Rbac;
using Neon.Tasks;

using Qdrant.Client;

using QdrantOperator.Entities;
using QdrantOperator.Extensions;

namespace QdrantOperator
{
    /// <summary>
    /// Reconciles <see cref="V1QdrantCollectionFieldIndex"/> resources.
    /// </summary>
    [RbacRule<V1QdrantCollectionFieldIndex>(Scope = EntityScope.Cluster, Verbs = RbacVerb.All, SubResources = "status")]
    public class QdrantCollectionFieldIndexController : ResourceControllerBase<V1QdrantCollectionFieldIndex>
    {
        private readonly IKubernetes                                     k8s;
        private readonly IFinalizerManager<V1QdrantCollectionFieldIndex> finalizerManager;
        private readonly ILogger<QdrantCollectionFieldIndexController>   logger;
        private readonly ILoggerFactory                                  loggerFactory;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="k8s"></param>
        /// <param name="finalizerManager"></param>
        /// <param name="logger"></param>
        /// <param name="loggerFactory"></param>
        public QdrantCollectionFieldIndexController(
            IKubernetes                                     k8s,
            IFinalizerManager<V1QdrantCollectionFieldIndex> finalizerManager,
            ILogger<QdrantCollectionFieldIndexController>   logger,
            ILoggerFactory                                  loggerFactory)
        {
            this.k8s                = k8s;
            this.finalizerManager   = finalizerManager;
            this.logger             = logger;
            this.loggerFactory      = loggerFactory;
        }

        private async Task<QdrantClient> CreateQdrantClientAsync(V1QdrantCollectionFieldIndex resource, V1QdrantCluster cluster)
        {
            await SyncContext.Clear;

            var clusterHost = $"{cluster.GetFullName()}.{resource.Metadata.NamespaceProperty}";
            var clusterPort = 6334;

            logger?.LogInformationEx(() => $"Connecting to cluster: {resource.Spec.Cluster} at: [{clusterHost}:{clusterPort}]");

            var qdrantClient = new QdrantClient(
                host:          clusterHost,
                port:          clusterPort,
                https:         false,
                grpcTimeout:   TimeSpan.FromSeconds(60),
                loggerFactory: this.loggerFactory);

            return qdrantClient;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public override async Task<ResourceControllerResult> ReconcileAsync(V1QdrantCollectionFieldIndex resource)
        {
            await SyncContext.Clear;

            var clusters = (await k8s.CustomObjects.ListNamespacedCustomObjectAsync<V1QdrantCluster>(resource.Metadata.NamespaceProperty))
                .Items.Where(c => c.Metadata.Name == resource.Spec.Cluster);

            if (clusters.Count() != 1)
            {
                return ResourceControllerResult.RequeueEvent(TimeSpan.FromMinutes(1));
            }

            var cluster = clusters.First();

            var qdrantClient = await CreateQdrantClientAsync(resource, cluster);

            if (!(await qdrantClient.CollectionExistsAsync(resource.Spec.Collection)))
            {
                return ResourceControllerResult.RequeueEvent(TimeSpan.FromMinutes(1));
            }

            try
            {
                await UpsertCollectionFieldIndexAsync(qdrantClient, resource);
            }
            catch (Exception e)
            {
                logger?.LogErrorEx(e);
                return ResourceControllerResult.RequeueEvent(TimeSpan.FromMinutes(1));
            }

            return ResourceControllerResult.Ok();
        }

        internal async Task UpsertCollectionFieldIndexAsync(QdrantClient qdrantClient, V1QdrantCollectionFieldIndex resource)
        {
           var info = await qdrantClient.GetCollectionInfoAsync(resource.Spec.Collection);

            if (info.PayloadSchema.ContainsKey(resource.Spec.FieldName)
                && info.PayloadSchema[resource.Spec.FieldName].Params == resource.Spec.TextIndexType.ToGrpc())
            {
                return;
            }
            if (info.PayloadSchema.ContainsKey(resource.Spec.FieldName))
            {
                await qdrantClient.DeletePayloadIndexAsync(resource.Spec.Collection, resource.Spec.FieldName, wait: true);
            }

            switch (resource.Spec.Type)
            {
                case Models.PayloadSchemaType.Keyword:
                case Models.PayloadSchemaType.Text:

                    await qdrantClient.CreatePayloadIndexAsync(resource.Spec.Collection, resource.Spec.FieldName,
                                        schemaType: resource.Spec.Type.ToGrpcType(),
                                        indexParams: resource.Spec.TextIndexType.ToGrpc(),
                                        wait: true);
                    
                    break;

                default:

                    await qdrantClient.CreatePayloadIndexAsync(resource.Spec.Collection, resource.Spec.FieldName,
                        schemaType: resource.Spec.Type.ToGrpcType(),
                        wait:true);

                    break;

            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public override Task DeletedAsync(V1QdrantCollectionFieldIndex entity)
        {
            return base.DeletedAsync(entity);
        }
    }
}
