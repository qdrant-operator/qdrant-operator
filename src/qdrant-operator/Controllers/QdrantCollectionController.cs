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
    [RbacRule<V1QdrantCollection>(Scope = EntityScope.Cluster, Verbs = RbacVerb.All)]
    [RbacRule<V1QdrantCluster>(Scope = EntityScope.Cluster, Verbs = RbacVerb.All)]
    [ResourceController(AutoRegisterFinalizers = true)]
    public class QdrantCollectionController : ResourceControllerBase<V1QdrantCollection>
    {
        private readonly IKubernetes                           k8s;
        private readonly IFinalizerManager<V1QdrantCollection> finalizerManager;
        private readonly ILogger<QdrantCollectionController>   logger;

        public QdrantCollectionController(
            IKubernetes k8s,
            IFinalizerManager<V1QdrantCollection> finalizerManager,
            ILogger<QdrantCollectionController> logger)
        {
            this.k8s              = k8s;
            this.finalizerManager = finalizerManager;
            this.logger           = logger;
        }

        public override async Task<ResourceControllerResult> ReconcileAsync(V1QdrantCollection resource)
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

            try
            {
                await CreateCollectionAsync(qdrantClient, resource);
            }
            catch (Exception e)
            {
                logger?.LogErrorEx(e);
                return ResourceControllerResult.RequeueEvent(TimeSpan.FromMinutes(1));
            }

            var patch = OperatorHelper.CreatePatch<V1QdrantCollection>();

            patch.Replace(c => c.Status, new V1QdrantCollection.V1QdrantCollectionStatus());
            patch.Replace(c => c.Status.CurrentSpec, resource.Spec);

            await k8s.CustomObjects.PatchNamespacedCustomObjectStatusAsync<V1QdrantCollection>(
                patch: OperatorHelper.ToV1Patch(patch),
                name: resource.Metadata.Name,
                namespaceParameter: resource.Metadata.NamespaceProperty);

            return ResourceControllerResult.Ok();
        }

        public override Task DeletedAsync(V1QdrantCollection entity)
        {
            return base.DeletedAsync(entity);
        }

        public async Task CreateCollectionAsync(QdrantClient qdrantClient, V1QdrantCollection resource)
        {
            var exists = false;
            CollectionInfo collectionInfo = null;
            try
            {
                collectionInfo = await qdrantClient.GetCollectionInfoAsync(resource.Metadata.Name);

                if (collectionInfo != null)
                {
                    exists = true;
                }
            }
            catch (Exception)
            {
                // doesn't exist
            }

            if (exists)
            {
                // check collectionInfo config matches config in resource.Spec

                if (resource.Status?.CurrentSpec != resource.Spec)
                {
                    var collectionParams = new CollectionParamsDiff();

                    if (resource.Spec.OnDiskPayload != resource.Status.CurrentSpec.OnDiskPayload)
                    {
                        collectionParams.OnDiskPayload = resource.Spec.OnDiskPayload.Value;
                    }

                    if (resource.Spec.ReplicationFactor != resource.Status.CurrentSpec.ReplicationFactor)
                    {
                        collectionParams.ReplicationFactor = (uint)resource.Spec.ReplicationFactor;
                    }

                    if (resource.Spec.WriteConsistencyFactor != resource.Status.CurrentSpec.WriteConsistencyFactor)
                    {
                        collectionParams.WriteConsistencyFactor = (uint)resource.Spec.WriteConsistencyFactor;
                    }

                    var optimizersConfig = resource.Spec.OptimizersConfigDiff?.ToGrpc(resource.Status.CurrentSpec.OptimizersConfigDiff);
                    var hnswConfig = resource.Spec.HnswConfig?.ToGrpc(resource.Status.CurrentSpec.HnswConfig);
                    var quantizationConfig = resource.Spec.QuantizationConfig?.ToGrpc(resource.Status.CurrentSpec.QuantizationConfig);

                    var sparseVectorsConfig = new SparseVectorConfig();

                    if (resource.Spec.SparseVectors != null)
                    {
                        foreach (var item in resource.Spec.SparseVectors)
                        {
                            sparseVectorsConfig.Map.Add(item.Key, new SparseVectorParams()
                            {
                                Index = item.Value.Index?.ToGrpc()
                            });

                        }
                    }

                    if (resource.Spec.VectorSpec?.NamedVectors != null
                        && resource.Spec.VectorSpec?.NamedVectors?.Count > 0)
                    {
                        var map = new VectorParamsDiffMap();

                        foreach (var item in resource.Spec.VectorSpec.NamedVectors)
                        {
                            Models.NamedVectorSpec other = null;
                            
                            resource.Status.CurrentSpec.VectorSpec.NamedVectors.TryGetValue(item.Key, out other);

                            var vectorParams = new VectorParamsDiff()
                            {
                                HnswConfig         = item.Value.HnswConfig?.ToGrpc(other.HnswConfig),
                                QuantizationConfig = item.Value.QuantizationConfig?.ToGrpc(other.QuantizationConfig)
                            };

                            if (item.Value.OnDisk.HasValue)
                            {
                                vectorParams.OnDisk = item.Value.OnDisk.Value;
                            }

                            map.Map.Add(item.Key, vectorParams);
                        }

                        await qdrantClient.UpdateCollectionAsync(
                            collectionName:      resource.Metadata.Name,
                            vectorsConfig:       map,
                            optimizersConfig:    optimizersConfig,
                            collectionParams:    collectionParams,
                            hnswConfig:          hnswConfig,
                            quantizationConfig:  quantizationConfig,
                            sparseVectorsConfig: sparseVectorsConfig);
                    }
                    else
                    {
                        var vectorsConfig = resource.Spec.VectorSpec?.ToGrpcDiff(resource.Status.CurrentSpec.VectorSpec);

                        await qdrantClient.UpdateCollectionAsync(
                            collectionName:      resource.Metadata.Name,
                            vectorsConfig:       vectorsConfig,
                            optimizersConfig:    optimizersConfig, 
                            collectionParams:    collectionParams,
                            hnswConfig:          hnswConfig,
                            quantizationConfig:  quantizationConfig,
                            sparseVectorsConfig: sparseVectorsConfig);
                    }
                }
            }
            else
            {
                // create collection
                var shardNumber            = (uint)resource.Spec.ShardNumber;
                var replicaFactor          = (uint)resource.Spec.ReplicationFactor;
                var writeConsistencyFactor = (uint)resource.Spec.WriteConsistencyFactor;
                var shardingMethod         = resource.Spec.ShardingMethod.ToGrpcShardingMethod();
                var onDiskPayload          = resource.Spec.OnDiskPayload.GetValueOrDefault();
                var walConfig              = resource.Spec.WalConfigDiff?.ToGrpc();

                var initFormCollection = resource.Spec?.InitFrom?.Collection;
                var collectionParams   = new CollectionParamsDiff()
                {
                    OnDiskPayload          = resource.Spec.OnDiskPayload.GetValueOrDefault(),
                    ReplicationFactor      = (uint)resource.Spec.ReplicationFactor,
                    WriteConsistencyFactor = (uint)resource.Spec.WriteConsistencyFactor
                };

                var optimizersConfig  = resource.Spec.OptimizersConfigDiff?.ToGrpc();
                var hnswConfig        = resource.Spec.HnswConfig?.ToGrpc();
                var quatizationConfig = resource.Spec.QuantizationConfig?.ToGrpc();

                var sparseVectorConfig = new SparseVectorConfig();

                if (resource.Spec.SparseVectors != null)
                {
                    foreach (var item in resource.Spec.SparseVectors)
                    {
                        sparseVectorConfig.Map.Add(item.Key, new SparseVectorParams()
                        {
                            Index = item.Value.Index?.ToGrpc()
                        });

                    }
                }

                if (resource.Spec.VectorSpec?.NamedVectors != null 
                    && resource.Spec.VectorSpec?.NamedVectors?.Count > 0)
                {
                    var map = new VectorParamsMap();
                    foreach (var item in resource.Spec.VectorSpec.NamedVectors)
                    {
                        var vectorParams = new VectorParams()
                        {
                            HnswConfig = item.Value.HnswConfig?.ToGrpc(),
                            QuantizationConfig = item.Value.QuantizationConfig?.ToGrpc()
                        };

                        if (item.Value.OnDisk.HasValue)
                        {
                            vectorParams.OnDisk = item.Value.OnDisk.Value;
                        }

                        map.Map.Add(item.Key, vectorParams);
                    }

                    await qdrantClient.CreateCollectionAsync(resource.Metadata.Name,
                        map, shardNumber, replicaFactor, writeConsistencyFactor, onDiskPayload,
                        hnswConfig, optimizersConfig, walConfig, quatizationConfig,
                        initFormCollection, shardingMethod, sparseVectorConfig);
                }
                else
                {
                    var vectorsConfig = new VectorParams();

                    if (resource.Spec.VectorSpec != null)
                    {
                        vectorsConfig.HnswConfig         = resource.Spec.VectorSpec.HnswConfig?.ToGrpc();
                        vectorsConfig.QuantizationConfig = resource.Spec.VectorSpec.QuantizationConfig?.ToGrpc();
                        vectorsConfig.Distance           = resource.Spec.VectorSpec.Distance.ToGrpcDistance();
                        vectorsConfig.Size               = (ulong)resource.Spec.VectorSpec.Size;
                    }

                    if (resource.Spec.VectorSpec?.OnDisk.HasValue == true)
                    {
                        vectorsConfig.OnDisk = resource.Spec.VectorSpec.OnDisk.Value;
                    }

                    await qdrantClient.CreateCollectionAsync(resource.Metadata.Name,
                        vectorsConfig, shardNumber, replicaFactor,
                        writeConsistencyFactor, onDiskPayload,
                        hnswConfig, optimizersConfig, walConfig,
                        quatizationConfig, initFormCollection, shardingMethod, sparseVectorConfig);
                }
            }
        }
    }
}
