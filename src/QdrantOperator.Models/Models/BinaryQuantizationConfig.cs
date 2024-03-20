namespace QdrantOperator.Models
{
    /// <summary>
    /// Represents the binary quantization parameters.
    /// </summary>
    public class BinaryQuantizationConfig : QuantizationConfigBase
    {
        /// <summary>
        /// Converts to GRPC type.
        /// </summary>
        /// <returns></returns>
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
