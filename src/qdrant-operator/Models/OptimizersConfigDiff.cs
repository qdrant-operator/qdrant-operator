using Neon.Operator.Attributes;
using System.ComponentModel;

namespace QdrantOperator.Models
{
    public class OptimizersConfigDiff
    {
        [DefaultValue(null)]
        public double? DeletedThreshold { get; set; } = null;

        [DefaultValue(null)]
        [Range(Minimum = 0, ExclusiveMinimum =false)]
        public int? VacuumMinVectorNumber { get; set; } = null;

        [DefaultValue(null)]
        [Range(Minimum = 0, ExclusiveMinimum = false)]
        public int? DefaultSegmentNumber { get; set; } = null;

        [DefaultValue(null)]
        [Range(Minimum = 0, ExclusiveMinimum = false)]
        public int? MaxSegmentSize { get; set; } = null;

        [DefaultValue(null)]
        [Range(Minimum = 0, ExclusiveMinimum = false)]
        public int? MemmapThreshold { get; set; } = null;

        [DefaultValue(null)]
        [Range(Minimum = 0, ExclusiveMinimum = false)]
        public int? IndexingThreshold { get; set; } = null;

        [DefaultValue(null)]
        [Range(Minimum = 0, ExclusiveMinimum = false)]
        public int? FlushIntervalSec { get; set; } = null;

        [DefaultValue(null)]
        [Range(Minimum = 0, ExclusiveMinimum = false)]
        public int? MaxOptimizationThreads { get; set; } = null;
    }
}
