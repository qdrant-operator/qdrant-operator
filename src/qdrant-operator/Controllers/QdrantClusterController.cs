using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using k8s;
using k8s.Autorest;
using k8s.Models;

using Microsoft.Extensions.Logging;

using Neon.Common;
using Neon.Diagnostics;
using Neon.K8s;
using Neon.K8s.Resources.Grafana;
using Neon.K8s.Resources.Prometheus;
using Neon.Operator;
using Neon.Operator.Attributes;
using Neon.Operator.Controllers;
using Neon.Operator.Finalizers;
using Neon.Operator.Rbac;
using Neon.Tasks;

using QdrantOperator.Entities;
using QdrantOperator.Util;

namespace QdrantOperator
{
    /// <summary>
    /// Reconciles <see cref="V1QdrantCluster"/> resources.
    /// </summary>
    [RbacRule<V1StatefulSet>(Scope = EntityScope.Cluster, Verbs = RbacVerb.All)]
    [RbacRule<V1ConfigMap>(Scope = EntityScope.Cluster, Verbs = RbacVerb.All)]
    [RbacRule<V1Secret>(Scope = EntityScope.Cluster, Verbs = RbacVerb.All)]
    [RbacRule<V1QdrantCluster>(Scope = EntityScope.Cluster, Verbs = RbacVerb.All, SubResources = "status")]
    [RbacRule<V1Service>(Scope = EntityScope.Cluster, Verbs = RbacVerb.All)]
    [RbacRule<V1ServiceAccount>(Scope = EntityScope.Cluster, Verbs = RbacVerb.All)]
    [RbacRule<V1PersistentVolumeClaim>(Scope = EntityScope.Cluster, Verbs = RbacVerb.All, SubResources = "status")]
    [RbacRule<V1ServiceMonitor>(Scope = EntityScope.Cluster, Verbs = RbacVerb.Get | RbacVerb.List | RbacVerb.Create | RbacVerb.Update)]
    [RbacRule<V1GrafanaDashboard>(Scope = EntityScope.Cluster, Verbs = RbacVerb.Get | RbacVerb.List | RbacVerb.Create | RbacVerb.Update)]
    [DependentResource<V1StatefulSet>]
    [DependentResource<V1ConfigMap>]
    [DependentResource<V1Secret>]
    [DependentResource<V1Service>]
    [DependentResource<V1ServiceAccount>]
    [ResourceController(AutoRegisterFinalizers = true)]
    public class QdrantClusterController : ResourceControllerBase<V1QdrantCluster>
    {
        private readonly IKubernetes                        k8s;
        private readonly IFinalizerManager<V1QdrantCluster> finalizerManager;
        private readonly ILogger<QdrantClusterController>   logger;
        private Dictionary<string, string>                  labels;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="k8s"></param>
        /// <param name="finalizerManager"></param>
        /// <param name="logger"></param>
        public QdrantClusterController(
            IKubernetes                        k8s,
            IFinalizerManager<V1QdrantCluster> finalizerManager,
            ILogger<QdrantClusterController>   logger)
        {
            this.k8s              = k8s;
            this.finalizerManager = finalizerManager;
            this.logger           = logger;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public override async Task<ResourceControllerResult> ReconcileAsync(V1QdrantCluster resource)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();
            
            await ReconcileInternalAsync(resource);

            await WaitForReplicasAsync(resource);

            await CheckVolumesAsync(resource);

            logger.LogInformationEx(() => $"RECONCILED: {resource.Name()}");

            return ResourceControllerResult.Ok();
        }

        internal async Task ReconcileInternalAsync(V1QdrantCluster resource)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();
            
            logger.LogInformation($"RECONCILING: {resource.Name()}");

            labels = new Dictionary<string, string>
            {
                { Labels.App,               resource.GetFullName() },
                { Labels.Instance,          resource.Metadata.Name },
                { Labels.Name,              resource.Metadata.Name },
                { Labels.Version,           resource.Spec.Image.Tag },
                { Constants.ManagedByLabel, Constants.ManagedBy }
            };

            await ConfigureSecretsAsync(resource);

            var tasks = new List<Task>()
            {
                UpsertStatefulsetAsync(resource),
                UpsertServiceAsync(resource),
                UpsertHeadlessServiceAsync(resource),
                UpsertConfigMapAsync(resource),
                UpsertServiceAccountAsync(resource)
            };

            if (resource.Spec.Metrics.ServiceMonitorEnabled)
            {
                tasks.Add(UpsertServiceMonitorAsync(resource));
            }

