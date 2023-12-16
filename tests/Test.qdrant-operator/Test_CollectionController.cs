using System;
using System.Threading.Tasks;

using FluentAssertions;

using Grpc.Core;

using k8s;

using Neon.Operator.Xunit;

using QdrantOperator;
using QdrantOperator.Models;
using QdrantOperator.Xunit;

namespace Test_QdrantOperator
{
    [Collection("Test_CollectionController")]
    [CollectionDefinition("Test_CollectionController", DisableParallelization = true)]
    public class Test_CollectionController : IClassFixture<TestOperatorFixture>, IClassFixture<QdrantFixture>, IDisposable
    {
        private TestOperatorFixture operatorFixture;
        private QdrantFixture qdrantFixture;

        public Test_CollectionController(
            TestOperatorFixture operatorFixture,
            QdrantFixture qdrantFixture
            )
        {
            this.operatorFixture = operatorFixture;
            this.operatorFixture.Operator.AddController<QdrantCollectionController>();
            this.operatorFixture.Operator.AddFinalizer<QdrantCollectionFinalizer>();
            this.operatorFixture.Start();

            this.qdrantFixture = qdrantFixture;
            this.qdrantFixture.Start();
        }

        [Fact]
        public async Task CreateCollectionAsync()
        {
            await qdrantFixture.ClearCollectionsAsync();

            // create resource
            var resource = new V1QdrantCollection().Initialize();
            resource.Metadata.Name          = "test-collection";
            resource.Spec                   = new V1QdrantCollection.V1QdrantCollectionSpec();
            resource.Spec.Cluster           = "test-cluster";
            resource.Spec.ReplicationFactor = 1;
            resource.Spec.VectorSpec        = new VectorSpec()
            {
                Distance = DistanceFunction.Cosine,
                OnDisk = true,
                Size = 10
            };

            var controller = operatorFixture.Operator.GetController<QdrantCollectionController>();

            await controller.CreateCollectionAsync(qdrantFixture.QdrantClient, resource);

            var collection = await qdrantFixture.QdrantClient.GetCollectionInfoAsync(resource.Metadata.Name);


            collection.Should().NotBeNull();
            collection.Config.Params.ReplicationFactor.Should().Be(1);
        }

        [Fact]
        public async Task UpdateCollectionAsync()
        {
            await qdrantFixture.ClearCollectionsAsync();
           
            // create resource
            var resource = new V1QdrantCollection().Initialize();

            resource.Metadata.Name          = "test-collection";
            resource.Spec                   = new V1QdrantCollection.V1QdrantCollectionSpec();
            resource.Spec.Cluster           = "test-cluster";
            resource.Spec.ReplicationFactor = 1;
            resource.Spec.OnDiskPayload     = true;
            resource.Spec.VectorSpec        = new VectorSpec()
            {
                Distance = DistanceFunction.Cosine,
                OnDisk = true,
                Size = 10
            };

            var controller = operatorFixture.Operator.GetController<QdrantCollectionController>();

            await controller.CreateCollectionAsync(qdrantFixture.QdrantClient, resource);

            var collection = await qdrantFixture.QdrantClient.GetCollectionInfoAsync(resource.Metadata.Name);
            collection.Config.Params.OnDiskPayload.Should().BeTrue();

            operatorFixture.ClearResources();

            var specString = KubernetesJson.Serialize(resource.Spec);

            resource.Status             = new V1QdrantCollection.V1QdrantCollectionStatus();
            resource.Status.CurrentSpec = KubernetesJson.Deserialize<V1QdrantCollection.V1QdrantCollectionSpec>(specString);
            
            resource.Spec.OnDiskPayload = false;

            operatorFixture.AddResource(resource);

            await controller.CreateCollectionAsync(qdrantFixture.QdrantClient, resource);

            collection = await qdrantFixture.QdrantClient.GetCollectionInfoAsync(resource.Metadata.Name);

            collection.Should().NotBeNull();
            collection.Config.Params.OnDiskPayload.Should().BeFalse();
        }

        [Fact]
        public async Task FinalizeCollectionAsync()
        {
            await qdrantFixture.ClearCollectionsAsync();

            var resource = new V1QdrantCollection().Initialize();

            resource.Metadata.Name              = "test-collection";
            resource.Metadata.NamespaceProperty = "foo";
            resource.Spec                       = new V1QdrantCollection.V1QdrantCollectionSpec();
            resource.Spec.Cluster               = "test-cluster";
            resource.Spec.ReplicationFactor     = 1;
            resource.Spec.OnDiskPayload         = true;
            resource.Spec.VectorSpec            = new VectorSpec()
            {
                Distance = DistanceFunction.Cosine,
                OnDisk = true,
                Size = 10
            };

            var controller = operatorFixture.Operator.GetController<QdrantCollectionController>();

            await controller.CreateCollectionAsync(qdrantFixture.QdrantClient, resource);

            var collection = await qdrantFixture.QdrantClient.GetCollectionInfoAsync(resource.Metadata.Name);

            collection.Should().NotBeNull();

            var finalizer = operatorFixture.Operator.GetFinalizer<QdrantCollectionFinalizer>();

            await finalizer.FinalizeCollectionAsync(qdrantFixture.QdrantClient, resource);

            Func<Task> act = () => qdrantFixture.QdrantClient.GetCollectionInfoAsync(resource.Metadata.Name);
            await act.Should().ThrowAsync<RpcException>();
        }

        public void Dispose() { }
    }
}
