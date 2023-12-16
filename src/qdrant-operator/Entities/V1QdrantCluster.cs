using System.ComponentModel;

using k8s;
using k8s.Models;

using QdrantOperator.Models;

namespace QdrantOperator
{
    [KubernetesEntity(Group = KubeGroup, Kind = KubeKind, ApiVersion = KubeApiVersion, PluralName = KubePlural)]
    public class V1QdrantCluster : IKubernetesObject<V1ObjectMeta>, ISpec<V1QdrantCluster.V1QdrantClusterSpec>, IStatus<V1QdrantCluster.V1QdrantClusterStatus>
    {
        /// <summary>
        /// The API version this Kubernetes type belongs to.
        /// </summary>
        public const string KubeApiVersion = "v1alpha1";

        /// <summary>
        /// The Kubernetes named schema this object is based on.
        /// </summary>
        public const string KubeKind = "QdrantCluster";

        /// <summary>
        /// The Group this Kubernetes type belongs to.
        /// </summary>
        public const string KubeGroup = "qdrant.io";

        /// <summary>
        /// The plural name of the entity.
        /// </summary>
        public const string KubePlural = "qdrantclusters";

        /// <summary>
        /// Constructor.
        /// </summary>
        public V1QdrantCluster()
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
        public V1QdrantClusterSpec Spec { get; set; }
        /// <inheritdoc/>
        public V1QdrantClusterStatus Status { get; set; }

        public class V1QdrantClusterSpec
        {
            public ImageSpec Image { get; set; }

            public PersistenceSpec Persistence { get; set; }
            
            [DefaultValue(1)]
            public int Replicas { get; set; } = 1;

            [DefaultValue(false)]
            public bool ServiceMonitors { get; set; } = false;
        }

        public class V1QdrantClusterStatus
        {
            public string Message { get; set; }
            
        }
    }
}
