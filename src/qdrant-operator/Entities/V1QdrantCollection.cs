using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

using k8s;
using k8s.Models;

using Neon.Operator.Attributes;

using QdrantOperator.Models;
using QdrantOperator.Models.Converters;

namespace QdrantOperator
{
    /// <summary>
    /// Represents a Qdrant Collection.
    /// </summary>
    [KubernetesEntity(Group = KubeGroup, Kind = KubeKind, ApiVersion = KubeApiVersion, PluralName = KubePlural)]
    public class V1QdrantCollection : IKubernetesObject<V1ObjectMeta>, ISpec<V1QdrantCollection.V1QdrantCollectionSpec>, IStatus<V1QdrantCollection.V1QdrantCollectionStatus>
    {
        /// <summary>
        /// The API version this Kubernetes type belongs to.
        /// </summary>
        public const string KubeApiVersion = "v1alpha1";

        /// <summary>
        /// The Kubernetes named schema this object is based on.
        /// </summary>
        public const string KubeKind = "QdrantCollection";

        /// <summary>
        /// The Group this Kubernetes type belongs to.
        /// </summary>
        public const string KubeGroup = "qdrant.io";

        /// <summary>
        /// The plural name of the entity.
        /// </summary>
        public const string KubePlural = "qdrantcollections";

        /// <summary>
        /// Constructor.
        /// </summary>
        public V1QdrantCollection()
        {
            ApiVersion = $"{KubeGroup}/{KubeApiVersion}";
            Kind = KubeKind;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string ApiVersion { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string Kind { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public V1ObjectMeta Metadata { get; set; }

        /// <summary>
        /// The collection spec.
        /// </summary>
        public V1QdrantCollectionSpec Spec { get; set; }

        /// <summary>
        /// The status of the collection.
        /// </summary>
        public V1QdrantCollectionStatus Status { get; set; }

        /// <summary>
        /// The collection spec.
        /// </summary>
        public class V1QdrantCollectionSpec
        {
            /// <summary>
            /// The cluster that the collection should be created in.
            /// </summary>
            public string Cluster { get; set; }

            /// <summary>
            /// The vector spec.
            /// </summary>

            [JsonConverter(typeof(VectorSpecConverter))]
            public VectorSpec VectorSpec { get; set; }

            /// <summary>
            /// <para>
            /// For auto sharding: Number of shards in collection. - Default is 1 for standalone,
            /// otherwise equal to the number of nodes - Minimum is 1 For custom sharding: Number of
            /// shards in collection per shard group. - Default is 1, meaning that each shard key will
            /// be mapped to a single shard - Minimum is 1
            /// </para>
            /// </summary>
            [DefaultValue(4)]
            [Range(Minimum = 4, ExclusiveMinimum = false)]
            public int ShardNumber { get; set; } = 4;

            /// <summary>
            /// Sharding method Default is Auto - points are distributed across all available shards Custom -
            /// points are distributed across shards according to shard key
            /// </summary>
            [DefaultValue(null)]
            [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumMemberConverter))]
            public ShardingMethod? ShardingMethod { get; set; } = null;

            /// <summary>
            /// Number of shards replicas. Default is 1 Minimum is 1
            /// </summary>
            [DefaultValue(1)]
            [Range(Minimum = 1, ExclusiveMinimum = false)]
            public int ReplicationFactor { get; set; } = 1;

            /// <summary>
            /// <para>
            /// Defines how many replicas should apply the operation for us to consider it successful. Increasing
            /// this number will make the collection more resilient to inconsistencies, but will also make it fail
            /// if not enough replicas are available. Does not have any performance impact.
            /// </para>
            /// </summary>
            [DefaultValue(1)]
            [Range(Minimum = 1, ExclusiveMinimum = false)]
            public int WriteConsistencyFactor { get; set; } = 1;

            /// <summary>
            /// <para>
            /// If true - point's payload will not be stored in memory. It will be read from the disk every time it
            /// is requested. This setting saves RAM by (slightly) increasing the response time. Note: those payload
            /// values that are involved in filtering and are indexed - remain in RAM.
            /// </para>
            /// </summary>
            [DefaultValue(null)]
            public bool? OnDiskPayload { get; set; } = null;

            /// <summary>
            /// Custom params for HNSW index. If none - values from service configuration file are used.
            /// </summary>
            public HnswConfig HnswConfig { get; set; }

            /// <summary>
            /// Custom params for WAL. If none - values from service configuration file are used.
            /// </summary>
            public WalConfigDiff WalConfig { get; set; }

            /// <summary>
            /// Custom params for Optimizers. If none - values from service configuration file are used.
            /// </summary>
            public OptimizersConfigDiff OptimizersConfig { get; set; }

            /// <summary>
            /// Specify other collection to copy data from.
            /// </summary>
            public InitFrom InitFrom { get; set; }

            /// <summary>
            /// Quantization parameters. If none - quantization is disabled.
            /// </summary>
            public QuantizationConfig QuantizationConfig { get; set; }

            /// <summary>
            /// Sparse vector data config.
            /// </summary>
            public Dictionary<string, SparseVectorParams> SparseVectors { get; set; }
        }

        /// <summary>
        /// The status of the collection.
        /// </summary>
        public class V1QdrantCollectionStatus
        {
            /// <summary>
            /// The currently applied spec.
            /// </summary>
            public V1QdrantCollectionSpec CurrentSpec { get; set; }
        }
    }
}
