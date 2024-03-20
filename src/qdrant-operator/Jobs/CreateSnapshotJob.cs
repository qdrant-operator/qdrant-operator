using System.Threading.Tasks;

using k8s;
using k8s.Autorest;
using k8s.Models;

using Quartz;

using QdrantOperator.Models;
using Neon.Common;
using Neon.Operator;
using Neon.K8s;

namespace QdrantOperator.Jobs
{

    /// <summary>
    /// Represents a job that creates a snapshot.
    /// </summary>
    public class CreateSnapshotJob : IJob
    {
        /// <summary>
        /// Executes the job to create a snapshot.
        /// </summary>
        /// <param name="context">The job execution context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Execute(IJobExecutionContext context)
        {
            var dataMap = context.MergedJobDataMap;

            var k8s = (IKubernetes)dataMap["Kubernetes"];

            var resource = (QdrantSnapshotSchedule)dataMap[nameof(QdrantSnapshotSchedule)];

            var snapshot = new QdrantSnapshot().Initialize();

            var snapshotName = $"{resource.Metadata.Name}-{NeonHelper.CreateBase36Uuid()}";
            snapshot.Metadata.Name = snapshotName;
            snapshot.Metadata.NamespaceProperty = resource.Metadata.NamespaceProperty;

            snapshot.Spec = resource.Spec.Snapshot;
            snapshot.AddOwnerReference(resource.MakeOwnerReference());
            await k8s.CustomObjects.UpsertNamespacedCustomObjectAsync<QdrantSnapshot>(
                body: snapshot,
                name: snapshot.Metadata.Name,
                namespaceParameter: snapshot.Metadata.NamespaceProperty);

            context.Result = snapshotName;
        }
    }
}
