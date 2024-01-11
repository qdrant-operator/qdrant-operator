using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using k8s;
using k8s.Models;

using Microsoft.Extensions.Logging;

using Neon.Common;
using Neon.Diagnostics;
using Neon.K8s;
using Neon.K8s.Resources.Prometheus;
using Neon.Operator;
using Neon.Operator.Attributes;
using Neon.Operator.Controllers;
using Neon.Operator.Finalizers;
using Neon.Operator.Rbac;
using Neon.Tasks;

using QdrantOperator.Entities;

namespace QdrantOperator
{
    [RbacRule<V1StatefulSet>(Scope = EntityScope.Cluster, Verbs = RbacVerb.All)]
    [RbacRule<V1ConfigMap>(Scope = EntityScope.Cluster, Verbs = RbacVerb.All)]
    [RbacRule<V1QdrantCluster>(Scope = EntityScope.Cluster, Verbs = RbacVerb.All, SubResources = "status")]
    [RbacRule<V1Service>(Scope = EntityScope.Cluster, Verbs = RbacVerb.All)]
    [RbacRule<V1ServiceAccount>(Scope = EntityScope.Cluster, Verbs = RbacVerb.All)]
    [RbacRule<V1PersistentVolumeClaim>(Scope = EntityScope.Cluster, Verbs = RbacVerb.All, SubResources = "status")]
    [ResourceController(AutoRegisterFinalizers = true)]
    public class QdrantClusterController : ResourceControllerBase<V1QdrantCluster>
    {
        private readonly IKubernetes                        k8s;
        private readonly IFinalizerManager<V1QdrantCluster> finalizerManager;
        private readonly ILogger<QdrantClusterController>   logger;
        private Dictionary<string, string>                  labels;
        public QdrantClusterController(
            IKubernetes                        k8s,
            IFinalizerManager<V1QdrantCluster> finalizerManager,
            ILogger<QdrantClusterController>   logger)
        {
            this.k8s              = k8s;
            this.finalizerManager = finalizerManager;
            this.logger           = logger;
        }

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

            labels = new Dictionary<string, string>();
            labels.Add("app", resource.GetFullName());
            labels.Add("app.kubernetes.io/instance", resource.Metadata.Name);
            labels.Add("app.kubernetes.io/name", resource.Metadata.Name);
            labels.Add("app.kubernetes.io/version", resource.Spec.Image.Tag);
            labels.Add(Constants.ManagedByLabel, Constants.ManagedBy);

            labels = new Dictionary<string, string>();
            labels.Add("app", resource.GetFullName());
            labels.Add("app.kubernetes.io/instance", resource.Metadata.Name);
            labels.Add("app.kubernetes.io/name", resource.Metadata.Name);
            labels.Add("app.kubernetes.io/version", resource.Spec.Image.Tag);

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

            await NeonHelper.WaitAllAsync(tasks);
        }

