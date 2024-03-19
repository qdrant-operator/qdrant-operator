using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using k8s;
using k8s.Autorest;
using k8s.Models;

using Microsoft.Extensions.Logging;

using Neon.Common;
using Neon.Diagnostics;
using Neon.K8s;
using Neon.K8s.Resources.Grafana;
using Neon.K8s.Resources.Prometheus;
using Neon.Operator;
using Neon.Operator.Attributes;
using Neon.Operator.Controllers;
using Neon.Operator.Finalizers;
using Neon.Operator.Rbac;
using Neon.Tasks;

using QdrantOperator.Entities;
using QdrantOperator.Util;
using QdrantOperator.Jobs;

using Quartz;


namespace QdrantOperator.Controllers
{
    /// <summary>
    /// QdrantSnapshotSchedule Controller
    /// </summary>
    [RbacRule<QdrantSnapshot>(Scope = EntityScope.Cluster, Verbs = RbacVerb.All)]
    [RbacRule<QdrantSnapshotSchedule>(Scope = EntityScope.Cluster, Verbs = RbacVerb.All)]
    public class QdrantSnapshotScheduleController : ResourceControllerBase<QdrantSnapshotSchedule>
    {
        private readonly IKubernetes                        k8s;
        private readonly ILogger<QdrantSnapshotScheduleController>   logger;
        private ISchedulerFactory schedulerFactory;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="k8s"></param>
        /// <param name="logger"></param>
        /// <param name="schedulerFactory"></param>
        public QdrantSnapshotScheduleController(
            IKubernetes k8s,
            ILogger<QdrantSnapshotScheduleController> logger,
            ISchedulerFactory schedulerFactory)
        {
            this.k8s = k8s;
            this.logger = logger;
            this.schedulerFactory = schedulerFactory;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public override async Task<ResourceControllerResult> ReconcileAsync(QdrantSnapshotSchedule resource)
        {
            var scheduler = await schedulerFactory.GetScheduler();

            if (!scheduler.IsStarted)
            {
                await scheduler.Start();
            }

            var jobKey = new JobKey(resource.Uid(), QdrantSnapshotSchedule.KubeKind);

            IJobDetail job = JobBuilder.Create<CreateSnapshotJob>()
              .WithIdentity(jobKey)
               .Build();

            job.JobDataMap.Put("Kubernetes", k8s);
            job.JobDataMap.Put(nameof(QdrantSnapshotSchedule), resource);

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(resource.Uid(), QdrantSnapshotSchedule.KubeKind)
                .StartNow()
                .WithCronSchedule(resource.Spec.Schedule)
                .Build();

            try
            {
                await scheduler.DeleteJob(jobKey);
            }
            catch (NullReferenceException)
            {
                // doesn't exist
            }

            await scheduler.ScheduleJob(job, trigger);

            return ResourceControllerResult.Ok();
        }

    }
}
