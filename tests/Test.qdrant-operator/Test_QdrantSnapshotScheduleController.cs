using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using k8s;
using k8s.Models;

using Microsoft.Extensions.DependencyInjection;

using Neon.Operator.Xunit;

using QdrantOperator;
using QdrantOperator.Controllers;
using QdrantOperator.Entities;
using QdrantOperator.Models;

using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;

namespace Test.QdrantOperator
{
    public class Test_QdrantSnapshotScheduleController : IClassFixture<TestOperatorFixture>
    {
        private TestOperatorFixture fixture;

        public Test_QdrantSnapshotScheduleController(TestOperatorFixture fixture)
        {
            this.fixture = fixture;
            fixture.Operator.AddController<QdrantSnapshotScheduleController>();
            ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
            fixture.Services.AddSingleton<ISchedulerFactory>(schedulerFactory);
            fixture.RegisterType<QdrantSnapshot>();
            fixture.Start();
        }

        [Fact]
        public async Task TestReconcileSnapshotSchedule()
        {
            fixture.ClearResources();

            var controller = fixture.Operator.GetController<QdrantSnapshotScheduleController>();

            var snapshotSchedule = new QdrantSnapshotSchedule()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "test",
                    NamespaceProperty = "test",
                    Uid = Guid.NewGuid().ToString()
                },
                Spec = new SnapshotScheduleSpec()
                {
                    Pause = true,
                    Schedule = "0 0 0 ? * * 2099",
                    Snapshot = new SnapshotSpec()
                    {
                        Cluster = "test",
                        Collection = "test-collection",
                        S3 = new S3Spec()
                        {
                            AccessKey = new V1SecretKeySelector()
                            {
                                Name = "test",
                                Key = "test-key",
                            },
                            SecretAccessKey = new V1SecretKeySelector()
                            {
                                Name = "test",
                                Key = "test-key",
                            },
                            Provider = "aws",
                            Region = "us-west-2",
                            Bucket = "test-bucket",
                            Prefix = "test-prefix",
                        }

                    }
                }
            };

            await controller.ReconcileAsync(snapshotSchedule);

            var schedulerFactory = fixture.Services.Where(s => s.ServiceType == typeof(ISchedulerFactory)).FirstOrDefault();
            var scheduler = await ((ISchedulerFactory)schedulerFactory.ImplementationInstance).GetScheduler();

            TestJobLIstener ourListener = new TestJobLIstener();

            scheduler.ListenerManager.AddJobListener(ourListener, GroupMatcher<JobKey>.GroupEquals(QdrantSnapshotSchedule.KubeKind));

            var jobs = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(QdrantSnapshotSchedule.KubeKind));

            // get the job detail
            var job = jobs.FirstOrDefault();
            await scheduler.TriggerJob(job);

            var finished = false;

            while (!finished)
            {
                finished = ourListener.HasFinished(job);

                if (!finished)
                {
                    await Task.Delay(100);
                }
            }

            var snapshots = fixture.Resources.OfType<QdrantSnapshot>().ToList();
        }
    }


    /// <summary>
    /// Test job listener class.
    /// </summary>
    public class TestJobLIstener : IJobListener
    {
        public string Name => nameof(TestJobLIstener);

        private List<string> startedJobs = new List<string>();
        private List<string> finishedJobs = new List<string>();

        /// <summary>
        /// Method called when a job execution is vetoed.
        /// </summary>
        public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Method called before a job is executed.
        /// </summary>
        public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            startedJobs.Add(context.JobDetail.Key.Name);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Method called after a job has been executed.
        /// </summary>
        public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = default)
        {
            finishedJobs.Add(context.JobDetail.Key.Name);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Check if a job has finished execution.
        /// </summary>
        public bool HasFinished(JobKey job)
        {
            return finishedJobs.Contains(job.Name);
        }
    }
}
