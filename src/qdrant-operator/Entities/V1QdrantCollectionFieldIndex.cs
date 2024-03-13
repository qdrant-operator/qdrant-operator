using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using k8s;
using k8s.Models;

using Neon.Operator.Attributes;

using QdrantOperator.Models;

namespace QdrantOperator

{
    /// <summary>
    /// Represents a Qdrant Collection Field Index.
    /// </summary>
    [KubernetesEntity(Group = KubeGroup, Kind = KubeKind, ApiVersion = KubeApiVersion, PluralName = KubePlural)]
    public class V1QdrantCollectionFieldIndex : IKubernetesObject<V1ObjectMeta>, ISpec<V1QdrantCollectionFieldIndex.V1QdrantCollectionFieldIndexSpec>
    {
        /// <summary>
        /// The API version this Kubernetes type belongs to.
        /// </summary>
        public const string KubeApiVersion = "v1alpha1";

        /// <summary>
        /// The Kubernetes named schema this object is based on.
        /// </summary>
        public const string KubeKind = "QdrantCollectionFieldIndex";

        /// <summary>
        /// The Group this Kubernetes type belongs to.
        /// </summary>
        public const string KubeGroup = "qdrant.io";

        /// <summary>
        /// The plural name of the entity.
        /// </summary>
        public const string KubePlural = "qdrantcollectionfieldindexes";

        /// <summary>
        /// Constructor.
        /// </summary>
        public V1QdrantCollectionFieldIndex()
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
        /// The spec for the field index.
        /// </summary>
        public V1QdrantCollectionFieldIndexSpec Spec { get; set; }

        /// <summary>
        /// The spec.
        /// </summary>
        public class V1QdrantCollectionFieldIndexSpec
        {
            /// <summary>
            /// The cluster that the index applies to.
            /// </summary>
            public string Cluster { get; set; }

            /// <summary>
            /// The collection that the index applies to.
            /// </summary>
            public string Collection { get; set; }

            /// <summary>
            /// The field name.
            /// </summary>
            [Required]
            public string FieldName { get; set; }

            /// <summary>
            /// The payload schema type.
            /// </summary>
            [Required]
            [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumMemberConverter))]
            public PayloadSchemaType Type { get; set; }

            /// <summary>
            /// The text index type.
            /// </summary>
            public TextIndexType TextIndexType { get; set; }


        }
    }
}
