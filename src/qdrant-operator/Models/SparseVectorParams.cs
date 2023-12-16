namespace QdrantOperator.Models
{
    public class SparseVectorParams
    {
        public Index Index { get; set; }

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
