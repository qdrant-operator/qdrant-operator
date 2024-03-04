using System.ComponentModel;

using Neon.Operator.Attributes;

namespace QdrantOperator.Models
{
    /// <summary>
    /// Represents scalar quantization parameters.
    /// </summary>
    public class ScalarQuantizationConfig : QuantizationConfigBase
    {
        /// <summary>
        /// The scalar type. This is set to int8.
        /// </summary>
        [DefaultValue("int8")]
        public string Type { get; set; } = "int8";

        /// <summary>
        /// Quantile for quantization. Expected value range in [0.5, 1.0].
        /// If not set - use the whole range of values
        /// </summary>
        [Range(Minimum = 0.5, ExclusiveMinimum = false, Maximum = 1.0, ExclusiveMaximum = false)]
        [DefaultValue(null)]
        public float? Quantile { get; set; } = null;

        /// <summary>
        /// Converts to GRPC type.
        /// </summary>
        /// <returns></returns>
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
