namespace QdrantOperator.Models
{
    /// <summary>
    /// The sparse vector parameters.
    /// </summary>
    public class SparseVectorParams
    {
        /// <summary>
        /// The index parameters.
        /// </summary>
        public Index Index { get; set; }

        /// <summary>
        /// Converts to GRPC type.
        /// </summary>
        /// <returns></returns>
        public Qdrant.Client.Grpc.SparseVectorParams ToGrpc()
        {
            var result = new Qdrant.Client.Grpc.SparseVectorParams();

            if (Index != null)
            {
                result.Index = Index?.ToGrpc();
            }

            return result;
        }
    }
}
