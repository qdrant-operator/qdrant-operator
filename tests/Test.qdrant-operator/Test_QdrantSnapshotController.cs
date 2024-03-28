using System.Threading.Tasks;
using System.Collections.Generic;


using AsyncKeyedLock;

using k8s;
using k8s.Models;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using Neon.Operator.Xunit;

using QdrantOperator;
using QdrantOperator.Controllers;
using QdrantOperator.Util;
using QdrantOperator.Xunit;
using QdrantOperator.Models;
using FluentAssertions;
using System.Linq;
using Qdrant.Client.Grpc;

namespace Test.QdrantOperator
{
    public class Test_QdrantSnapshotController : IClassFixture<TestOperatorFixture>, IClassFixture<QdrantFixture>
    {
        private TestOperatorFixture fixture;
        private QdrantFixture qdrantFixture;
        private Mock<IClusterHelper> mockClusterHelper;

        public Test_QdrantSnapshotController(TestOperatorFixture fixture,
            QdrantFixture qdrantFixture)
        {
            this.fixture = fixture;
            this.qdrantFixture = qdrantFixture;

            mockClusterHelper = new Mock<IClusterHelper>();

            fixture.Services.AddSingleton<AsyncKeyedLocker<string>>();
            fixture.Services.AddSingleton<IClusterHelper>(mockClusterHelper.Object);

            fixture.Operator.AddController<QdrantSnapshotController>();
            fixture.RegisterType<V1QdrantCluster>();
            fixture.RegisterType<QdrantSnapshot>();
            fixture.RegisterType<V1Pod>();
            fixture.RegisterType<V1Job>();
            fixture.Start();
            qdrantFixture.Start();
        }

        [Fact]
        public async Task TestCreateSnapshot()
        {
            fixture.ClearResources();

            var controller = fixture.Operator.GetController<QdrantSnapshotController>();
            // create QdrantSnapshot
            var snapshot = new QdrantSnapshot().Initialize();
            snapshot.Metadata.Name = "test-name";
            snapshot.Metadata.NamespaceProperty = "test-namespace";
            snapshot.Spec = new SnapshotSpec();
            snapshot.Spec.Cluster = "test-cluster";
            snapshot.Spec.Collection = "test-collection";

            await qdrantFixture.QdrantClient.CreateCollectionAsync(snapshot.Spec.Collection);
            fixture.AddResource(snapshot);
            var cluster = new V1QdrantCluster().Initialize();
            cluster.Metadata.Name = "test-cluster";
            cluster.Metadata.NamespaceProperty = "test-namespace";
            fixture.AddResource(cluster);
            var nodeName = "test-node";

            mockClusterHelper.Setup(helper => helper.CreateQdrantClientAsync(
                It.IsAny<V1QdrantCluster>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ReturnsAsync(qdrantFixture.QdrantClient);

            await controller.CreateSnapshotInternalAsync(cluster, snapshot, nodeName);

            // verify snapshot got created
            await qdrantFixture.QdrantClient.ListSnapshotsAsync(snapshot.Spec.Collection);
            // verify that the status was updated for test-node
            var snapshotStatus = snapshot.Status.Nodes[nodeName];
            Assert.NotNull(snapshotStatus);
        }

        [Fact]
        public async Task TestCreateUploadJob()
        {
            fixture.ClearResources();

            var controller = fixture.Operator.GetController<QdrantSnapshotController>();
            var nodeName = "test-node";
           

            // create QdrantSnapshot
            var snapshot = new QdrantSnapshot().Initialize();
            snapshot.Metadata.Name = "test-name";
            snapshot.Metadata.NamespaceProperty = "test-namespace";
            snapshot.Spec = new SnapshotSpec();
            snapshot.Spec.Cluster = "test-cluster";
            snapshot.Spec.Collection = "test-collection";
            snapshot.Spec.S3 = new S3Spec()
            {
                Bucket = "aws",
                Region = "test-region",
                AccessKey = new V1SecretKeySelector()
                {
                    Name = "test-secret",
                    Key = "access"
                },
                SecretAccessKey = new V1SecretKeySelector()
                {
                    Name = "test-secret",
                    Key = "secret"
                }
            };
            snapshot.Status = new SnapshotStatus();
            snapshot.Status.Nodes = new System.Collections.Generic.Dictionary<string, SnapshotNodeStatus>();
            snapshot.Status.Nodes.Add(nodeName, new SnapshotNodeStatus());

            fixture.AddResource(snapshot);

            // create pod and make sure it has volumes attached
            var pod = new V1Pod().Initialize();
            pod.Metadata.Name = nodeName;
            pod.Metadata.NamespaceProperty = snapshot.Metadata.NamespaceProperty;
            pod.Spec = new V1PodSpec();
            pod.Spec.NodeName = nodeName;
            pod.Spec.Volumes = new List<V1Volume>()
            {
                new V1Volume()
            {
                Name = Constants.QdrantSnapshots,
                PersistentVolumeClaim = new V1PersistentVolumeClaimVolumeSource()
                {
                    ClaimName = "test-claim"
                }
            }
            };
            pod.Spec.Containers = new List<V1Container>()
            {
                new V1Container()
                {
                    Name = Constants.QdrantContainerName,
                    VolumeMounts = new List<V1VolumeMount>()
                    {
                        new V1VolumeMount()
                        {
                            Name = Constants.QdrantSnapshots,
                            MountPath = "/mnt/data"
                        }
                    }
                 
                }
            };
            fixture.AddResource(pod);

            await controller.CreateJobInternalAsync(snapshot, nodeName, snapshot.Spec.S3.AccessKey, snapshot.Spec.S3.SecretAccessKey);

            var jobs = fixture.GetResources<V1Job>();

            jobs.Should().NotBeEmpty();

            var job = jobs.First();

            job.Spec.Template.Spec.Volumes.Should().NotBeEmpty();
            job.Spec.Template.Spec.Volumes.First().Name.Should().Be(Constants.QdrantSnapshots);
            job.Spec.Template.Spec.Volumes.First().PersistentVolumeClaim.ClaimName.Should().Be("test-claim");

            job.Spec.Template.Spec.Containers.Should().NotBeEmpty();

            var container = job.Spec.Template.Spec.Containers.First();
            container.VolumeMounts.Should().NotBeEmpty();
            container.VolumeMounts.First().Name.Should().Be(Constants.QdrantSnapshots);
            container.VolumeMounts.First().MountPath.Should().Be("/mnt/data");

            container.Env.Should().NotBeEmpty();
            container.Env.Count.Should().Be(8);
            container.Env
                .Where(e => e.Name == Constants.S3AccessKey
                    && e.ValueFrom.SecretKeyRef.Name == snapshot.Spec.S3.AccessKey.Name
                    && e.ValueFrom.SecretKeyRef.Key == snapshot.Spec.S3.AccessKey.Key)
                .Should()
                .ContainSingle();
        }
    }
}
