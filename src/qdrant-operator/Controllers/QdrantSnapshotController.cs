using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

using AsyncKeyedLock;

using k8s;
using k8s.Models;

using Microsoft.Extensions.Logging;

using Neon.Operator.Attributes;
using Neon.Operator.Controllers;
using Neon.Operator.Rbac;

using Neon.K8s;
using Neon.K8s.Core;
using Neon.Diagnostics;
using Neon.Operator.Util;
using Amazon.S3;
using Amazon;
using Amazon.S3.Model;

using QdrantOperator.Util;
using QdrantOperator.Entities;
using Neon.Common;
using Qdrant.Client.Grpc;
using System.Net.Sockets;


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
        private readonly ClusterHelper                      clusterHelper;

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
            ClusterHelper clusterHelper)
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


            cluster.Status.Conditions ??= new List<V1Condition>();

            var condition = new V1Condition()
            {
                Type = Conditions.CreatingSnapshot,
                Status = Conditions.TrueStatus,
                LastTransitionTime = DateTime.UtcNow,
            };

            cluster.Status.Conditions = cluster.Status.Conditions.Where(c => c.Type != condition.Type).ToList();
            cluster.Status.Conditions.Add(condition);

            var patch = OperatorHelper.CreatePatch<V1QdrantCluster>();

            patch.Replace(path => path.Status.Conditions, cluster.Status.Conditions);

            await k8s.CustomObjects.PatchNamespacedCustomObjectStatusAsync<V1QdrantCluster>(
                patch: OperatorHelper.ToV1Patch<V1QdrantCluster>(patch),
                name: resource.Name(),
                namespaceParameter: resource.Namespace());


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

                var client = await clusterHelper.CreateQdrantClientAsync(
                    cluster: cluster,
                    namespaceParameter: resource.Namespace(),
                    nodeName: nodeName);

                var snapshot = await client.CreateSnapshotAsync(resource.Spec.Collection);

                var SnapshotPatch = OperatorHelper.CreatePatch<QdrantSnapshot>();

                if (resource.Status.Nodes == null)
                {
                    SnapshotPatch.Replace(path => path.Status.Nodes, new Dictionary<string, Models.SnapshotNodeStatus>());
                }

                var newSnapshotStatus = new Models.SnapshotNodeStatus()
                {
                    Name = snapshot.Name,
                    CreationTime = snapshot.CreationTime.ToDateTime(),
                    Size = snapshot.Size,
                    Checksum = snapshot.Checksum,

                };

                SnapshotPatch.Replace(path => path.Status.Nodes[nodeName], newSnapshotStatus);

                resource = await k8s.CustomObjects.PatchNamespacedCustomObjectStatusAsync<QdrantSnapshot>(
                    patch: OperatorHelper.ToV1Patch<QdrantSnapshot>(SnapshotPatch),
                    name: resource.Name(),
                    namespaceParameter: resource.Namespace());
            }

            for (int i = 0; i < cluster.Spec.Replicas; i++)
            {
                var nodeName = $"{cluster.GetFullName()}-{i}";

                var snapshotName = resource.Status.Nodes[nodeName].Name;
                var qdrantPod = await k8s.CoreV1.ReadNamespacedPodAsync(nodeName, resource.Namespace());
                var qdrantContainer = qdrantPod.Spec.Containers
                        .Where(c => c.Name == Constants.QdrantContainerName)
                        .FirstOrDefault();

                if (qdrantContainer == null)
                {
                    logger?.LogInformationEx("qdrant container is null");
                    continue;
                }

                var job = new V1Job().Initialize();

                job.Metadata.Name = $"upload-{nodeName}-{NeonHelper.CreateBase36Uuid()}";
                job.Metadata.NamespaceProperty = resource.Namespace();

                var volumes      = qdrantPod.Spec.Volumes.Where(v => v.PersistentVolumeClaim != null).ToList();
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
                                            Name = "SNAPSHOT_ID",
                                            Value = resource.Uid()
                                        },
                                        new V1EnvVar()
                                        {
                                            Name = "SNAPSHOT_NAME",
                                            Value = snapshotName
                                        },
                                        new V1EnvVar()
                                        {
                                            Name = "COLLECTION_NAME",
                                            Value = resource.Spec.Collection
                                        },
                                        new V1EnvVar()
                                        {
                                            Name = "S3_ACCESS_KEY",
                                            Value = awsAccessKeyId
                                        },
                                        new V1EnvVar()
                                        {
                                            Name = "S3_SECRET_ACCESS_KEY",
                                            Value = awsSecretAccessKey
                                        },
                                        new V1EnvVar()
                                        {
                                            Name = "S3_BUCKET_NAME",
                                            Value = resource.Spec.S3.Bucket
                                        },
                                        new V1EnvVar()
                                        {
                                            Name = "S3_BUCKET_REGION",
                                            Value = resource.Spec.S3.Region
                                        },
                                        new V1EnvVar()
                                        {
                                            Name = "QDRANT_NODE_ID",
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

            return ResourceControllerResult.Ok();

        }

    }
}
