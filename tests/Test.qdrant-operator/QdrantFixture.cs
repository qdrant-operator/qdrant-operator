using Neon.Xunit;

using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace QdrantOperator.Xunit
{
    public sealed class QdrantFixture : ContainerFixture
    {
        /// <summary>
        /// Returns the URI for a Qdrant server running locally (probably as a Docker container).
        /// </summary>
        public const string ConnectionUri = "http://localhost:6334";

        /// <summary>
        /// Constructs the fixture.
        /// </summary>
        public QdrantFixture()
        {
        }

        /// <summary>
        /// Returns the Qdrant connection.
        /// </summary>
        public QdrantClient QdrantClient { get; private set; }


        public TestFixtureStatus Start(
            string image = null,
            string name = "qdrant-test",
            string[] args = null,
            string hostInterface = null)
        {
            base.CheckDisposed();

            return base.Start(
                () =>
                {
                    StartAsComposed(image, name, args, hostInterface);
                });
        }


        public void StartAsComposed(
            string image = null,
            string name = "qdrant-test",
            string[] args = null,
            string hostInterface = null)
        {
            image = image ?? $"qdrant/qdrant:latest";

            base.CheckWithinAction();

            var dockerArgs =
                new string[]
                {
                    "-p", $"{GetHostInterface(hostInterface)}:6333:6333",
                    "-p", $"{GetHostInterface(hostInterface)}:6334:6334"
                };

            if (!IsRunning)
            {
                StartAsComposed(name, image, dockerArgs, args);
            }

            var channel = QdrantChannel.ForAddress(ConnectionUri);
            var grpcClient = new QdrantGrpcClient(channel);

            QdrantClient = new QdrantClient(grpcClient);
        }

        public async Task ClearCollectionsAsync()
        {
            foreach (var collection in await QdrantClient.ListCollectionsAsync())
            {
                await QdrantClient.DeleteCollectionAsync(collection);
            }
        }

        /// <summary>
        /// Restarts the Qdrant container to clear any previous state and returns the 
        /// new client connection.
        /// </summary>
        /// <returns>The new connection.</returns>
        public new QdrantClient Restart()
        {
            base.Restart();

            if (QdrantClient != null)
            {
                QdrantClient.Dispose();
                QdrantClient = null;
            }

            var channel = QdrantChannel.ForAddress(ConnectionUri);
            var grpcClient = new QdrantGrpcClient(channel);

            QdrantClient = new QdrantClient(grpcClient);

            return QdrantClient;
        }

        /// <summary>
        /// This method completely resets the fixture by removing and recreating
        /// the Qdrant container.
        /// </summary>
        public override void Reset()
        {
            if (QdrantClient != null)
            {
                QdrantClient.Dispose();
                QdrantClient = null;
            }

            base.Reset();
        }
    }
}