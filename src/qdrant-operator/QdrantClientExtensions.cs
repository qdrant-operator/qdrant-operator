using System;
using System.Threading;
using System.Threading.Tasks;

using Grpc.Core;

using Qdrant.Client;

namespace QdrantOperator
{
    public static class QdrantClientExtensions
    {
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
