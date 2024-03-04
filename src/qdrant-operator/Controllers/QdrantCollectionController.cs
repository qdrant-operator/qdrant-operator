using System;
using System.Linq;
using System.Threading.Tasks;

using Grpc.Core;

using k8s;
using k8s.Models;

using Microsoft.Extensions.Logging;

using Neon.Common;
using Neon.Diagnostics;
using Neon.K8s;
using Neon.K8s.PortForward;
using Neon.Net;
using Neon.Operator.Attributes;
using Neon.Operator.Controllers;
using Neon.Operator.Finalizers;
using Neon.Operator.Rbac;
using Neon.Operator.Util;
using Neon.Tasks;

using Qdrant.Client;
using Qdrant.Client.Grpc;

using QdrantOperator.Entities;
using QdrantOperator.Extensions;

namespace QdrantOperator
{
    [RbacRule<V1QdrantCollection>(Scope = EntityScope.Cluster, Verbs = RbacVerb.All, SubResources = "status")]
    [RbacRule<V1QdrantCluster>(Scope = EntityScope.Cluster, Verbs = RbacVerb.All, SubResources = "status")]
    [ResourceController(AutoRegisterFinalizers = true)]
    public class QdrantCollectionController : ResourceControllerBase<V1QdrantCollection>
    {
        private readonly IKubernetes                           k8s;
        private readonly IFinalizerManager<V1QdrantCollection> finalizerManager;
        private readonly ILogger<QdrantCollectionController>   logger;
        private readonly ILoggerFactory                        loggerFactory;

        public QdrantCollectionController(
            IKubernetes                           k8s,
            IFinalizerManager<V1QdrantCollection> finalizerManager,
            ILogger<QdrantCollectionController>   logger,
            ILoggerFactory                        loggerFactory)
        {
            this.k8s                = k8s;
            this.finalizerManager   = finalizerManager;
            this.logger             = logger;
            this.loggerFactory      = loggerFactory;
        }

        private async Task<QdrantClient> CreateQdrantClientAsync(V1QdrantCollection resource, V1QdrantCluster cluster)
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

        public override async Task<ResourceControllerResult> ReconcileAsync(V1QdrantCollection resource)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            var clusters = (await k8s.CustomObjects.ListNamespacedCustomObjectAsync<V1QdrantCluster>(resource.Metadata.NamespaceProperty))
                .Items.Where(c => c.Metadata.Name == resource.Spec.Cluster);

            if (clusters.Count() != 1)
            {
                logger?.LogErrorEx(() => $"Cluster: {resource.Spec.Cluster} not found, requeuing.");

                return ResourceControllerResult.RequeueEvent(TimeSpan.FromMinutes(1));
            }

            var cluster = clusters.First();

            var qdrantClient = await CreateQdrantClientAsync(resource, cluster);

            try
            {
                await CreateCollectionAsync(qdrantClient, resource);
            }
            catch (Exception e)
            {
                logger?.LogErrorEx(e);
                return ResourceControllerResult.RequeueEvent(TimeSpan.FromMinutes(1));
            }

            resource = await k8s.CustomObjects.ReadNamespacedCustomObjectAsync<V1QdrantCollection>(resource.Name(), resource.Namespace());

            resource.Status = new V1QdrantCollection.V1QdrantCollectionStatus()
            {
                CurrentSpec = resource.Spec
            };

            var collection = await k8s.CustomObjects.ReplaceNamespacedCustomObjectStatusAsync<V1QdrantCollection>(
                body:               resource,
                group:              V1QdrantCollection.KubeGroup,
                version:            V1QdrantCollection.KubeApiVersion,
                namespaceParameter: resource.Metadata.NamespaceProperty,
                plural:             V1QdrantCollection.KubePlural,
                name:               resource.Metadata.Name);

            return ResourceControllerResult.Ok();
        }