            if (resource.Spec.Metrics.Grafana.DashboardEnabled)
            {
                tasks.Add(UpsertGrafanaDashboardAsync(resource));
            }

            await NeonHelper.WaitAllAsync(tasks);
        }

        internal async Task CheckVolumesAsync(V1QdrantCluster resource)
        {
            for (int i = 0; i < resource.Spec.Replicas; i++)
            {
                var volumeName = $"{Constants.QdrantStorage}-{resource.GetFullName()}-{i}";
                var pvc = await k8s.CoreV1.ReadNamespacedPersistentVolumeClaimAsync(volumeName,resource.Namespace());

                if (pvc.Spec.Resources.Requests["storage"].Value == resource.Spec.Persistence.Size)
                {
                    continue;
                }

                pvc.Spec.Resources = new V1ResourceRequirements()
                {
                    Requests = new Dictionary<string, ResourceQuantity>()
                    {
                        { "storage", new ResourceQuantity(resource.Spec.Persistence.Size) }
                    }
                };

                await k8s.CoreV1.ReplaceNamespacedPersistentVolumeClaimAsync(
                    body:               pvc,
                    name:               volumeName,
                    namespaceParameter: resource.Namespace());
            }
        }
        internal async Task UpsertStatefulsetAsync(V1QdrantCluster resource)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            var statefulsetList = await k8s.AppsV1.ListNamespacedStatefulSetAsync(
                namespaceParameter: resource.Namespace(),
                fieldSelector:      $"metadata.name={resource.GetFullName()}");

            V1StatefulSet statefulSet;
            bool exists = false;
            if (statefulsetList.Items.Count > 0)
            {
                logger.LogInformationEx(() => $"StatefulSet for {resource.GetFullName()}/Qdrant exists, updating existing StatefulSet.");
                exists      = true;
                statefulSet = statefulsetList.Items[0];
            }
            else
            {
                statefulSet = new V1StatefulSet().Initialize();
                statefulSet.Metadata.Name = resource.GetFullName();
                statefulSet.Metadata.SetNamespace(resource.Namespace());
                statefulSet.Metadata.SetLabels(labels);
                statefulSet.AddOwnerReference(resource.MakeOwnerReference());
            }

            var runAsUser  = resource.Spec.SecurityContext?.RunAsUser ?? Constants.RunAsUser;
            var runAsGroup = resource.Spec.SecurityContext?.RunAsGroup ?? Constants.RunAsGroup;
            var fsGroup    = resource.Spec.PodSecurityContext?.FsGroup ?? Constants.FsGroup;

            var env = new List<V1EnvVar>()
            {
                new V1EnvVar()
                {
                    Name  = "QDRANT_INIT_FILE_PATH",
                    Value = "/qdrant/init/.qdrant-initialized",
                }
            };

            if (resource.Spec.ApiKey.Enabled)
            {
                var secretRef = resource.Spec.ApiKey.Secret ?? new V1SecretKeySelector()
                {
                    Name = resource.Spec.ApiKey.Secret?.Name ?? resource.GetFullName(),
                    Key  = resource.Spec.ApiKey.Secret?.Key ?? Constants.ApiKeySecretKey,
                };

                env.Add(new V1EnvVar(name: Constants.ApiKeyEnvName, valueFrom: new V1EnvVarSource(secretKeyRef: secretRef)));
            }

            if (resource.Spec.ReadApiKey.Enabled)
            {
                var secretRef = resource.Spec.ReadApiKey.Secret ?? new V1SecretKeySelector()
                {
                    Name = resource.Spec.ReadApiKey.Secret?.Name ?? resource.GetFullName(),
                    Key  = resource.Spec.ReadApiKey.Secret?.Key ?? Constants.ReadApiKeySecretKey,
                };

                env.Add(new V1EnvVar(name: Constants.ReadApiKeyEnvName, valueFrom: new V1EnvVarSource(secretKeyRef: secretRef)));
            }

