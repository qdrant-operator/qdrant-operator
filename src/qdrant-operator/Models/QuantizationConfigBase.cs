using System.ComponentModel;

namespace QdrantOperator.Models
{
    public class QuantizationConfigBase
    {
        [DefaultValue(false)]
        public bool? AlwaysRam { get; set; } = false;
    }
}
