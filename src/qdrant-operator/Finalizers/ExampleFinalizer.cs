using k8s;
using k8s.Models;

using Microsoft.Extensions.Logging;

using Neon.Operator.Finalizers;
using Neon.Tasks;

using System;
using System.Threading.Tasks;

namespace QdrantOperator
{
    public class ExampleFinalizer : ResourceFinalizerBase<V1QdrantCluster>
    {
        private readonly IKubernetes k8s;
        private readonly ILogger<QdrantClusterController> logger;
        public ExampleFinalizer(
            IKubernetes k8s,
            ILogger<QdrantClusterController> logger)
        {
            this.k8s = k8s;
            this.logger = logger;
        }

        public async Task FinalizeAsync(V1QdrantCluster resource)
        {
            await SyncContext.Clear;

            logger.LogInformation($"FINALIZED: {resource.Name()}");
        }
    }
}
