using System.ComponentModel.DataAnnotations;

using QdrantOperator.Extensions;

namespace QdrantOperator.Models
{
    public class ProductQuantizationConfig : QuantizationConfigBase
    {
        [Required]
        public CompressionRatio CompressionRatio { get; set; }
      
        public Qdrant.Client.Grpc.ProductQuantization ToGrpc()
        {
            return new Qdrant.Client.Grpc.ProductQuantization()
            {
                Compression = CompressionRatio.ToGrpcCompressionRatio()
            };
        }
    }
}
