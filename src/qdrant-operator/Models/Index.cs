using System.ComponentModel;

using Neon.Operator.Attributes;

using Qdrant.Client.Grpc;

namespace QdrantOperator.Models
{
    /// <summary>
    /// Represents an Index.
    /// </summary>
    public class Index
    {
        /// <summary>
        /// Minimal size (in kilobytes) of vectors for additional payload-based indexing.
        /// If payload chunk is smaller than full_scan_threshold_kb additional indexing won't
        /// be used - in this case full-scan search should be preferred by query planner and
        /// additional indexing is not required. Note: 1Kb = 1 vector of size 256
        /// </summary>
        [DefaultValue(null)]
        [Range(Minimum = 0, ExclusiveMinimum = false)]
        public int? FullScanThreshold { get; set; } = null;

        /// <summary>
        /// Store the index on disk. If set to false, the index will be stored in RAM. Defaults to false.
        /// </summary>
        [DefaultValue(false)]
        public bool? OnDisk { get; set; } = false;

        /// <summary>
        /// Converts to GRPC type.
        /// </summary>
        /// <returns></returns>
        public Qdrant.Client.Grpc.SparseIndexConfig ToGrpc()
        {
            var result = new SparseIndexConfig();

            if (FullScanThreshold.HasValue)
            {
                result.FullScanThreshold = (ulong)FullScanThreshold.Value;
            }

            if (OnDisk.HasValue)
            {
                result.OnDisk = OnDisk.Value;
            }

            return result;
        }
    }
}
