using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using k8s;
using k8s.Models;

using Neon.Operator.Attributes;

using QdrantOperator.Models;

namespace QdrantOperator

{
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

        /// <inheritdoc/>
        public string ApiVersion { get; set; }
        /// <inheritdoc/>
        public string Kind { get; set; }
        /// <inheritdoc/>
        public V1ObjectMeta Metadata { get; set; }
        public V1QdrantCollectionFieldIndexSpec Spec { get; set; }

        public class V1QdrantCollectionFieldIndexSpec
        {
            public string Cluster { get; set; }
            public string Collection { get; set; }


            [Required]
            public string FieldName { get; set; }

            [Required]
            public PayloadSchemaType Type { get; set; }

            public TextIndexType TextIndexType { get; set; }


        }
    }
}
