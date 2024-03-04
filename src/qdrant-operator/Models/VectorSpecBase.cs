using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace QdrantOperator.Models
{
    /// <summary>
    /// Common vector parameters.
    /// </summary>
    public class VectorSpecBase
    {
        /// <summary>
        /// Size of a vectors used.
        /// </summary>
        [DefaultValue(1)]
        public long Size { get; set; } = 1;

        /// <summary>
        /// Type of internal tags, build from payload Distance function types used to compare vectors
        /// </summary>
        [DefaultValue(DistanceFunction.Cosine)]
        public DistanceFunction Distance { get; set; } = DistanceFunction.Cosine;

        /// <summary>
        /// Custom params for HNSW index. If none - values from collection configuration are used.
        /// </summary>
        public HnswConfig HnswConfig { get; set; }

        /// <summary>
        /// Custom params for quantization. If none - values from collection configuration are used.c
        /// </summary>
        public QuantizationConfig QuantizationConfig { get; set; }

        /// <summary>
        /// If true, vectors are served from disk, improving RAM usage at the cost of latency Default: false
        /// </summary>
        [DefaultValue(false)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? OnDisk { get; set; } = false;
    }
}
