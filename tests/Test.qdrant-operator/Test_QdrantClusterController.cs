using k8s.Models;
using QdrantOperator;
using QdrantOperator.Models;
using Neon.Operator.Xunit;
using FluentAssertions;

namespace Test.qdrant_operator
{
    public class Test_QdrantClusterController : IClassFixture<TestOperatorFixture>
    {
        private TestOperatorFixture fixture;

        public Test_QdrantClusterController(TestOperatorFixture fixture)
        {
            this.fixture = fixture;
            fixture.Operator.AddController<QdrantClusterController>();
            fixture.RegisterType<V1ConfigMap>();
            fixture.RegisterType<V1Service>();
            fixture.RegisterType<V1ServiceAccount>();
            fixture.RegisterType<V1StatefulSet>();
            fixture.Start();
        }

        [Fact]
        public async Task TestReconcileCreatesStatefulset()
        {
            fixture.ClearResources();

            var controller = fixture.Operator.GetController<QdrantClusterController>();

            var qdrantCluster = new V1QdrantCluster()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "test",
                    NamespaceProperty = "test",
                },
                Spec = new V1QdrantCluster.V1QdrantClusterSpec()
                {
                    Image = new ImageSpec()
                    {
                        PullPolicy = "Always",
                        Repository = "test/image",
                        Tag = "not-latest"
                    },
                    Persistence = new PersistenceSpec()
                    {
                        Size = "9999Gi",
                        StorageClassName = "test-storage-class"
                    }
                }
            };
            await controller.ReconcileAsync(qdrantCluster);

            // verify result
            var statefulsets = fixture.Resources.OfType<V1StatefulSet>();
            var service = fixture.Resources.OfType<V1Service>();
            var serviceHeadless = fixture.Resources.OfType<V1Service>();
            var configMap = fixture.Resources.OfType<V1ConfigMap>();
            var serviceAccount = fixture.Resources.OfType<V1ServiceAccount>();

            statefulsets.Should().HaveCount(1);
            statefulsets.First().Metadata.Name.Should().Be(qdrantCluster.Metadata.Name);

            service.Should().HaveCount(1);
            service.First().Metadata.Name.Should().Be(qdrantCluster.Metadata.Name);

            serviceHeadless.Should().HaveCount(1);
            serviceHeadless.First().Metadata.Name.Should().Be(qdrantCluster.Metadata.Name);

            configMap.Should().HaveCount(1);
            configMap.First().Metadata.Name.Should().Be(qdrantCluster.Metadata.Name);

            serviceAccount.Should().HaveCount(1);
            serviceAccount.First().Metadata.Name.Should().Be(qdrantCluster.Metadata.Name);




        }
    }
}