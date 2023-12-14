using System.ComponentModel;
using System.Text.Json.Serialization;

namespace QdrantOperator.Models
{
    public class VectorSpec
    {
        [DefaultValue(1)]
        public int Size { get; set; } = 1;

        [DefaultValue(DistanceFunction.Cosine)]
        public DistanceFunction Distance { get; set; } = DistanceFunction.Cosine;
        public HnswConfig HnswConfig { get; set; }

        public QuantizationConfig QuantizationConfig { get; set; }

        [DefaultValue(false)]
        public bool? OnDisk { get; set; } = false;
    }
}
