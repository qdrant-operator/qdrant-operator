using System.ComponentModel.DataAnnotations;

using QdrantOperator.Extensions;

namespace QdrantOperator.Models
{
    /// <summary>
    /// Specifies Product quantization parameters.
    /// </summary>
    public class ProductQuantizationConfig : QuantizationConfigBase
    {
        /// <summary>
        /// The compression ratio.
        /// </summary>
        [Required]
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumMemberConverter))]
        public CompressionRatio CompressionRatio { get; set; }

        /// <summary>
        /// Converts to GRPC type.
        /// </summary>
        /// <returns></returns>
        public Qdrant.Client.Grpc.ProductQuantization ToGrpc()
        {
            return new Qdrant.Client.Grpc.ProductQuantization()
            {
                Compression = CompressionRatio.ToGrpcCompressionRatio()
            };
        }
    }
}
