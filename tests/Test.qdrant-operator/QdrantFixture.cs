using System.Threading.Tasks;

using Neon.Xunit;

using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace QdrantOperator.Xunit
{
    /// <summary>
    /// Qdrant cluster fixture.
    /// </summary>
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

        /// <summary>
        /// Start method.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <param name="hostInterface"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Starts the Qdrant container as a composed service.
        /// </summary>
        /// <param name="image">The Docker image to use. If null, the latest version of the qdrant image will be used.</param>
        /// <param name="name">The name of the container. Default is "qdrant-test".</param>
        /// <param name="args">Additional arguments to pass to the Docker container.</param>
        /// <param name="hostInterface">The host interface to bind the container ports to. If null, the default interface will be used.</param>
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

        /// <summary>
        /// Deletes all collections in the Qdrant server.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ClearCollectionsAsync()
        {
            foreach (var collection in await QdrantClient.ListCollectionsAsync())
            {
                await QdrantClient.DeleteCollectionAsync(collection);
            }
        }

        /// <summary>
        /// Restarts the Qdrant container to clear any previous state and returns the new client connection.
        /// </summary>
        /// <returns>The new Qdrant client connection.</returns>
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
        /// Resets the Qdrant fixture by removing and recreating the Qdrant container.
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