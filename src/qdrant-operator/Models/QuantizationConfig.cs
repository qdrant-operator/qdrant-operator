using Qdrant.Client.Grpc;
using System.ComponentModel;

namespace QdrantOperator.Models
{
    public class QuantizationConfig
    {
        public ScalarQuantizationConfig ScalarQuantizationConfig { get; set; }
        public ProductQuantization ProductQuantization { get; set; }

        public BinaryQuantization BinaryQuantization { get; set; }


    }
}
