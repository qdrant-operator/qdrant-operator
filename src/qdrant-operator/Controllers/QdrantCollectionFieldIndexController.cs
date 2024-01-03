using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using k8s;

using Microsoft.Extensions.Logging;

using Neon.Diagnostics;
using Neon.K8s;
using Neon.Operator.Attributes;
using Neon.Operator.Controllers;
using Neon.Operator.Finalizers;
using Neon.Operator.Rbac;
using Neon.Operator.Util;
using Neon.Tasks;

using Qdrant.Client;
using Qdrant.Client.Grpc;

using QdrantOperator.Extensions;

namespace QdrantOperator
{
    [RbacRule<V1QdrantCollectionFieldIndex>(Scope = EntityScope.Cluster, Verbs = RbacVerb.All, SubResources = "status")]
    public class QdrantCollectionFieldIndexController : ResourceControllerBase<V1QdrantCollectionFieldIndex>
    {
        private readonly IKubernetes k8s;
        private readonly IFinalizerManager<V1QdrantCollectionFieldIndex> finalizerManager;
        private readonly ILogger<QdrantCollectionFieldIndexController> logger;

        public QdrantCollectionFieldIndexController(
            IKubernetes k8s,
            IFinalizerManager<V1QdrantCollectionFieldIndex> finalizerManager,
            ILogger<QdrantCollectionFieldIndexController> logger)
        {
            this.k8s = k8s;
            this.finalizerManager = finalizerManager;
            this.logger = logger;
        }
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
            var qdrantClient = new QdrantClient(
                host:  $"{cluster.Metadata.Name}.{cluster.Metadata.NamespaceProperty}",
                port:  6334,
                https: false);

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
        public async Task UpsertCollectionFieldIndexAsync(QdrantClient qdrantClient, V1QdrantCollectionFieldIndex resource)
        {
           var info = await qdrantClient.GetCollectionInfoAsync(resource.Spec.Collection);

            if (info.PayloadSchema.ContainsKey(resource.Spec.FieldName)
                && info.PayloadSchema[resource.Spec.FieldName].Params == resource.Spec.TextIndexType.ToGrpc())
            {
                return;
            }
            if (info.PayloadSchema.ContainsKey(resource.Spec.FieldName))
            {
                await qdrantClient.DeletePayloadIndexAsync(resource.Spec.Collection, resource.Spec.FieldName);
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
        public override Task DeletedAsync(V1QdrantCollectionFieldIndex entity)
        {
            return base.DeletedAsync(entity);
        }
    }
}
