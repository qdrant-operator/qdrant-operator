﻿using k8s.Models;
using k8s;
using Neon.Operator.Attributes;
using QdrantOperator.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using static QdrantOperator.V1QdrantCollection;


namespace QdrantOperator.Entities
{
    [KubernetesEntity(Group = KubeGroup, Kind = KubeKind, ApiVersion = KubeApiVersion, PluralName = KubePlural)]
    public class V1QdrantCollection : IKubernetesObject<V1ObjectMeta>, ISpec<V1QdrantCollection.V1QdrantCollectionSpec>
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

        /// <inheritdoc/>
        public string ApiVersion { get; set; }
        /// <inheritdoc/>
        public string Kind { get; set; }
        /// <inheritdoc/>
        public V1ObjectMeta Metadata { get; set; }
        /// <inheritdoc/>
        public V1QdrantCollectionSpec Spec { get; set; }
        public class V1QdrantCollectionSpec
        {
            public string Cluster { get; set; }
            public VectorSpec VectorSpec { get; set; }

            [DefaultValue(null)]
            [Range(Minimum = 4, ExclusiveMinimum = false)]
            public int? ShardNumber { get; set; } = null;

            [DefaultValue(null)]
            public ShardingMethod? ShardingMethod { get; set; } = null;

            [DefaultValue(null)]
            [Range(Minimum = 1, ExclusiveMinimum = false)]
            public int? ReplicationFactor { get; set; } = null;

            [DefaultValue(null)]
            [Range(Minimum = 1, ExclusiveMinimum = false)]
            public int? WriteConsistencyFactor { get; set; } = null;

            [DefaultValue(null)]
            public bool? OnDiskPayload { get; set; } = null;
            public HnswConfig HnswConfig { get; set; }
            public WalConfigDiff WalConfigDiff { get; set; }
            public OptimizersConfigDiff OptimizersConfigDiff { get; set; }
            public InitFrom InitFrom { get; set; }
            public QuantizationConfig QuantizationConfig { get; set; }
            public Dictionary<string, SparseVectorParams> SparseVectors { get; set; }

        }
    }
}