using k8s;
using k8s.Models;

using Microsoft.Extensions.Logging;

using Neon.Operator.Controllers;
using Neon.Operator.Finalizers;
using Neon.Operator.ResourceManager;
using Neon.Tasks;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QdrantOperator
{
    public class ExampleController : ResourceControllerBase<V1ExampleEntity>
    {
        private readonly IKubernetes k8s;
        private readonly IFinalizerManager<V1ExampleEntity> finalizerManager;
        private readonly ILogger<ExampleController> logger;

        public ExampleController(
            IKubernetes k8s,
            IFinalizerManager<V1ExampleEntity> finalizerManager,
            ILogger<ExampleController> logger)
        {
            this.k8s = k8s;
            this.finalizerManager = finalizerManager;
            this.logger = logger;
        }

        public async Task<ResourceControllerResult> ReconcileAsync(V1ExampleEntity resource)
        {
            await SyncContext.ClearAsync;

            logger.LogInformation($"RECONCILING: {resource.Name()}");

            await finalizerManager.RegisterAllFinalizersAsync(resource);

            logger.LogInformation($"RECONCILED: {resource.Name()}");

            return ResourceControllerResult.Ok();
        }
    }
}
