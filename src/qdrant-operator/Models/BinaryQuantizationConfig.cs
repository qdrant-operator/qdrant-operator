namespace QdrantOperator.Models
{
    public class BinaryQuantizationConfig : QuantizationConfigBase
    {
        public Qdrant.Client.Grpc.BinaryQuantization ToGrpc()
        {
            var result = new Qdrant.Client.Grpc.BinaryQuantization();

            if (base.AlwaysRam.HasValue)
            {
                result.AlwaysRam = base.AlwaysRam.Value;
            }

            return result;
        }
    }
}
