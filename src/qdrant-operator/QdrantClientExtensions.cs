using System;
using System.Threading;
using System.Threading.Tasks;

using Grpc.Core;

using Qdrant.Client;

namespace QdrantOperator
{
    /// <summary>
    /// Qdrant client extensions.
    /// </summary>
    public static class QdrantClientExtensions
    {
        /// <summary>
        /// Verifies that a collection exists.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="collectionName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<bool> CollectionExistsAsync(this QdrantClient client, string collectionName, CancellationToken cancellationToken = default)
        {
            try
            {
                var info = await client.GetCollectionInfoAsync(collectionName, cancellationToken);
                return true;
            }
            catch (RpcException rpc)
            {
                if (rpc.StatusCode == StatusCode.NotFound)
                {
                    return false;
                }

                throw;
            }
        }
    }
}
