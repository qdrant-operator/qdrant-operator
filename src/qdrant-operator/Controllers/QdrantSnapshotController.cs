using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

using AsyncKeyedLock;

using k8s;
using k8s.Models;

using Microsoft.Extensions.Logging;

using Neon.Common;
using Neon.Diagnostics;
using Neon.K8s;
using Neon.K8s.Core;
using Neon.Operator.Attributes;
using Neon.Operator.Controllers;
using Neon.Operator.Rbac;
using Neon.Operator.Util;

using QdrantOperator.Entities;
using QdrantOperator.Util;


namespace QdrantOperator.Controllers
{

    /// <summary>
    /// QdrantSnapshot Controller
    /// </summary>
    [RbacRule<QdrantSnapshot>(Scope = EntityScope.Cluster, Verbs = RbacVerb.All)]
    [RbacRule<V1QdrantCluster>(Scope = EntityScope.Cluster, Verbs = RbacVerb.All, SubResources = "status")]
    public class QdrantSnapshotController : ResourceControllerBase<QdrantSnapshot>
    {
        private readonly IKubernetes                        k8s;
        private readonly ILogger<QdrantSnapshotController>  logger;
        private readonly AsyncKeyedLocker<string>           lockProvider;
        private readonly IClusterHelper                     clusterHelper;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="k8s"></param>
        /// <param name="logger"></param>
        /// <param name="lockProvider"></param>
        /// <param name="clusterHelper"></param>
        public QdrantSnapshotController(
            IKubernetes k8s,
            ILogger<QdrantSnapshotController> logger,
            AsyncKeyedLocker<string> lockProvider,
            IClusterHelper clusterHelper)
        {
            this.k8s          = k8s;
            this.logger       = logger;
            this.lockProvider = lockProvider;
            this.clusterHelper = clusterHelper;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public override async Task<ResourceControllerResult> ReconcileAsync(QdrantSnapshot resource)
        {
            using var _lock = await lockProvider.LockAsync($"{nameof(V1QdrantCluster)}/{resource.Spec.Cluster}/{resource.Metadata.NamespaceProperty}");


            // get the cluster
            var cluster = await k8s.CustomObjects.GetNamespacedCustomObjectAsync<V1QdrantCluster>(
                name: resource.Spec.Cluster,
                namespaceParameter: resource.Namespace(),
                throwIfNotFound: false);

            if (cluster == null)
            {
                logger?.LogErrorEx(() => $"Cluster: {resource.Spec.Cluster} not found, requeuing.");

                return ResourceControllerResult.RequeueEvent(TimeSpan.FromMinutes(1));
            }

            await cluster.SetIsCreatingSnapshotAsync(k8s);

            var secret = await k8s.CoreV1.ReadNamespacedSecretAsync(name: resource.Spec.S3.AccessKey.Name, namespaceParameter: resource.Namespace());
            var awsAccessKeyId = Encoding.UTF8.GetString(secret.Data[resource.Spec.S3.AccessKey.Key]);

            secret = await k8s.CoreV1.ReadNamespacedSecretAsync(name: resource.Spec.S3.SecretAccessKey.Name, namespaceParameter: resource.Namespace());
            var awsSecretAccessKey = Encoding.UTF8.GetString(secret.Data[resource.Spec.S3.SecretAccessKey.Key]);

            var s3 = new AmazonS3Client(
                awsAccessKeyId:     awsAccessKeyId,
                awsSecretAccessKey: awsSecretAccessKey,
                clientConfig:       new AmazonS3Config()
                {
                    Timeout        = TimeSpan.FromHours(1),
                    RetryMode      = Amazon.Runtime.RequestRetryMode.Standard,
                    MaxErrorRetry  = 3,
                    RegionEndpoint = RegionEndpoint.GetBySystemName(resource.Spec.S3.Region)
                });

            var putRequest = new PutObjectRequest
            {
                BucketName  = resource.Spec.S3.Bucket,
                Key         = $"{resource.Uid()}/QdrantCluster.json",
                ContentBody = KubernetesHelper.JsonSerialize(cluster),
                ContentType = "application/json",
            };

            var putResponse = await s3.PutObjectAsync(putRequest);

            putResponse.EnsureSuccess();

            for (int i = 0; i < cluster.Spec.Replicas; i++)
            {
                var nodeName = $"{cluster.GetFullName()}-{i}";

                if (resource.Status.Nodes.TryGetValue(nodeName, out var _status))
                {
                    if (!_status.Checksum.IsNullOrEmpty())
                    {
                        continue;
                    }
                }

                await CreateSnapshotInternalAsync(
                    cluster:cluster,
                    resource: resource,
                    nodeName: nodeName);

            }

            for (int i = 0; i < cluster.Spec.Replicas; i++)
            {
                var nodeName = $"{cluster.GetFullName()}-{i}";

                await CreateJobInternalAsync(
                    resource:resource,
                    nodeName:nodeName,
                    awsAccessKey: resource.Spec.S3.AccessKey,
                    awsSecretAccessKey: resource.Spec.S3.SecretAccessKey);
            }

            return ResourceControllerResult.Ok();

        }

        internal async Task CreateSnapshotInternalAsync(V1QdrantCluster cluster, QdrantSnapshot resource, string nodeName)
        {
            var client = await clusterHelper.CreateQdrantClientAsync(
                    cluster: cluster,
                    namespaceParameter: resource.Metadata.NamespaceProperty,
                    nodeName: nodeName);

            var snapshot = await client.CreateSnapshotAsync(resource.Spec.Collection);

            var SnapshotPatch = OperatorHelper.CreatePatch<QdrantSnapshot>();

            if (resource.Status == null)
            {
                resource.Status = new Models.SnapshotStatus();
                SnapshotPatch.Replace(path => path.Status, resource.Status);
            }

            if (resource.Status.Nodes == null)
            {
                resource.Status.Nodes = new Dictionary<string, Models.SnapshotNodeStatus>();
            }

            var newSnapshotStatus = new Models.SnapshotNodeStatus()
            {
                Name = snapshot.Name,
                CreationTime = snapshot.CreationTime.ToDateTime(),
                Size = snapshot.Size,
                Checksum = snapshot.Checksum,

            };
            resource.Status.Nodes[nodeName] = newSnapshotStatus;
            SnapshotPatch.Replace(path => path.Status.Nodes, resource.Status.Nodes);


            resource = await k8s.CustomObjects.PatchNamespacedCustomObjectStatusAsync<QdrantSnapshot>(
                patch: OperatorHelper.ToV1Patch<QdrantSnapshot>(SnapshotPatch),
                name: resource.Name(),
                namespaceParameter: resource.Namespace());

            await cluster.SetIsCreatingSnapshotAsync(k8s,false);

        }

        internal async Task CreateJobInternalAsync(
            QdrantSnapshot resource,
            string nodeName,
            V1SecretKeySelector awsAccessKey,
            V1SecretKeySelector awsSecretAccessKey)
        {
            var snapshotName    = resource.Status.Nodes[nodeName].Name;
            var qdrantPod       = await k8s.CoreV1.ReadNamespacedPodAsync(nodeName, resource.Namespace());
            var qdrantContainer = qdrantPod.Spec.Containers
                        .Where(c => c.Name == Constants.QdrantContainerName)
                        .FirstOrDefault();

            if (qdrantContainer == null)
            {
                logger?.LogInformationEx("qdrant container is null");
                throw new Exception($"qdrant container not found in pod: {nodeName}");
            }

            var job = new V1Job().Initialize();

            job.Metadata.Name = $"upload-{nodeName}-{NeonHelper.CreateBase36Uuid()}";
            job.Metadata.NamespaceProperty = resource.Namespace();

            var volumes      = qdrantPod.Spec.Volumes.Where(v => v.Name == Constants.QdrantSnapshots).ToList();
            var volumeMounts = qdrantContainer.VolumeMounts.Where(vm => volumes.Any(v => v.Name == vm.Name)).ToList();

            job.Spec = new V1JobSpec()
            {
                Template = new V1PodTemplateSpec()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Annotations = new Dictionary<string, string>()
                        {
                            { "sidecar.istio.io/inject", "false" }
                        }
                    },
                    Spec = new V1PodSpec()
                    {
                        Volumes = volumes,
                        SecurityContext = qdrantPod.Spec.SecurityContext,
                        Tolerations = qdrantPod.Spec.Tolerations,
                        RestartPolicy = "OnFailure",
                        Containers = new List<V1Container>()
                            {
                                new V1Container()
                                {
                                    Name            = "upload",
                                    Image           = "ghcr.io/qdrant-operator/snapshot-upload:main:latest",
                                    ImagePullPolicy = "Always",
                                    Env = new List<V1EnvVar>()
                                    {
                                        new V1EnvVar()
                                        {
                                            Name = Constants.QdrantSnapshotId,
                                            Value = resource.Uid()
                                        },
                                        new V1EnvVar()
                                        {
                                            Name =  Constants.QdrantSnapshotName,
                                            Value = snapshotName
                                        },
                                        new V1EnvVar()
                                        {
                                            Name = Constants.QdrantCollectionName,
                                            Value = resource.Spec.Collection
                                        },
                                        new V1EnvVar()
                                        {
                                            Name = Constants.S3AccessKey,
                                            ValueFrom = new V1EnvVarSource()
                                            {
                                                SecretKeyRef = awsAccessKey
                                            }
                                        },
                                        new V1EnvVar()
                                        {
                                            Name = Constants.S3SecretAccessKey,
                                            ValueFrom = new V1EnvVarSource()
                                            {
                                                SecretKeyRef = awsSecretAccessKey
                                            }
                                        },
                                        new V1EnvVar()
                                        {
                                            Name = Constants.S3BucketName,
                                            Value = resource.Spec.S3.Bucket
                                        },
                                        new V1EnvVar()
                                        {
                                            Name = Constants.S3BucketRegion,
                                            Value = resource.Spec.S3.Region
                                        },
                                        new V1EnvVar()
                                        {
                                            Name = Constants.QdrantNodeId,
                                            Value = nodeName
                                        }
                                    },
                                    VolumeMounts = volumeMounts,
                                }
                            },
                        NodeSelector = new Dictionary<string, string>()
                            {
                                {
                                    "kubernetes.io/hostname", qdrantPod.Spec.NodeName
                                }
                            },
                    }
                }
            };

            await k8s.BatchV1.CreateNamespacedJobAsync(job, resource.Namespace());

            var SnapshotPatch = OperatorHelper.CreatePatch<QdrantSnapshot>();
            SnapshotPatch.Replace(path => path.Status.Nodes[nodeName].JobName, job.Name());

            await k8s.CustomObjects.PatchNamespacedCustomObjectStatusAsync<QdrantSnapshot>(
                                   patch: OperatorHelper.ToV1Patch<QdrantSnapshot>(SnapshotPatch),
                                   name: resource.Name(),
                                   namespaceParameter: resource.Namespace());

        }

    }
}
