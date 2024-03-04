using System.Collections.Generic;
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
            /// <summary>
            /// Options for defining the container image.
            /// </summary>
            public ImageSpec Image { get; set; } = new ImageSpec();

            /// <summary>
            /// Persistence options.
            /// </summary>
            public PersistenceSpec Persistence { get; set; } = new PersistenceSpec();

            /// <summary>
            /// Thhe number of servers to deploy in the cluster.
            /// </summary>
            [DefaultValue(1)]
            public int Replicas { get; set; } = 1;

            /// <summary>
            /// Metrics related options.
            /// </summary>
            public MetricsOptions Metrics { get; set; } = new MetricsOptions();

            /// <summary>
            /// Where to put the servers.
            /// </summary>
            public Dictionary<string, string> NodeSelector { get; set; } = new Dictionary<string, string>();

            /// <summary>
            /// Whether to add an antiAffinity rule to the statefulset. Enabling this will ensure
            /// that only 1 qdrant container will be deployed per kubernetes host.
            /// </summary>
            [DefaultValue(true)]
            public bool AntiAffinity { get; set; } = true;

            /// <summary>
            /// The resource requirements for each qdrant server.
            /// </summary>
            public V1ResourceRequirements Resources { get; set; } = new V1ResourceRequirements();

            /// <summary>
            /// The pod security context options.
            /// </summary>
            public V1PodSecurityContext PodSecurityContext { get; set; }

            /// <summary>
            /// The container security context options.
            /// </summary>
            public V1SecurityContext SecurityContext { get; set; }

            /// <summary>
            /// Optionally set some extra pod annotations.
            /// </summary>
            public Dictionary<string, string> PodAnnotations { get; set; } = new Dictionary<string, string>();

            /// <summary>
            /// Optionally set some extra pod labels.
            /// </summary>
            public Dictionary<string, string> PodLabels { get; set; } = new Dictionary<string, string>();
        }

        public class V1QdrantClusterStatus
        {
            public string Message { get; set; }
            
        }
    }
}
