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
    /// interface for Qdrant cluster helper.
    /// </summary>
    public interface IClusterHelper
    {
        /// <summary>
        /// Creating qdrant client async method
        /// </summary>
        /// <param name="cluster"></param>
        /// <param name="namespaceParameter"></param>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        Task<QdrantClient> CreateQdrantClientAsync(
            V1QdrantCluster cluster,
            string namespaceParameter,
            string nodeName = null);
    }
}
