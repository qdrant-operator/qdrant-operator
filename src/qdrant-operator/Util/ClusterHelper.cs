using System;
using System.Text;
using System.Threading.Tasks;

using k8s;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Neon.Common;
using Neon.Diagnostics;
using Neon.K8s.PortForward;
using Neon.Net;
using Neon.Tasks;

using Qdrant.Client;

using QdrantOperator.Entities;

namespace QdrantOperator.Util
{
    /// <summary>
    /// Qdrant cluster helper.
    /// </summary>
    public class ClusterHelper : IClusterHelper
    {
        private readonly IKubernetes            k8s;
        private readonly ILogger<ClusterHelper> logger;
        private readonly ILoggerFactory         loggerFactory;
        private readonly IServiceProvider       services;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="k8s"></param>
        /// <param name="logger"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="services"></param>
        public ClusterHelper(
            IKubernetes            k8s,
            ILogger<ClusterHelper> logger,
            ILoggerFactory         loggerFactory,
            IServiceProvider       services)
        {
            this.k8s           = k8s;
            this.logger        = logger;
            this.loggerFactory = loggerFactory;
            this.services      = services;
        }

        public async Task<QdrantClient> CreateQdrantClientAsync(
            V1QdrantCluster cluster,
            string namespaceParameter,
            string nodeName = null)
        {
            await SyncContext.Clear;

            var serviceName = cluster.GetFullName();

            var clusterHost = $"{serviceName}.{namespaceParameter}";
            var clusterPort = 6334;

            logger?.LogInformationEx(() => $"Connecting to cluster: {cluster.Metadata.Name} at: [{clusterHost}:{clusterPort}]");

            if (NeonHelper.IsDevWorkstation)
            {
                var portManager = this.services.GetRequiredService<PortForwardManager>();

                var port = NetHelper.GetUnusedTcpPort();

                if (!nodeName.IsNullOrEmpty())
                {
                    portManager.StartPodPortForward(nodeName, namespaceParameter, port, clusterPort);
                }
                else
                {
                    await portManager.StartServicePortForwardAsync(cluster.GetFullName(), namespaceParameter, port, clusterPort);
                }

                clusterHost = "localhost";
                clusterPort = port;
            }

            string apiKey = null;
            if (cluster.Spec.ApiKey?.Enabled == true)
            {
                var secretName = cluster.Spec.ApiKey.Secret?.Name ?? cluster.GetFullName();
                var secretKey  = cluster.Spec.ApiKey.Secret?.Key ?? Constants.ApiKeySecretKey;

                var secret = await k8s.CoreV1.ReadNamespacedSecretAsync(name: secretName, namespaceParameter: namespaceParameter);
                apiKey = Encoding.UTF8.GetString(secret.Data[secretKey]);
            }

            if (string.IsNullOrEmpty(apiKey)
                && cluster.Spec.ReadApiKey?.Enabled == true)
            {
                var secretName = cluster.Spec.ReadApiKey.Secret?.Name ?? cluster.GetFullName();
                var secretKey  = cluster.Spec.ReadApiKey.Secret?.Key ?? Constants.ReadApiKeySecretKey;

                var secret = await k8s.CoreV1.ReadNamespacedSecretAsync(name: secretName, namespaceParameter: namespaceParameter);
                apiKey = Encoding.UTF8.GetString(secret.Data[secretKey]);
            }

            var qdrantClient = new QdrantClient(
                host:          clusterHost,
                port:          clusterPort,
                https:         false,
                apiKey:        apiKey,
                grpcTimeout:   TimeSpan.FromSeconds(60),
                loggerFactory: this.loggerFactory);

            return qdrantClient;
        }

    }
}
