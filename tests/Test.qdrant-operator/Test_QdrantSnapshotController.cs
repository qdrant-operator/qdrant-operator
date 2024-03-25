using System.Threading.Tasks;

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
            fixture.RegisterType<QdrantSnapshot>();
            fixture.RegisterType<V1Pod>();
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
            snapshot.Status = new SnapshotStatus();

            await qdrantFixture.QdrantClient.CreateCollectionAsync(snapshot.Spec.Collection);
            fixture.AddResource(snapshot);
            var cluster = new V1QdrantCluster().Initialize();
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
        }

        [Fact]
        public async Task TestCreateUploadJob()
        {
            fixture.ClearResources();

            var controller = fixture.Operator.GetController<QdrantSnapshotController>();
            // create QdrantSnapshot
            var snapshot = new QdrantSnapshot().Initialize();
            fixture.AddResource(snapshot);

            // create pod and make sure it has volumes attached
            var pod = new V1Pod().Initialize();
            fixture.AddResource(pod);


        }
    }
}