            var spec = new V1StatefulSetSpec()
            {
                Replicas = resource.Spec.Replicas,
                Selector = new V1LabelSelector()
                {
                    MatchLabels = new Dictionary<string, string>()
                },
                Template = new V1PodTemplateSpec()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Labels = new Dictionary<string, string>()
                    },
                    Spec = new V1PodSpec()
                    {
                        Volumes = new List<V1Volume>()
                        {
                            new V1Volume()
                            {
                                Name = Constants.QdrantConfig,
                                ConfigMap = new V1ConfigMapVolumeSource()
                                {
                                    Name        = resource.GetFullName(),
                                    DefaultMode = 493
                                }

                            },
                            new V1Volume()
                            {
                                Name     = Constants.QdrantSnapshots,
                                EmptyDir = new V1EmptyDirVolumeSource() { }
                            },
                            new V1Volume()
                            {
                                Name     = Constants.QdrantInit,
                                EmptyDir = new V1EmptyDirVolumeSource() { }
                            }


                        },
                        InitContainers = new List<V1Container>()
                        {

                            new V1Container()
                            {
                                Name    = Constants.OwnershipInitContainerName,
                                Image   = $"{resource.Spec.Image.Repository}:{resource.Spec.Image.Tag}",
                                Command = new List<string>()
                                {
                                    "chown",
                                    "-R",
                                    $"{runAsUser}:{fsGroup}",
                                    "/qdrant/storage"
                                },
                                Resources    = new V1ResourceRequirements() { },
                                VolumeMounts = new List<V1VolumeMount>()
                                {
                                    new V1VolumeMount()
                                    {
                                        Name = Constants.QdrantStorage,
                                        MountPath = Constants.QdrantStoragePath,
                                    }
                                },
                                TerminationMessagePath   = "/dev/termination-log",
                                TerminationMessagePolicy = "File",
                                ImagePullPolicy          = resource.Spec.Image.PullPolicy,
                            }
                        },
                        Containers = new List<V1Container>()
                        {
                            new V1Container()
                            {
                                Name    = Constants.QdrantContainerName,
                                Image   = $"{resource.Spec.Image.Repository}:{resource.Spec.Image.Tag}",
                                Command = new List<string>()
                                {
                                    "/bin/bash",
                                    "-c"
                                },
                                Args = new List<string>()
                                {
                                    "./config/initialize.sh",
                                },
                                Ports = new List<V1ContainerPort>()
                                {
                                    new V1ContainerPort()
                                    {
                                        Name          = Constants.HttpPortName,
                                        ContainerPort = Constants.HttpPort,
                                        Protocol      = "TCP",
                                    },
                                    new V1ContainerPort()
                                    {
                                        Name          = Constants.GrpcPortName,
                                        ContainerPort = Constants.GrpcPort,
                                        Protocol      = "TCP"
                                    },
                                    new V1ContainerPort()
                                    {
                                        Name          = Constants.P2pPortName,
                                        ContainerPort = Constants.P2pPort,
                                        Protocol      = "TCP"
                                    },
                                },
                                Env = env,
                                Resources = resource.Spec.Resources,
                                VolumeMounts = new List<V1VolumeMount>()
                                {
                                    new V1VolumeMount()
                                    {
                                        Name      = Constants.QdrantStorage,
                                        MountPath = Constants.QdrantStoragePath,
                                    },
                                    new V1VolumeMount()
                                    {
                                        Name      = Constants.QdrantConfig,
                                        MountPath = "/qdrant/config/initialize.sh",
                                        SubPath   = "initialize.sh"
                                    },
                                    new V1VolumeMount()
                                    {
                                        Name      = Constants.QdrantConfig,
                                        MountPath = "/qdrant/config/production.yaml",
                                        SubPath   = "production.yaml"
                                    },
                                    new V1VolumeMount()
                                    {
                                        Name      = Constants.QdrantSnapshots,
                                        MountPath = "/qdrant/snapshots",
                                    },
                                    new V1VolumeMount()
                                    {
                                        Name      = Constants.QdrantInit,
                                        MountPath = "/qdrant/init"
                                    }
                                },
                                ReadinessProbe = new V1Probe()
                                {
                                    HttpGet = new V1HTTPGetAction()
                                    {
                                        Path   = "/readyz",
                                        Port   = Constants.HttpPort,
                                        Scheme = "HTTP"
                                    },
                                    InitialDelaySeconds = 5,
                                    TimeoutSeconds      = 1,
                                    PeriodSeconds       = 5,
                                    SuccessThreshold    = 1,
                                    FailureThreshold    = 6,
                                },
                                TerminationMessagePath   = "/dev/termination-log",
                                TerminationMessagePolicy = "File",
                                ImagePullPolicy          = resource.Spec.Image.PullPolicy,
                                SecurityContext          = resource.Spec.SecurityContext ??
                                new V1SecurityContext()
                                {
                                    Privileged               = false,
                                    RunAsUser                = runAsUser,
                                    RunAsGroup               = runAsGroup,
                                    RunAsNonRoot             = true,
                                    ReadOnlyRootFilesystem   = true,
                                    AllowPrivilegeEscalation = false,
                                }
                            }
                        },
                        NodeSelector                  = resource.Spec.NodeSelector,
                        RestartPolicy                 = "Always",
                        TerminationGracePeriodSeconds = 30,
                        DnsPolicy                     = "ClusterFirst",
                        ServiceAccountName            = resource.GetFullName(),
                        ServiceAccount                = resource.GetFullName(),
                        SecurityContext               = resource.Spec.PodSecurityContext ??
                        new V1PodSecurityContext()
                        {
                            FsGroup             = fsGroup,
                            FsGroupChangePolicy = "Always"
                        },
                        SchedulerName             = "default-scheduler",
                        Tolerations               = resource.Spec.Tolerations,
                        TopologySpreadConstraints = resource.Spec.TopologySpreadConstraints
                    }
                },
                ServiceName          = Constants.HeadlessServiceName(resource.GetFullName()),
                VolumeClaimTemplates = new List<V1PersistentVolumeClaim>()
                {
                    new V1PersistentVolumeClaim()
                    {
                        Metadata = new V1ObjectMeta()
                        {
                            Name   = Constants.QdrantStorage,
                        },
                        Spec = new V1PersistentVolumeClaimSpec()
                        {
                            AccessModes = new List<string>()
                            {
                                "ReadWriteOnce"
                            },
                            Resources = new V1ResourceRequirements()
                            {
                                Requests = new Dictionary<string,ResourceQuantity>()
                                {
                                    { "storage", new ResourceQuantity(resource.Spec.Persistence.Size) }

                                }
                            },
                            StorageClassName = resource.Spec.Persistence.StorageClassName,
                            VolumeMode       = "Filesystem"
                        }

                    }
                }
            };

            if (resource.Spec.AntiAffinity == true)
            {
                spec.Template.Spec.Affinity = new V1Affinity()
                {
                    PodAntiAffinity = new V1PodAntiAffinity()
                    {
                        RequiredDuringSchedulingIgnoredDuringExecution =
                        [
                            new V1PodAffinityTerm()
                            {
                                LabelSelector = new V1LabelSelector(){
                                    MatchExpressions =
                                    [
                                        new V1LabelSelectorRequirement()
                                        {
                                            Key              = Labels.App,
                                            OperatorProperty = "In",
                                            Values           =
                                            [
                                                resource.GetFullName()
                                            ]
                                        }
                                    ]
                                },
                                TopologyKey = Labels.KubernetesHostname
                            }
                        ]
                    }
                };
            }

            spec.Template.EnsureMetadata();
            spec.Selector.MatchLabels.AddRange(labels);
            spec.Template.Metadata.SetAnnotations(resource.Spec.PodAnnotations);
            spec.Template.Metadata.SetLabels(labels);
            spec.Template.Metadata.SetLabels(resource.Spec.PodLabels);
            spec.VolumeClaimTemplates.First().Metadata.SetLabels(labels);

            if (exists)
            {
                statefulSet.Spec.Replicas                                = spec.Replicas;
                statefulSet.Spec.Template                                = spec.Template;
                statefulSet.Spec.UpdateStrategy                          = spec.UpdateStrategy;
                statefulSet.Spec.PersistentVolumeClaimRetentionPolicy    = spec.PersistentVolumeClaimRetentionPolicy;
                statefulSet.Spec.MinReadySeconds                         = spec.MinReadySeconds;
                statefulSet.Spec.Template.Spec.Tolerations               = spec.Template.Spec.Tolerations;
                statefulSet.Spec.Template.Spec.TopologySpreadConstraints = spec.Template.Spec.TopologySpreadConstraints;

                await k8s.AppsV1.ReplaceNamespacedStatefulSetAsync(
                    body:               statefulSet,
                    name:               statefulSet.Name(),
                    namespaceParameter: resource.Namespace());
            }
            else
            {
                statefulSet.Spec = spec;

                await k8s.AppsV1.CreateNamespacedStatefulSetAsync(
                    body:               statefulSet,
                    namespaceParameter: resource.Namespace());
            }
        }

        internal async Task UpsertServiceAsync(V1QdrantCluster resource)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            var serviceList = await k8s.CoreV1.ListNamespacedServiceAsync(
                namespaceParameter: resource.Namespace(),
                fieldSelector:      $"metadata.name={resource.GetFullName()}");

            V1Service service;
            bool exists = false;
            if (serviceList.Items.Count > 0)
            {
                logger.LogInformationEx(() => $"Service for {resource.GetFullName()}/Qdrant exists, updating existing Service.");
                exists  = true;
                service = serviceList.Items[0];
            }
            else
            {
                service = new V1Service().Initialize();
                service.Metadata.Name = resource.GetFullName();
                service.Metadata.SetNamespace(resource.Namespace());
                service.Metadata.SetLabels(labels);
                service.AddOwnerReference(resource.MakeOwnerReference());
            }

            service.Metadata.SetLabel("metrics", "true");

            service.Spec = CreateServiceSpec(resource.GetFullName(), false);

            if (exists)
            {
                await k8s.CoreV1.ReplaceNamespacedServiceAsync(
                    body:               service,
                    name:               service.Name(),
                    namespaceParameter: resource.Namespace());
            }
            else
            {
                await k8s.CoreV1.CreateNamespacedServiceAsync(
                    body:               service,
                    namespaceParameter: resource.Namespace());
            }

        }

        internal async Task UpsertHeadlessServiceAsync(V1QdrantCluster resource)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            var serviceList = await k8s.CoreV1.ListNamespacedServiceAsync(
                namespaceParameter: resource.Namespace(),
                fieldSelector:      $"metadata.name={resource.GetFullName()}");

            V1Service service;
            bool exists = false;
            if (serviceList.Items.Count > 0)
            {
                logger.LogInformationEx(() => $"Service for {resource.GetFullName()}/Qdrant exists, updating existing Service.");
                exists  = true;
                service = serviceList.Items[0];
            }
            else
            {
                service = new V1Service().Initialize();
                service.Metadata.Name = Constants.HeadlessServiceName(resource.GetFullName());
                service.Metadata.SetNamespace(resource.Namespace());
                service.Metadata.SetLabels(labels);
                service.AddOwnerReference(resource.MakeOwnerReference());
            }
            service.Spec = CreateServiceSpec(resource.GetFullName(), true);

            if (exists)
            {
                service.Spec.ClusterIP = null; // it's immutable

                await k8s.CoreV1.ReplaceNamespacedServiceAsync(
                    body:               service,
                    name:               service.Name(),
                    namespaceParameter: resource.Namespace());
            }
            else
            {
                await k8s.CoreV1.CreateNamespacedServiceAsync(
                    body:               service,
                    namespaceParameter: resource.Namespace());
            }
        }

        internal async Task UpsertConfigMapAsync(V1QdrantCluster resource)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            var configMapList = await k8s.CoreV1.ListNamespacedConfigMapAsync(
                namespaceParameter: resource.Namespace(),
                fieldSelector:      $"metadata.name={resource.GetFullName()}");

            V1ConfigMap configMap;
            bool exists = false;
            if (configMapList.Items.Count > 0)
            {
                logger.LogInformationEx(() => $"ConfigMap for {resource.GetFullName()}/Qdrant exists, updating existing ConfigMap.");
                exists    = true;
                configMap = configMapList.Items[0];
            }
            else
            {
                configMap = new V1ConfigMap().Initialize();
                configMap.Metadata.Name = resource.GetFullName();
                configMap.Metadata.SetNamespace(resource.Namespace());
                configMap.Metadata.SetLabels(labels);
                configMap.AddOwnerReference(resource.MakeOwnerReference());
            }

            var configData = new Dictionary<string, string>();

            configData.Add("initialize.sh", 
$@"#!/bin/sh
SET_INDEX=${{HOSTNAME##*-}}
echo ""Starting initializing for pod $SET_INDEX""
if [ ""$SET_INDEX"" = ""0"" ]; then
    exec ./entrypoint.sh --uri 'http://{resource.GetFullName()}-0.{Constants.HeadlessServiceName(resource.GetFullName())}:{Constants.P2pPort}'
else
    exec ./entrypoint.sh --bootstrap 'http://{resource.GetFullName()}-0.{Constants.HeadlessServiceName(resource.GetFullName())}:{Constants.P2pPort}' --uri 'http://{resource.GetFullName()}-'""$SET_INDEX""'.{Constants.HeadlessServiceName(resource.GetFullName())}:{Constants.P2pPort}'
fi".Replace("\r\n", "\n"));
            configData.Add("production.yaml", 
$@"cluster:
  consensus:
    tick_period_ms: 100
  enabled: true
  p2p:
    port: {Constants.P2pPort}
service:
  enable_tls: false".Replace("\r\n", "\n"));
            configMap.Data = configData;

            if (exists)
            {
                await k8s.CoreV1.ReplaceNamespacedConfigMapAsync(
                    body:               configMap,
                    name:               configMap.Name(),
                    namespaceParameter: resource.Namespace());
            }
            else
            {
                await k8s.CoreV1.CreateNamespacedConfigMapAsync(
                    body:               configMap,
                    namespaceParameter: resource.Namespace());
            }

        }


        internal async Task ConfigureSecretsAsync(V1QdrantCluster resource)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            if (resource.Spec.ApiKey.Enabled == false && resource.Spec.ReadApiKey.Enabled == false)
            {
                logger?.LogInformationEx(() => "ApiKeys not enabled.");

                return;
            }

            logger?.LogInformationEx(() => "Reconciling secrets.");

            if (resource.Spec.ApiKey.Enabled)
            {
                var secretName = resource.Spec.ApiKey.Secret?.Name ?? resource.GetFullName();
                var secretKey  = resource.Spec.ApiKey.Secret?.Key ?? Constants.ApiKeySecretKey;

                V1Secret secret = null;
                try
                {
                    secret = await k8s.CoreV1.ReadNamespacedSecretAsync(name: secretName, namespaceParameter: resource.Namespace());

                    logger?.LogInformationEx(() => "ApiKey secret exists, checking keys.");

                    if (!secret.Data.TryGetValue(secretKey, out _))
                    {
                        logger?.LogInformationEx(() => $"ApiKey secret does not contain key {secretKey}, creating secret.");

                        secret.Data = secret.Data ?? new Dictionary<string, byte[]>();
                        secret.Data.Add(secretKey, System.Text.Encoding.UTF8.GetBytes(NeonHelper.GetCryptoRandomPassword(resource.Spec.ApiKey.KeyLength)));

                        await k8s.CoreV1.UpsertSecretAsync(secret, namespaceParameter: secret.Namespace());
                    }
                }
                catch (Exception e) when (e is HttpOperationException)
                {
                    logger?.LogInformationEx(() => "ApiKey secret does not exist, creating.");

                    if (((HttpOperationException)e).Response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        secret = new V1Secret().Initialize();
                        secret.Metadata.Name = secretName;
                        secret.Metadata.NamespaceProperty = resource.Namespace();
                        secret.Metadata.SetLabels(labels);
                        secret.AddOwnerReference(resource.MakeOwnerReference());

                        secret.Data = new Dictionary<string, byte[]>();
                        secret.Data.Add(secretKey, System.Text.Encoding.UTF8.GetBytes(NeonHelper.GetCryptoRandomPassword(resource.Spec.ApiKey.KeyLength)));

                        await k8s.CoreV1.UpsertSecretAsync(secret, namespaceParameter: secret.Namespace());
                    }
                }
            }

            if (resource.Spec.ReadApiKey.Enabled)
            {
                var secretName = resource.Spec.ReadApiKey.Secret?.Name ?? resource.GetFullName();
                var secretKey  = resource.Spec.ReadApiKey.Secret?.Key ?? Constants.ReadApiKeySecretKey;

                V1Secret secret = null;
                try
                {
                    secret = await k8s.CoreV1.ReadNamespacedSecretAsync(name: secretName, namespaceParameter: resource.Namespace());

                    logger?.LogInformationEx(() => "ReadApiKey secret exists, checking keys.");

                    if (!secret.Data.TryGetValue(secretKey, out _))
                    {
                        logger?.LogInformationEx(() => $"ReadApiKey secret does not contain key {secretKey}, creating secret.");

                        secret.Data = secret.Data ?? new Dictionary<string, byte[]>();
                        secret.Data.Add(secretKey, System.Text.Encoding.UTF8.GetBytes(NeonHelper.GetCryptoRandomPassword(resource.Spec.ReadApiKey.KeyLength)));

                        await k8s.CoreV1.UpsertSecretAsync(secret, namespaceParameter: secret.Namespace());
                    }
                }
                catch (Exception e) when (e is HttpOperationException)
                {
                    logger?.LogInformationEx(() => "ReadApiKey secret does not exist, creating.");

                    if (((HttpOperationException)e).Response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        secret = new V1Secret().Initialize();
                        secret.Metadata.Name = secretName;
                        secret.Metadata.NamespaceProperty = resource.Namespace();
                        secret.Metadata.SetLabels(labels);
                        secret.AddOwnerReference(resource.MakeOwnerReference());

                        secret.Data = new Dictionary<string, byte[]>();
                        secret.Data.Add(secretKey, System.Text.Encoding.UTF8.GetBytes(NeonHelper.GetCryptoRandomPassword(resource.Spec.ReadApiKey.KeyLength)));

                        await k8s.CoreV1.UpsertSecretAsync(secret, namespaceParameter: secret.Namespace());
                    }
                }
            }
        }

        internal async Task UpsertServiceAccountAsync(V1QdrantCluster resource)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            var serviceAccountList = await k8s.CoreV1.ListNamespacedServiceAccountAsync(
                namespaceParameter: resource.Namespace(),
                fieldSelector:      $"metadata.name={resource.GetFullName()}");

            V1ServiceAccount serviceAccount;
            bool exists = false;

            if (serviceAccountList.Items.Count > 0)
            {
                logger.LogInformationEx(() => $"Service Account for {resource.GetFullName()}/Qdrant exists, updating existing Service Account.");
                exists         = true;
                serviceAccount = serviceAccountList.Items[0];
            }
            else
            {
                serviceAccount = new V1ServiceAccount().Initialize();
                serviceAccount.Metadata.Name = resource.GetFullName();
                serviceAccount.Metadata.SetNamespace(resource.Namespace());
                serviceAccount.Metadata.SetLabels(labels);
                serviceAccount.AddOwnerReference(resource.MakeOwnerReference());
            }

            if (exists)
            {
                await k8s.CoreV1.ReplaceNamespacedServiceAccountAsync(
                    body:               serviceAccount,
                    name:               serviceAccount.Name(),
                    namespaceParameter: resource.Namespace());
            }
            else
            {
                await k8s.CoreV1.CreateNamespacedServiceAccountAsync(
                    body:               serviceAccount,
                    namespaceParameter: resource.Namespace());
            }
        }

        internal async Task UpsertServiceMonitorAsync(V1QdrantCluster resource)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            var metadata = typeof(V1ServiceMonitor).GetKubernetesTypeMetadata();
            var review   = new V1SelfSubjectAccessReview().Initialize();

            review.Spec = new V1SelfSubjectAccessReviewSpec();
            review.Spec.ResourceAttributes = new V1ResourceAttributes()
            {
                Group             = metadata.Group,
                Version           = metadata.ApiVersion,
                Resource          = metadata.PluralName,
                Verb              = "list, get, create, update",
                NamespaceProperty = resource.Namespace()
            };

            review = await k8s.AuthorizationV1.CreateSelfSubjectAccessReviewAsync(review);

            if (review.Status.Allowed != true)
            {
                logger?.LogErrorEx(() => $"Not authorized. Qdrant-operator needs [create, get, list, update] permissions for {metadata.ApiVersion}/{metadata.PluralName}");
            }

            var serviceMonitorList = await k8s.CustomObjects.ListNamespacedCustomObjectAsync<V1ServiceMonitor>(
                namespaceParameter: resource.Namespace(),
                fieldSelector:      $"metadata.name={resource.GetFullName()}");

            V1ServiceMonitor serviceMonitor;

            if (serviceMonitorList.Items.Count > 0)
            {
                logger.LogInformationEx(() => $"ServiceMonitor for {resource.GetFullName()}/Qdrant exists, updating existing ServiceMonitor.");
                serviceMonitor = serviceMonitorList.Items[0];
            }
            else
            {
                serviceMonitor = new V1ServiceMonitor().Initialize();
                serviceMonitor.Metadata.Name = resource.GetFullName();
                serviceMonitor.Metadata.SetNamespace(resource.Namespace());
                serviceMonitor.Metadata.SetLabels(labels);
                serviceMonitor.AddOwnerReference(resource.MakeOwnerReference());
            }

            serviceMonitor.Spec = new V1ServiceMonitorSpec();
            serviceMonitor.Spec.Endpoints = new List<Endpoint>()
            {
                new Endpoint()
                {
                    Interval      = resource.Spec.Metrics.Interval,
                    Path          = "/metrics",
                    Port          = Constants.HttpPortName,
                    Scheme        = "http",
                    HonorLabels   = resource.Spec.Metrics.HonorLabels,
                    ScrapeTimeout = resource.Spec.Metrics.ScrapeTimeout,
                }
            };
            serviceMonitor.Spec.Selector = new V1LabelSelector()
            {
                MatchLabels = new Dictionary<string, string>()
            };

            serviceMonitor.Spec.Selector.MatchLabels.AddRange(labels);
            serviceMonitor.Spec.Selector.MatchLabels.Add("metrics", "true");

            await k8s.CustomObjects.UpsertNamespacedCustomObjectAsync(
                   body:               serviceMonitor,
                   name:               serviceMonitor.Metadata.Name,
                   namespaceParameter: resource.Namespace());
        }

        internal async Task UpsertGrafanaDashboardAsync(V1QdrantCluster resource)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            var metadata = typeof(V1GrafanaDashboard).GetKubernetesTypeMetadata();
            var review   = new V1SelfSubjectAccessReview().Initialize();

            review.Spec = new V1SelfSubjectAccessReviewSpec();
            review.Spec.ResourceAttributes = new V1ResourceAttributes()
            {
                Group = metadata.Group,
                Version = metadata.ApiVersion,
                Resource = metadata.PluralName,
                Verb = "create,get,list,update",
                NamespaceProperty = resource.Namespace()
            };

            review = await k8s.AuthorizationV1.CreateSelfSubjectAccessReviewAsync(review);

            if (review.Status.Allowed != true)
            {
                logger?.LogErrorEx(() => $"Not authorized. Qdrant-operator needs [create, get, list, update] permissions for {metadata.ApiVersion}/{metadata.PluralName}");
            }

            var grafanaDashboard = await k8s.CustomObjects.GetNamespacedCustomObjectAsync<V1GrafanaDashboard>(
                namespaceParameter: resource.Namespace(),
                name: resource.GetFullName(),
                throwIfNotFound: false);

            if (grafanaDashboard == null)
            {     
                grafanaDashboard = new V1GrafanaDashboard().Initialize();
                grafanaDashboard.Metadata.Name = resource.GetFullName();
                grafanaDashboard.Metadata.SetNamespace(resource.Namespace());
                grafanaDashboard.Metadata.SetLabels(labels);
                grafanaDashboard.AddOwnerReference(resource.MakeOwnerReference());
            }

            var metricInterval = DurationHelper.ParseDuration(resource.Spec.Metrics.Interval);

            foreach (var label in resource.Spec.Metrics.Grafana.InstanceSelector.MatchLabels)
            {
                grafanaDashboard.SetLabel(label.Key, label.Value);
            }

            grafanaDashboard.Spec = new V1GrafanaDashboardSpec();
            grafanaDashboard.Spec.Json = string.Format(
                format: Dashboard.DashboardJson,
                DurationHelper.ToDurationString(metricInterval * 2), resource.Uid());

            grafanaDashboard.Spec.Datasources = new List<V1GrafanaDatasource>()
            {
                new V1GrafanaDatasource()
                {
                    InputName      = "DS_PROMETHEUS",
                    DatasourceName = resource.Spec.Metrics.Grafana.DatasourceName
                }
            };

            await k8s.CustomObjects.UpsertNamespacedCustomObjectAsync(
                   body:               grafanaDashboard,
                   name:               grafanaDashboard.Metadata.Name,
                   namespaceParameter: resource.Namespace());
        }

        internal V1ServiceSpec CreateServiceSpec(string selectorName, bool headless = false)
        {
            var spec = new V1ServiceSpec()
            {
                Ports = new List<V1ServicePort>()
                {
                    new V1ServicePort()
                    {
                        Name          = Constants.HttpPortName,
                        Protocol      = "TCP",
                        Port          = Constants.HttpPort,
                        TargetPort    = Constants.HttpPort,
                        AppProtocol   = "http",
                    },
                    new V1ServicePort()
                    {
                        Name          = Constants.GrpcPortName,
                        Protocol      = "TCP",
                        Port          = Constants.GrpcPort,
                        TargetPort    = Constants.GrpcPort,
                        AppProtocol   = "grpc"
                    },
                    new V1ServicePort()
                    {
                        Name          = Constants.P2pPortName,
                        Protocol      = "TCP",
                        Port          = Constants.P2pPort,
                        TargetPort    = Constants.P2pPort,
                        AppProtocol   = "tcp"
                    }
                },
                Selector              = new Dictionary<string, string>(),
                Type                  = "ClusterIP",
                InternalTrafficPolicy = "Cluster"

            };

            spec.Selector.AddRange(labels);

            if (headless)
            {
                spec.ClusterIP        = "None";
            }

            return spec;
        }

        private async Task WaitForReplicasAsync(V1QdrantCluster resource)
        {
            await SyncContext.Clear;

            // Start the watcher.

            var cts = new CancellationTokenSource();

            await k8s.WatchAsync<V1StatefulSet>(
                async (@event) =>
                {
                    await SyncContext.Clear;

                    if (@event.Value.Status.Replicas == @event.Value.Status.AvailableReplicas)
                    {
                        cts.Cancel();
                    }
                },
                namespaceParameter: resource.Namespace(),
                fieldSelector:      $"metadata.name={resource.GetFullName()}",
                retryDelay:         TimeSpan.FromSeconds(30),
                logger:             logger,
                cancellationToken:  cts.Token);
        }
    }

}
