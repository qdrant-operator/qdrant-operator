using System;
using System.Threading.Tasks;

using FluentAssertions;

using Grpc.Core;

using k8s;

using Microsoft.Extensions.DependencyInjection;

using Neon.Operator.Xunit;

using OpenTelemetry.Resources;

using QdrantOperator;
using QdrantOperator.Models;
using QdrantOperator.Util;
using QdrantOperator.Xunit;

namespace Test_QdrantOperator
{
    [Collection("Test_CollectionFieldIndexController")]
    [CollectionDefinition("Test_CollectionFieldIndexController", DisableParallelization = true)]
    public class Test_CollectionFieldIndexController : IClassFixture<TestOperatorFixture>, IClassFixture<QdrantFixture>, IDisposable
    {
        private TestOperatorFixture operatorFixture;
        private QdrantFixture qdrantFixture;

        public Test_CollectionFieldIndexController(
            TestOperatorFixture operatorFixture,
            QdrantFixture qdrantFixture
            )
        {
            this.operatorFixture = operatorFixture;
            this.operatorFixture.Operator.AddController<QdrantCollectionFieldIndexController>();
            this.operatorFixture.Operator.AddFinalizer<QdrantCollectionFinalizer>();
            this.operatorFixture.Services.AddSingleton<ClusterHelper>();
            this.operatorFixture.Start();

            this.qdrantFixture = qdrantFixture;
            this.qdrantFixture.Start();
        }

        [Fact]
        public async Task CreateCollectionFieldIndexAsync()
        {
           
            await qdrantFixture.ClearCollectionsAsync();

            await qdrantFixture.QdrantClient.CreateCollectionAsync("test-collection", new Qdrant.Client.Grpc.VectorParams()
            {
                Size = 5,
                Distance = Qdrant.Client.Grpc.Distance.Cosine,
            });

            var controller = operatorFixture.Operator.GetController<QdrantCollectionFieldIndexController>();
            var resource = new V1QdrantCollectionFieldIndex().Initialize();

            resource.Metadata.Name = "test-collection-field-index";
            resource.Spec = new V1QdrantCollectionFieldIndex.V1QdrantCollectionFieldIndexSpec();
            resource.Spec.Cluster = "test-cluster";
            resource.Spec.Collection = "test-collection";
            resource.Spec.FieldName = "test-field";
            resource.Spec.TextIndexType = new TextIndexType()
            {
                Tokenizer = TokenizerType.Word,
                MinTokenLen = 1,
                MaxTokenLen = 10,
                Loweracase = true
            };
            resource.Spec.Type = PayloadSchemaType.Text;

            await controller.UpsertCollectionFieldIndexAsync(qdrantFixture.QdrantClient,resource);

            var collection = await qdrantFixture.QdrantClient.GetCollectionInfoAsync(resource.Spec.Collection);

            collection.PayloadSchema.Should().NotBeNull();



        }

        [Fact]
        public async Task UpdateCollectionFieldIndexAsync()
        {

            await qdrantFixture.ClearCollectionsAsync();

            await qdrantFixture.QdrantClient.CreateCollectionAsync("test-collection", new Qdrant.Client.Grpc.VectorParams()
            {
                Size = 5,
                Distance = Qdrant.Client.Grpc.Distance.Cosine,
            });

            var controller = operatorFixture.Operator.GetController<QdrantCollectionFieldIndexController>();
            var resource = new V1QdrantCollectionFieldIndex().Initialize();

            resource.Metadata.Name = "test-collection-field-index";
            resource.Spec = new V1QdrantCollectionFieldIndex.V1QdrantCollectionFieldIndexSpec();
            resource.Spec.Cluster = "test-cluster";
            resource.Spec.Collection = "test-collection";
            resource.Spec.FieldName = "test-field";
            resource.Spec.TextIndexType = new TextIndexType()
            {
                Tokenizer = TokenizerType.Word,
                MinTokenLen = 1,
                MaxTokenLen = 10,
                Loweracase = true
            };
            resource.Spec.Type = PayloadSchemaType.Text;
            await controller.UpsertCollectionFieldIndexAsync(qdrantFixture.QdrantClient, resource);
            var collection = await qdrantFixture.QdrantClient.GetCollectionInfoAsync(resource.Spec.Collection);

            collection.PayloadSchema.Should().NotBeNull();

            resource.Spec.TextIndexType = new TextIndexType()
            {
                Tokenizer = TokenizerType.Multilingual,
                MinTokenLen = 1,
                MaxTokenLen = 10,
                Loweracase = false
            };

            await controller.UpsertCollectionFieldIndexAsync(qdrantFixture.QdrantClient, resource);
            collection = await qdrantFixture.QdrantClient.GetCollectionInfoAsync(resource.Spec.Collection);

            collection.PayloadSchema.Should().NotBeNull();
            collection.PayloadSchema[resource.Spec.FieldName].Params.TextIndexParams.Lowercase.Should().BeFalse();
            collection.PayloadSchema[resource.Spec.FieldName].Params.TextIndexParams.Tokenizer.Should().Be(Qdrant.Client.Grpc.TokenizerType.Multilingual);


        }

        public void Dispose() { }
    }
}
