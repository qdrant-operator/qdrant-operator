using Neon.Operator.Attributes;
using System.ComponentModel;

namespace QdrantOperator.Models
{
    public class ScalarQuantizationConfig : QuantizationConfigBase
    {
        [DefaultValue("int8")]
        public string Type { get; set; } = "int8";

        [Range(Minimum = 0.5, ExclusiveMinimum = false, Maximum = 1.0, ExclusiveMaximum = false)]
        [DefaultValue(null)]
        public float? Quantile { get; set; } = null;
    }
}
