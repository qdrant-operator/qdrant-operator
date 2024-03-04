namespace QdrantOperator.Models
{
    /// <summary>
    /// Represents quantization parameters.
    /// </summary>
    public class QuantizationConfig
    {
        /// <summary>
        /// The scalar quantization parameters
        /// </summary>
        public ScalarQuantizationConfig ScalarQuantizationConfig { get; set; } = new ScalarQuantizationConfig();

        /// <summary>
        /// The product quantization parameters.
        /// </summary>
        public ProductQuantizationConfig ProductQuantization { get; set; } = new ProductQuantizationConfig();

        /// <summary>
        /// The binary quantizxation parameters.
        /// </summary>
        public BinaryQuantizationConfig BinaryQuantization { get; set; } = new BinaryQuantizationConfig();

        /// <summary>
        /// Converts to GRPC type.
        /// </summary>
        /// <returns></returns>
        public Qdrant.Client.Grpc.QuantizationConfig ToGrpc()
        {
            return new Qdrant.Client.Grpc.QuantizationConfig()
            {
                Binary = BinaryQuantization?.ToGrpc(),
                Product = ProductQuantization?.ToGrpc(),
                Scalar = ScalarQuantizationConfig?.ToGrpc()
            };
        }

        /// <summary>
        /// Converts to GRPC type.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
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
