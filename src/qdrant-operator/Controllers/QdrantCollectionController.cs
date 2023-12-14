using k8s;
using k8s.Models;

using Microsoft.Extensions.Logging;
using Neon.Common;
using Neon.Diagnostics;
using Neon.Operator;
using Neon.Operator.Attributes;
using Neon.Operator.Controllers;
using Neon.Operator.Finalizers;
using Neon.Operator.Rbac;
using Neon.Operator.ResourceManager;
using Neon.Tasks;
using QdrantOperator.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace QdrantOperator.Controllers
{
    [RbacRule<V1QdrantCollection>(Scope = EntityScope.Cluster, Verbs = RbacVerb.All)]
    public class QdrantCollectionController : ResourceControllerBase<V1QdrantCollection>
    {
        private readonly IKubernetes k8s;
        private readonly IFinalizerManager<V1QdrantCollection> finalizerManager;
        private readonly ILogger<QdrantClusterController> logger;
        private Dictionary<string, string> labels;
        public QdrantCollectionController(
            IKubernetes k8s,
            IFinalizerManager<V1QdrantCollection> finalizerManager,
            ILogger<QdrantCollectionController> logger)
        {
            this.k8s = k8s;
            this.finalizerManager = finalizerManager;
            this.logger = logger;
        }

        public override async Task<ResourceControllerResult> ReconcileAsync(V1QdrantCollection resource)
        {
            //u
        }


     }
}
