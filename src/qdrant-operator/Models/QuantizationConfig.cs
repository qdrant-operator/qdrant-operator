namespace QdrantOperator.Models
{
    public class QuantizationConfig
    {
        public ScalarQuantizationConfig ScalarQuantizationConfig { get; set; } = new ScalarQuantizationConfig();
        public ProductQuantizationConfig ProductQuantization { get; set; } = new ProductQuantizationConfig();

        public BinaryQuantizationConfig BinaryQuantization { get; set; } = new BinaryQuantizationConfig();

        public Qdrant.Client.Grpc.QuantizationConfig ToGrpc()
        {
            return new Qdrant.Client.Grpc.QuantizationConfig()
            {
                Binary = BinaryQuantization?.ToGrpc(),
                Product = ProductQuantization?.ToGrpc(),
                Scalar = ScalarQuantizationConfig?.ToGrpc()
            };
        }

        public Qdrant.Client.Grpc.QuantizationConfigDiff ToGrpc(QuantizationConfig other)
        {
            var result = new Qdrant.Client.Grpc.QuantizationConfigDiff();

            if (ScalarQuantizationConfig != other?.ScalarQuantizationConfig)
            {
                result.Scalar = ScalarQuantizationConfig?.ToGrpc();
            }

            if (ProductQuantization != other?.ProductQuantization)
            {
                result.Product = ProductQuantization?.ToGrpc();
            }

            if (BinaryQuantization != other?.BinaryQuantization)
            {
                result.Binary = BinaryQuantization?.ToGrpc();
            }

            return result;
        }
    }
}
