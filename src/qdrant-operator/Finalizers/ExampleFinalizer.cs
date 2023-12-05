using k8s;
using k8s.Models;

using Microsoft.Extensions.Logging;

using Neon.Operator.Finalizers;
using Neon.Tasks;

using System;
using System.Threading.Tasks;

namespace QdrantOperator
{
    public class ExampleFinalizer : ResourceFinalizerBase<V1ExampleEntity>
    {
        private readonly IKubernetes k8s;
        private readonly ILogger<ExampleController> logger;
        public ExampleFinalizer(
            IKubernetes k8s,
            ILogger<ExampleController> logger)
        {
            this.k8s = k8s;
            this.logger = logger;
        }

        public async Task FinalizeAsync(V1ExampleEntity resource)
        {
            await SyncContext.ClearAsync;

            logger.LogInformation($"FINALIZED: {resource.Name()}");
        }
    }
}
