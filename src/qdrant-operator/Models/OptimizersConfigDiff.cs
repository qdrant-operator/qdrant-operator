using System.ComponentModel;

using Neon.Operator.Attributes;

namespace QdrantOperator.Models
{
    /// <summary>
    /// Custom params for Optimizers. If none - values from service configuration file are used.c
    /// </summary>
    public class OptimizersConfigDiff
    {
        /// <summary>
        /// The minimal fraction of deleted vectors in a segment, required to perform segment optimization
        /// </summary>
        [DefaultValue(null)]
        public double? DeletedThreshold { get; set; } = null;

        /// <summary>
        /// The minimal number of vectors in a segment, required to perform segment optimization
        /// </summary>
        [DefaultValue(null)]
        [Range(Minimum = 0, ExclusiveMinimum = false)]
        public int? VacuumMinVectorNumber { get; set; } = null;

        /// <summary>
        /// <para>
        /// Target amount of segments optimizer will try to keep. Real amount of segments may vary depending
        /// on multiple parameters: - Amount of stored points - Current write RPS
        /// </para>
        /// <para>
        /// It is recommended to select default number of segments as a factor of the number of search threads,
        /// so that each segment would be handled evenly by one of the threads If default_segment_number = 0,
        /// will be automatically selected by the number of available CPUs
        /// </para>
        /// </summary>
        [DefaultValue(null)]
        [Range(Minimum = 0, ExclusiveMinimum = false)]
        public int? DefaultSegmentNumber { get; set; } = null;

        /// <summary>
        /// <para>
        /// Do not create segments larger this size (in kilobytes). Large segments might require disproportionately
        /// long indexation times, therefore it makes sense to limit the size of segments.
        /// </para>
        /// <para>
        /// If indexation speed have more priority for your - make this parameter lower. If search speed is more
        /// important - make this parameter higher. Note: 1Kb = 1 vector of size 256
        /// </para>
        /// </summary>
        [DefaultValue(null)]
        [Range(Minimum = 0, ExclusiveMinimum = false)]
        public int? MaxSegmentSize { get; set; } = null;

        /// <summary>
        /// <para>
        /// Maximum size (in kilobytes) of vectors to store in-memory per segment. Segments larger than this threshold will
        /// be stored as read-only memmaped file.
        /// </para>
        /// <para>
        /// Memmap storage is disabled by default, to enable it, set this threshold to a reasonable value.
        /// </para>
        /// <para>
        /// To disable memmap storage, set this to 0.
        /// </para>
        /// <note>1Kb = 1 vector of size 256</note>
        /// </summary>
        [DefaultValue(null)]
        [Range(Minimum = 0, ExclusiveMinimum = false)]
        public int? MemmapThreshold { get; set; } = null;

        /// <summary>
        /// <para>
        /// Maximum size (in kilobytes) of vectors allowed for plain index, exceeding this threshold will enable vector
        /// indexing
        /// </para>
        /// <para>
        /// Default value is 20,000, based on https://github.com/google-research/google-research/blob/master/scann/docs/algorithms.md.
        /// </para>
        /// <para>
        /// To disable vector indexing, set to 0.
        /// </para>
        /// <note>1Kb = 1 vector of size 256</note>
        /// </summary>
        [DefaultValue(null)]
        [Range(Minimum = 0, ExclusiveMinimum = false)]
        public int? IndexingThreshold { get; set; } = null;

        /// <summary>
        /// Minimum interval between forced flushes.
        /// </summary>
        [DefaultValue(null)]
        [Range(Minimum = 0, ExclusiveMinimum = false)]
        public int? FlushIntervalSec { get; set; } = null;

        /// <summary>
        /// Maximum available threads for optimization workers.
        /// </summary>
        [DefaultValue(null)]
        [Range(Minimum = 0, ExclusiveMinimum = false)]
        public int? MaxOptimizationThreads { get; set; } = null;

        /// <summary>
        /// Converts to GRPC type.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
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
