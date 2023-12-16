using System.ComponentModel;

using Neon.Operator.Attributes;

namespace QdrantOperator.Models
{
    public class OptimizersConfigDiff
    {
        [DefaultValue(null)]
        public double? DeletedThreshold { get; set; } = null;

        [DefaultValue(null)]
        [Range(Minimum = 0, ExclusiveMinimum = false)]
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

        public Qdrant.Client.Grpc.OptimizersConfigDiff ToGrpc(OptimizersConfigDiff other = null)
        {
            var result = new Qdrant.Client.Grpc.OptimizersConfigDiff();

            if (DeletedThreshold.HasValue
                && DeletedThreshold != other?.DeletedThreshold)
            {
                result.DeletedThreshold = DeletedThreshold.Value;
            }

            if (VacuumMinVectorNumber.HasValue
                && VacuumMinVectorNumber != other?.VacuumMinVectorNumber)
            {
                result.VacuumMinVectorNumber = (ulong)VacuumMinVectorNumber.Value;
            }

            if (DefaultSegmentNumber.HasValue
                && DefaultSegmentNumber != other?.DefaultSegmentNumber)
            {
                result.DefaultSegmentNumber = (ulong)DefaultSegmentNumber.Value;
            }

            if (MaxSegmentSize.HasValue
                && MaxSegmentSize != other?.MaxSegmentSize)
            {
                result.MaxSegmentSize = (ulong)MaxSegmentSize.Value;
            }

            if (MemmapThreshold.HasValue
                && MemmapThreshold != other?.MemmapThreshold)
            {
                result.MemmapThreshold = (ulong)MemmapThreshold.Value;
            }

            if (IndexingThreshold.HasValue
                && IndexingThreshold != other?.IndexingThreshold)
            {
                result.IndexingThreshold = (ulong)IndexingThreshold.Value;
            }

            if (FlushIntervalSec.HasValue
                && FlushIntervalSec != other?.FlushIntervalSec)
            {
                result.FlushIntervalSec = (ulong)FlushIntervalSec.Value;
            }

            if (MaxOptimizationThreads.HasValue                                                                                                      && MaxOptimizationThreads != other?.MaxOptimizationThreads)
            {
                result.MaxOptimizationThreads = (ulong)MaxOptimizationThreads.Value;
            }

            return result;
        }
    }
}
