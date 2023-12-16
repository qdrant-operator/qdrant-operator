using System;
using System.Threading.Tasks;

using k8s;
using k8s.Models;

using Microsoft.Extensions.Logging;

using Neon.Operator.Finalizers;
using Neon.Tasks;

namespace QdrantOperator
{
    public class QdrantClusterFinalizer : ResourceFinalizerBase<V1QdrantCluster>
    {
        private readonly IKubernetes k8s;
        private readonly ILogger<QdrantClusterController> logger;
        public QdrantClusterFinalizer(
            IKubernetes k8s,
            ILogger<QdrantClusterController> logger)
        {
            this.k8s = k8s;
            this.logger = logger;
        }

        public override async Task FinalizeAsync(V1QdrantCluster resource)
        {
            await SyncContext.Clear;

            logger.LogInformation($"FINALIZED: {resource.Name()}");

            await DeleteStatefulsetAsync(resource);
            await DeleteServiceAsync(resource);
            await DeleteHeadlessServiceAsync(resource);
            await DeleteConfigMapAsync(resource);
            await DeleteServiceAccountAsync(resource);
        }

        public async Task DeleteStatefulsetAsync(V1QdrantCluster resource)
        {
            bool statefulsetExists = false;
            try
            {
                await k8s.AppsV1.ReadNamespacedStatefulSetAsync(
                    name: resource.Name(),
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
                        name: resource.Name(),
                        namespaceParameter: resource.Namespace());
            }

        }
        public async Task DeleteServiceAsync(V1QdrantCluster resource)
        {
            bool serviceExists = false;
            try
            {
                await k8s.CoreV1.ReadNamespacedServiceAsync(
                    name: resource.Name(),
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
                        name: resource.Name(),
                        namespaceParameter: resource.Namespace());
            }

        }
        public async Task DeleteHeadlessServiceAsync(V1QdrantCluster resource)
        {
            bool serviceExists = false;
            try
            {
                await k8s.CoreV1.ReadNamespacedServiceAsync(
                    name: resource.Name(),
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
                        name: resource.Name(),
                        namespaceParameter: resource.Namespace());
            }

        }
        public async Task DeleteConfigMapAsync(V1QdrantCluster resource)
        {
            bool configMapExists = false;
            try
            {
                await k8s.CoreV1.ReadNamespacedConfigMapAsync(
                    name: resource.Name(),
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
                        name: resource.Name(),
                        namespaceParameter: resource.Namespace());
            }

        }
        public async Task DeleteServiceAccountAsync(V1QdrantCluster resource)
        {
            bool serviceExists = false;
            try
            {
                await k8s.CoreV1.ReadNamespacedServiceAccountAsync(
                    name: resource.Name(),
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
                        name: resource.Name(),
                        namespaceParameter: resource.Namespace());
            }

        }


    }
}

