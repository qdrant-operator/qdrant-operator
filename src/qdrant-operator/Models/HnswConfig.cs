using System.ComponentModel;

using Neon.Operator.Attributes;

namespace QdrantOperator.Models
{
    /// <summary>
    /// Custom params for HNSW index. If none - values from service configuration file are used.
    /// </summary>
    public class HnswConfig
    {
        /// <summary>
        /// Number of edges per node in the index graph. Larger the value - more
        /// accurate the search, more space required.
        /// </summary>
        [Range(Minimum = 0, ExclusiveMinimum = false)]
        [DefaultValue(null)]
        public int? M { get; set; } = null;

        /// <summary>
        /// Number of neighbours to consider during the index building. Larger the
        /// value - more accurate the search, more time required to build the index.
        /// </summary>
        [Range(Minimum = 4, ExclusiveMinimum = false)]
        [DefaultValue(null)]
        public int? EfConstruct { get; set; } = null;

        /// <summary>
        /// Minimal size (in kilobytes) of vectors for additional payload-based indexing.
        /// If payload chunk is smaller than full_scan_threshold_kb additional indexing
        /// won't be used - in this case full-scan search should be preferred by query
        /// planner and additional indexing is not required. Note: 1Kb = 1 vector of size 256
        /// </summary>
        [Range(Minimum = 10, ExclusiveMinimum = false)]
        [DefaultValue(null)]
        public int? FullScanThreshold { get; set; } = null;

        /// <summary>
        /// Number of parallel threads used for background index building. If 0 - auto selection.
        /// </summary>
        [Range(Minimum = 0, ExclusiveMinimum = false)]
        [DefaultValue(null)]
        public int? MaxIndexingThreads { get; set; } = null;

        /// <summary>
        /// Store HNSW index on disk. If set to false, the index will be stored in RAM. Default: false
        /// </summary>
        [DefaultValue(false)]
        public bool? OnDisk { get; set; } = false;

        /// <summary>
        /// Custom M param for additional payload-aware HNSW links. If not set, default M will be used.
        /// </summary>
        [Range(Minimum = 0, ExclusiveMinimum = false)]
        [DefaultValue(null)]
        public int? PayloadM { get; set; } = null;

        /// <summary>
        /// Converts to GRPC type.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public Qdrant.Client.Grpc.HnswConfigDiff ToGrpc(HnswConfig other = null)
        {
            var result = new Qdrant.Client.Grpc.HnswConfigDiff();

            if (M.HasValue &&
                M != other?.M)
            {
                result.M = (ulong)M.Value;
            }

            if (EfConstruct.HasValue &&
                EfConstruct != other?.EfConstruct)
            {
                result.EfConstruct = (ulong)EfConstruct.Value;
            }

            if (FullScanThreshold.HasValue &&
                FullScanThreshold != other?.FullScanThreshold)
            {
                result.FullScanThreshold = (ulong)FullScanThreshold.Value;
            }

            if (MaxIndexingThreads.HasValue &&
                MaxIndexingThreads != other?.MaxIndexingThreads)
            {
                result.MaxIndexingThreads = (ulong)MaxIndexingThreads.Value;
            }

            if (OnDisk.HasValue &&
                OnDisk != other?.OnDisk)
            {
                result.OnDisk = OnDisk.Value;
            }

            if (PayloadM.HasValue &&
                PayloadM != other?.PayloadM)
            {
                result.PayloadM = (ulong)PayloadM.Value;
            }

            return result;
        }
    }
}
