using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

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
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? OnDisk { get; set; } = false;
    }
}
