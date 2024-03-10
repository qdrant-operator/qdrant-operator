using System;
using System.Threading.Tasks;

using k8s;
using k8s.Models;

using Microsoft.Extensions.Logging;

using Neon.Diagnostics;
using Neon.K8s;
using Neon.K8s.Resources.Prometheus;
using Neon.Operator.Finalizers;
using Neon.Tasks;

using QdrantOperator.Entities;

namespace QdrantOperator
{
    /// <summary>
    /// Finalizes a qdrant cluster resource.
    /// </summary>
    public class QdrantClusterFinalizer : ResourceFinalizerBase<V1QdrantCluster>
    {
        private readonly IKubernetes                      k8s;
        private readonly ILogger<QdrantClusterController> logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="k8s"></param>
        /// <param name="logger"></param>
        public QdrantClusterFinalizer(
            IKubernetes                      k8s,
            ILogger<QdrantClusterController> logger)
        {
            this.k8s    = k8s;
            this.logger = logger;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public override async Task FinalizeAsync(V1QdrantCluster resource)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            await DeleteStatefulsetAsync(resource);
            await DeleteServiceAsync(resource);
            await DeleteHeadlessServiceAsync(resource);
            await DeleteConfigMapAsync(resource);
            await DeleteServiceAccountAsync(resource);
            await DeleteServiceMonitorAsync(resource);

            logger.LogInformationEx(() => $"FINALIZED: {resource.Name()}");
        }

        /// <summary>
        /// Deletes the statefulset for the resource.
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public async Task DeleteStatefulsetAsync(V1QdrantCluster resource)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            bool statefulsetExists = false;
            try
            {
                await k8s.AppsV1.ReadNamespacedStatefulSetAsync(
                    name:               resource.GetFullName(),
                    namespaceParameter: resource.Namespace());

                statefulsetExists = true;
            }
            catch (Exception)
            {
                // doesn't exist
            }

            if (statefulsetExists)
            {
                await k8s.AppsV1.DeleteNamespacedStatefulSetAsync(
                    name:               resource.GetFullName(),
                    namespaceParameter: resource.Namespace());
            }

        }

        /// <summary>
        /// Deletes the service for the resource.
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public async Task DeleteServiceAsync(V1QdrantCluster resource)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            bool serviceExists = false;
            try
            {
                await k8s.CoreV1.ReadNamespacedServiceAsync(
                    name:               resource.GetFullName(),
                    namespaceParameter: resource.Namespace());

                serviceExists = true;
            }
            catch (Exception)
            {
                // doesn't exist
            }

            if (serviceExists)
            {
                await k8s.CoreV1.DeleteNamespacedServiceAsync(
                    name:               resource.GetFullName(),
                    namespaceParameter: resource.Namespace());
            }

        }

        /// <summary>
        /// Deletes the headless service for the resource.
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public async Task DeleteHeadlessServiceAsync(V1QdrantCluster resource)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            bool serviceExists = false;
            try
            {
                await k8s.CoreV1.ReadNamespacedServiceAsync(
                    name:               Constants.HeadlessServiceName(resource.GetFullName()),
                    namespaceParameter: resource.Namespace());

                serviceExists = true;
            }
            catch (Exception)
            {
                // doesn't exist
            }

            if (serviceExists)
            {
                await k8s.CoreV1.DeleteNamespacedServiceAsync(
                    name:               Constants.HeadlessServiceName(resource.GetFullName()),
                    namespaceParameter: resource.Namespace());
            }

        }

        /// <summary>
        /// Deletes the configmap for the resource.
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public async Task DeleteConfigMapAsync(V1QdrantCluster resource)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            bool configMapExists = false;
            try
            {
                await k8s.CoreV1.ReadNamespacedConfigMapAsync(
                    name:               resource.GetFullName(),
                    namespaceParameter: resource.Namespace());

                configMapExists = true;
            }
            catch (Exception)
            {
                // doesn't exist
            }

            if (configMapExists)
            {
                await k8s.CoreV1.DeleteNamespacedConfigMapAsync(
                    name:               resource.GetFullName(),
                    namespaceParameter: resource.Namespace());
            }

        }

        /// <summary>
        /// Deletes the service account for the resource.
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public async Task DeleteServiceAccountAsync(V1QdrantCluster resource)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            bool serviceExists = false;
            try
            {
                await k8s.CoreV1.ReadNamespacedServiceAccountAsync(
                    name: resource.GetFullName(),
                    namespaceParameter: resource.Namespace());

                serviceExists = true;
            }
            catch (Exception)
            {
                // doesn't exist
            }

            if (serviceExists)
            {
                await k8s.CoreV1.DeleteNamespacedServiceAccountAsync(
                    name: resource.GetFullName(),
                    namespaceParameter: resource.Namespace());
            }

        }

        /// <summary>
        /// Deletes any service monitors for the resource.
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public async Task DeleteServiceMonitorAsync(V1QdrantCluster resource)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            var service = await k8s.CustomObjects.GetNamespacedCustomObjectAsync<V1ServiceMonitor>(
                    name:               resource.GetFullName(),
                    namespaceParameter: resource.Namespace(),
                    throwIfNotFound:    false);

            if (service != null)
            {
                await k8s.CustomObjects.DeleteNamespacedCustomObjectAsync<V1ServiceMonitor>(
                    name:               resource.GetFullName(),
                    namespaceParameter: resource.Namespace());
            }

        }
    }
}