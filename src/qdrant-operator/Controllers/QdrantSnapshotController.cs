using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AsyncKeyedLock;

using k8s;

using Microsoft.Extensions.Logging;

using Neon.Operator.Attributes;
using Neon.Operator.Controllers;
using Neon.Operator.Rbac;


namespace QdrantOperator.Controllers
{

    /// <summary>
    /// QdrantSnapshot Controller
    /// </summary>
    [RbacRule<QdrantSnapshot>(Scope = EntityScope.Cluster, Verbs = RbacVerb.All)]
    public class QdrantSnapshotController : ResourceControllerBase<QdrantSnapshot>
    {
        private readonly IKubernetes                        k8s;
        private readonly ILogger<QdrantSnapshotController>  logger;
        private readonly AsyncKeyedLocker                   lockProvider;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="k8s"></param>
        /// <param name="logger"></param>
        /// <param name="lockProvider"></param>
        public QdrantSnapshotController(
            IKubernetes k8s,
            ILogger<QdrantSnapshotController> logger,
            AsyncKeyedLocker lockProvider)
        {
            this.k8s          = k8s;
            this.logger       = logger;
            this.lockProvider = lockProvider;
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
            // somebody updated cluster
            // check the status

            return ResourceControllerResult.Ok();

        }

    }
}
