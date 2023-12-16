using System.Collections.Generic;
using System.ComponentModel;

namespace QdrantOperator.Models
{
    public class VectorSpecBase
    {
        [DefaultValue(1)]
        public long Size { get; set; } = 1;

        [DefaultValue(DistanceFunction.Cosine)]
        public DistanceFunction Distance { get; set; } = DistanceFunction.Cosine;
        public HnswConfig HnswConfig { get; set; }

        public QuantizationConfig QuantizationConfig { get; set; }

        [DefaultValue(false)]
        public bool? OnDisk { get; set; } = false;
    }
}