        public async Task CheckVolumesAsync(V1QdrantCluster resource)
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
                    body: pvc,
                    name: volumeName,
                    namespaceParameter: resource.Namespace());


            }
        }
        public async Task UpsertStatefulsetAsync(V1QdrantCluster resource)
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
                statefulSet.Metadata.Labels = labels;
                statefulSet.AddOwnerReference(resource.MakeOwnerReference());
            }

            var spec = new V1StatefulSetSpec()
            {
                Replicas = resource.Spec.Replicas,
                Selector = new V1LabelSelector()
                {
                    MatchLabels = labels
                },
                Template = new V1PodTemplateSpec()
                {
                    Metadata = new V1ObjectMeta()
                    {
                        Labels = labels
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
                                    "1000:3000",
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
                                ImagePullPolicy          = "IfNotPresent",
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
                                Env = new List<V1EnvVar>()
                                {
                                    new V1EnvVar()
                                    {
                                        Name  = "QDRANT_INIT_FILE_PATH",
                                        Value = "/qdrant/init/.qdrant-initialized",
                                    }
                                },
                                Resources = new V1ResourceRequirements(){},
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
                                ImagePullPolicy          = "IfNotPresent",
                                SecurityContext          = new V1SecurityContext()
                                {
                                    Privileged             = false,
                                    RunAsUser              = 1000,
                                    RunAsGroup             = 2000,
                                    RunAsNonRoot           = true,
                                    ReadOnlyRootFilesystem = true,
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
                        SecurityContext               = new V1PodSecurityContext()
                        {
                            FsGroup             = 3000,
                            FsGroupChangePolicy = "Always"
                        },
                        SchedulerName = "default-scheduler"
                    }
                },
                ServiceName          = Constants.HeadlessServiceName(resource.GetFullName())
            };

            if (exists)
            {
                spec.VolumeClaimTemplates = statefulSet.Spec.VolumeClaimTemplates;
            }
            else
            {
                spec.VolumeClaimTemplates = new List<V1PersistentVolumeClaim>()
                {
                    new V1PersistentVolumeClaim()
                    {
                        Metadata = new V1ObjectMeta()
                        {
                            Name   = Constants.QdrantStorage,
                            Labels = labels
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
                };
            }

            statefulSet.Spec = spec;

            if (exists)
            {
                await k8s.AppsV1.ReplaceNamespacedStatefulSetAsync(
                    body:               statefulSet, 
                    name:statefulSet.Name(),
                    namespaceParameter: statefulSet.Metadata.NamespaceProperty);
            }
            else
            {
                await k8s.AppsV1.CreateNamespacedStatefulSetAsync(
                    body:               statefulSet,
                    namespaceParameter: statefulSet.Metadata.NamespaceProperty);
            }

        }

        public async Task UpsertServiceAsync(V1QdrantCluster resource)
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
                service.Metadata.Labels = labels;
                service.AddOwnerReference(resource.MakeOwnerReference());
            }
            service.Spec = CreateServiceSpec(resource.GetFullName(), false);

            if (exists)
            {
                await k8s.CoreV1.ReplaceNamespacedServiceAsync(
                    body:               service,
                    name:               service.Name(),
                    namespaceParameter: service.Metadata.NamespaceProperty);
            }
            else
            {
                await k8s.CoreV1.CreateNamespacedServiceAsync(
                    body:               service,
                    namespaceParameter: service.Metadata.NamespaceProperty);
            }

        }

        public async Task UpsertHeadlessServiceAsync(V1QdrantCluster resource)
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
                service.Metadata.Labels = labels;
                service.AddOwnerReference(resource.MakeOwnerReference());
            }
            service.Spec = CreateServiceSpec(resource.GetFullName(), true);

            if (exists)
            {
                service.Spec.ClusterIP = null; // it's immutable

                await k8s.CoreV1.ReplaceNamespacedServiceAsync(
                    body:               service,
                    name:               service.Name(),
                    namespaceParameter: service.Metadata.NamespaceProperty);
            }
            else
            {
                await k8s.CoreV1.CreateNamespacedServiceAsync(
                    body:               service,
                    namespaceParameter: service.Metadata.NamespaceProperty);
            }
        }

        public async Task UpsertConfigMapAsync(V1QdrantCluster resource)
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
                configMap.Metadata.Labels = labels;
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
                    namespaceParameter: configMap.Metadata.NamespaceProperty);
            }
            else
            {
                await k8s.CoreV1.CreateNamespacedConfigMapAsync(
                    body:               configMap,
                    namespaceParameter: configMap.Metadata.NamespaceProperty);
            }

        }

        public async Task UpsertServiceAccountAsync(V1QdrantCluster resource)
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
                serviceAccount.Metadata.Labels = labels;
                serviceAccount.AddOwnerReference(resource.MakeOwnerReference());
            }

            if (exists)
            {
                await k8s.CoreV1.ReplaceNamespacedServiceAccountAsync(
                    body:               serviceAccount,
                    name:               serviceAccount.Name(),
                    namespaceParameter: serviceAccount.Metadata.NamespaceProperty);
            }
            else
            {
                await k8s.CoreV1.CreateNamespacedServiceAccountAsync(
                    body:               serviceAccount,
                    namespaceParameter: serviceAccount.Metadata.NamespaceProperty);
            }
        }

        public async Task UpsertServiceMonitorAsync(V1QdrantCluster resource)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

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
                serviceMonitor.Metadata.Labels = labels;
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
                MatchLabels = labels
            };


            await k8s.CustomObjects.UpsertNamespacedCustomObjectAsync(
                   body:               serviceMonitor,
                   name:               serviceMonitor.Name(),
                   namespaceParameter: serviceMonitor.Metadata.NamespaceProperty);
        }

        public V1ServiceSpec CreateServiceSpec(string selectorName, bool headless = false)
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
                Selector              = labels,
                Type                  = "ClusterIP",
                InternalTrafficPolicy = "Cluster"

            };

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