        public override async Task DeletedAsync(V1QdrantCollection entity)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            await base.DeletedAsync(entity);
        }

        public async Task CreateCollectionAsync(QdrantClient qdrantClient, V1QdrantCollection resource)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

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
            catch (RpcException e) when (e.StatusCode == StatusCode.NotFound || e.StatusCode == StatusCode.InvalidArgument)
            {
                // doesn't exist
            }
            catch (RpcException e)
            {
                logger?.LogErrorEx(e);
                throw;
            }

            if (exists)
            {
                if (resource.Spec.VectorSpec?.NamedVectors != null)
                {
                    var currentVectors = resource.Status?.CurrentSpec?.VectorSpec?.NamedVectors?.Select(nv => nv.Name).Distinct().Order();
                    var expectedVectors = resource.Spec.VectorSpec?.NamedVectors?.Select(nv => nv.Name).Distinct().Order();

                    if (!currentVectors.SequenceEqual(expectedVectors))
                    {
                        logger?.LogInformationEx(() => "Current vectors not equal to expected vectors, recreating collection. " +
                        $"Current Vectors: {string.Join(',', currentVectors)}" +
                        $"Expected Vectors {string.Join(',', expectedVectors)}");

                        await qdrantClient.DeleteCollectionAsync(resource.Metadata.Name, TimeSpan.FromMinutes(5));

                        exists = false;
                    }
                }
            }

            if (exists)
            {
                // check collectionInfo config matches config in resource.Spec

                if (resource.Status?.CurrentSpec != resource.Spec)
                {
                    var collectionParams = new CollectionParamsDiff();

                    if (resource.Spec.OnDiskPayload.HasValue
                        && resource.Spec.OnDiskPayload != resource.Status?.CurrentSpec?.OnDiskPayload)
                    {
                        collectionParams.OnDiskPayload = resource.Spec.OnDiskPayload.Value;
                    }

                    if (resource.Spec.ReplicationFactor != resource.Status?.CurrentSpec?.ReplicationFactor)
                    {
                        collectionParams.ReplicationFactor = (uint)resource.Spec.ReplicationFactor;
                    }

                    if (resource.Spec.WriteConsistencyFactor != resource.Status?.CurrentSpec?.WriteConsistencyFactor)
                    {
                        collectionParams.WriteConsistencyFactor = (uint)resource.Spec.WriteConsistencyFactor;
                    }

                    var optimizersConfig   = resource.Spec.OptimizersConfig?.ToGrpc(resource.Status?.CurrentSpec?.OptimizersConfig);
                    var hnswConfig         = resource.Spec.HnswConfig?.ToGrpc(resource.Status?.CurrentSpec?.HnswConfig);
                    var quantizationConfig = resource.Spec.QuantizationConfig?.ToGrpc(resource.Status?.CurrentSpec?.QuantizationConfig);

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
                            var other = resource.Status?.CurrentSpec?.VectorSpec.NamedVectors.Where(nv => nv.Name == item.Name).FirstOrDefault();

                            var vectorParams = new VectorParamsDiff()
                            {
                                HnswConfig         = item.HnswConfig?.ToGrpc(other.HnswConfig),
                                QuantizationConfig = item.QuantizationConfig?.ToGrpc(other.QuantizationConfig)
                            };

                            if (item.OnDisk.HasValue)
                            {
                                vectorParams.OnDisk = item.OnDisk.Value;
                            }

                            map.Map.Add(item.Name, vectorParams);
                        }

                        await qdrantClient.UpdateCollectionAsync(
                            collectionName:      resource.Metadata.Name,
                            vectorsConfig:       map,
                            optimizersConfig:    optimizersConfig,
                            collectionParams:    collectionParams,
                            hnswConfig:          hnswConfig,
                            quantizationConfig:  quantizationConfig,
                            sparseVectorsConfig: sparseVectorsConfig,
                            timeout:             TimeSpan.FromMinutes(5));
                    }
                    else
                    {
                        var vectorsConfig = resource.Spec.VectorSpec?.ToGrpcDiff(resource.Status?.CurrentSpec?.VectorSpec);

                        await qdrantClient.UpdateCollectionAsync(
                            collectionName:      resource.Metadata.Name,
                            vectorsConfig:       vectorsConfig,
                            optimizersConfig:    optimizersConfig, 
                            collectionParams:    collectionParams,
                            hnswConfig:          hnswConfig,
                            quantizationConfig:  quantizationConfig,
                            sparseVectorsConfig: sparseVectorsConfig,
                            timeout:             TimeSpan.FromMinutes(5));
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
                var walConfig              = resource.Spec.WalConfig?.ToGrpc();

                var initFormCollection = resource.Spec?.InitFrom?.Collection;
                var collectionParams   = new CollectionParamsDiff()
                {
                    OnDiskPayload          = resource.Spec.OnDiskPayload.GetValueOrDefault(),
                    ReplicationFactor      = (uint)resource.Spec.ReplicationFactor,
                    WriteConsistencyFactor = (uint)resource.Spec.WriteConsistencyFactor
                };

                var optimizersConfig  = resource.Spec.OptimizersConfig?.ToGrpc();
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
                            HnswConfig         = item.HnswConfig?.ToGrpc(),
                            QuantizationConfig = item.QuantizationConfig?.ToGrpc(),
                            Size               = (ulong)item.Size,
                            Distance           = item.Distance.ToGrpcDistance(),
                        };

                        if (item.OnDisk.HasValue)
                        {
                            vectorParams.OnDisk = item.OnDisk.Value;
                        }

                        map.Map.Add(item.Name, vectorParams);
                    }

                    await qdrantClient.CreateCollectionAsync(
                        collectionName:         resource.Metadata.Name,
                        vectorsConfig:          map,
                        shardNumber:            shardNumber,
                        replicationFactor:      replicaFactor,
                        writeConsistencyFactor: writeConsistencyFactor,
                        onDiskPayload:          onDiskPayload,
                        hnswConfig:             hnswConfig,
                        optimizersConfig:       optimizersConfig,
                        walConfig:              walConfig,
                        quantizationConfig:     quatizationConfig,
                        initFromCollection:     initFormCollection,
                        shardingMethod:         shardingMethod,
                        sparseVectorsConfig:    sparseVectorConfig,
                        timeout:                TimeSpan.FromMinutes(5));
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

                    await qdrantClient.CreateCollectionAsync(
                        collectionName:         resource.Metadata.Name,
                        vectorsConfig:          vectorsConfig,
                        shardNumber:            shardNumber,
                        replicationFactor:      replicaFactor,
                        writeConsistencyFactor: writeConsistencyFactor,
                        onDiskPayload:          onDiskPayload,
                        hnswConfig:             hnswConfig,
                        optimizersConfig:       optimizersConfig,
                        walConfig:              walConfig,
                        quantizationConfig:     quatizationConfig,
                        initFromCollection:     initFormCollection,
                        shardingMethod:         shardingMethod,
                        sparseVectorsConfig:    sparseVectorConfig,
                        timeout:                TimeSpan.FromMinutes(5));
                }
            }
        }
    }
}
