using Neon.Operator.Attributes;
using System.ComponentModel;

namespace QdrantOperator.Models
{
    public class HnswConfig
    {
        [Range(Minimum = 0, ExclusiveMinimum = false)]
        [DefaultValue(null)]
        public int? M { get; set; } =null;

        [Range(Minimum = 4, ExclusiveMinimum = false)]
        [DefaultValue(null)]
        public int? EfConstruct { get; set; } = null;

        [Range(Minimum = 10, ExclusiveMinimum = false)]
        [DefaultValue(null)]
        public int? FullScanThreshold { get; set; } = null;

        [Range(Minimum = 0, ExclusiveMinimum = false)]
        [DefaultValue(null)]
        public int? MaxIndexingThreads { get; set; } = null;

        [DefaultValue(false)]
        public bool? OnDisk { get; set; } = false;

        [Range(Minimum = 0, ExclusiveMinimum = false)]
        [DefaultValue(null)]
        public int? PayloadM { get; set; } = null;


    }
}
