using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using k8s;
using k8s.KubeConfigModels;
using k8s.Models;

using Neon.Operator.Util;
using Neon.K8s;
using OpenTelemetry.Resources;

using QdrantOperator.Models;
using Neon.Common;

namespace QdrantOperator
{
    /// <summary>
    /// Represents a Qdrant cluster.
    /// </summary>
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
        public const string KubeGroup = Constants.KubernetesGroup;

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
        public V1QdrantClusterSpec Spec { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public V1QdrantClusterStatus Status { get; set; }

        /// <summary>
        /// The Qdrant cluster spec.
        /// </summary>
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
            /// The number of servers to deploy in the cluster.
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
            [DefaultValue(null)]
            public Dictionary<string, string> NodeSelector { get; set; }

            /// <summary>
            /// Whether to add an antiAffinity rule to the statefulset. Enabling this will ensure
            /// that only 1 qdrant container will be deployed per kubernetes host.
            /// </summary>
            [DefaultValue(true)]
            public bool AntiAffinity { get; set; } = true;

            /// <summary>
            /// The resource requirements for each qdrant server.
            /// </summary>
            [DefaultValue(null)]
            public V1ResourceRequirements Resources { get; set; }

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
            [DefaultValue(null)]
            public Dictionary<string, string> PodAnnotations { get; set; }

            /// <summary>
            /// Optionally set some extra pod labels.
            /// </summary>
            [DefaultValue(null)]
            public Dictionary<string, string> PodLabels { get; set; }

            /// <summary>
            /// Optionally add pod tolerations.
            /// </summary>
            [DefaultValue(null)]
            public List<V1Toleration> Tolerations { get; set; }

            /// <summary>
            /// Optionally add topology spread constraints.
            /// </summary>
            [DefaultValue(null)]
            public List<V1TopologySpreadConstraint> TopologySpreadConstraints { get; set; }

            /// <summary>
            /// Optionally set an API Key. If the secret specified does not exist, it will be created.
            /// </summary>
            public ApiKeyOptions ApiKey { get; set; } = new ApiKeyOptions();

            /// <summary>
            /// Optionally set an API Key for read operations. If the secret specified does not exist, it will be created.
            /// </summary>
            public ApiKeyOptions ReadApiKey { get; set; } = new ApiKeyOptions();
        }

        /// <summary>
        /// The status of the cluster.
        /// </summary>
        public class V1QdrantClusterStatus
        {
            /// <summary>
            /// The status message.
            /// </summary>
            public string Message { get; set; }
            /// <summary>
            /// Conditions of status
            /// </summary>
            public List<V1Condition> Conditions { get; set; }

            /// <summary>
            /// snapshot creation status
            /// </summary>
            /// <returns></returns>
            public bool IsCreatingSnapshot()
            {
                return Conditions.Any(c => c.Type == QdrantOperator.Conditions.CreatingSnapshot
                                            && c.Status == QdrantOperator.Conditions.TrueStatus);
            }
            
        }

        /// <summary>
        /// Adding a condition to the list of conditions by patching it.
        /// If the condition already exists, it will be updated.
        /// </summary>
        /// <param name="k8s"></param>
        /// <param name="type"></param>
        /// <param name="status"></param>
        /// <param name="message"></param>
        /// <param name="reason"></param>
        /// <returns></returns>

        public async Task SetConditionAsync(IKubernetes k8s, string type, string status, string message = null, string reason = null)
        {
            var patch = OperatorHelper.CreatePatch<V1QdrantCluster>();

            if (Status == null)
            {
                Status = new V1QdrantCluster.V1QdrantClusterStatus();
                patch.Replace(path => path.Status, Status);
            }

            var condition = new V1Condition()
            {
                Type = type,
                Status = status,
                LastTransitionTime = DateTime.UtcNow,
            };

            if (!message.IsNullOrEmpty())
            {
                condition.Message = message;
            }

            if (!reason.IsNullOrEmpty())
            {
                condition.Reason = reason;
            }

            Status.Conditions ??= new List<V1Condition>();

            Status.Conditions = Status.Conditions.Where(c => c.Type != condition.Type).ToList();

            Status.Conditions.Add(condition);

            patch.Replace(path => path.Status.Conditions, Status.Conditions);

            await k8s.CustomObjects.PatchNamespacedCustomObjectStatusAsync<V1QdrantCluster>(
                patch: OperatorHelper.ToV1Patch<V1QdrantCluster>(patch),
                name: Metadata.Name,
                namespaceParameter: Metadata.NamespaceProperty);
        }

        /// <summary>
        /// set the snapshot creation status. The default value is true.
        /// </summary>
        /// <param name="k8s"></param>
        /// <param name="value"></param>
        /// <returns></returns>

        public Task SetIsCreatingSnapshotAsync(IKubernetes k8s, bool value = true)
        {
            return SetConditionAsync(k8s, Conditions.CreatingSnapshot, value ? Conditions.TrueStatus : Conditions.FalseStatus);
        }
    }
}
