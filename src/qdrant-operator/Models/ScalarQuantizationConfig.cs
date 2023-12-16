using System.ComponentModel;

using Neon.Operator.Attributes;

namespace QdrantOperator.Models
{
    public class ScalarQuantizationConfig : QuantizationConfigBase
    {
        [DefaultValue("int8")]
        public string Type { get; set; } = "int8";

        [Range(Minimum = 0.5, ExclusiveMinimum = false, Maximum = 1.0, ExclusiveMaximum = false)]
        [DefaultValue(null)]
        public float? Quantile { get; set; } = null;

        public Qdrant.Client.Grpc.ScalarQuantization ToGrpc()
        {
            var result = new Qdrant.Client.Grpc.ScalarQuantization();

            result.Type = Qdrant.Client.Grpc.QuantizationType.Int8;

            if (Quantile.HasValue)
            {
                result.Quantile = Quantile.Value;
            }

            return result;
        }

    }
}
