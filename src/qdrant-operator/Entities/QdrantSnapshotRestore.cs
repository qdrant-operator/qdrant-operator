using System.Collections.Generic;
using System.ComponentModel;

using k8s;
using k8s.Models;

using QdrantOperator.Models;
namespace QdrantOperator
{
    /// <summary>
    /// Represents a Qdrant Snapshot Schedule.
    /// </summary>
    [KubernetesEntity(Group = KubeGroup, Kind = KubeKind, ApiVersion = KubeApiVersion, PluralName = KubePlural)]
    public class QdrantSnapshotRestore : IKubernetesObject<V1ObjectMeta>, ISpec<SnapshotRestoreSpec>
    {
        /// <summary>
        /// The API version this Kubernetes type belongs to.
        /// </summary>
        public const string KubeApiVersion = "v1alpha1";

        /// <summary>
        /// The Kubernetes named schema this object is based on.
        /// </summary>
        public const string KubeKind = "QdrantSnapshotRestore";

        /// <summary>
        /// The Group this Kubernetes type belongs to.
        /// </summary>
        public const string KubeGroup = Constants.KubernetesGroup;

        /// <summary>
        /// The plural name of the entity.
        /// </summary>
        public const string KubePlural = "qdrantsnapshotrestores";

        /// <summary>
        /// Constructor.
        /// </summary>
        public QdrantSnapshotRestore()
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
        /// <inheritdoc/>
        /// </summary>
        public SnapshotRestoreSpec Spec { get; set; }

    }
}
