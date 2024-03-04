using System.ComponentModel;

using Neon.Operator.Attributes;

namespace QdrantOperator.Models
{
    /// <summary>
    /// Custom params for WAL. If none - values from service configuration file are used.
    /// </summary>
    public class WalConfigDiff
    {
        /// <summary>
        /// Size of a single WAL segment in MB
        /// </summary>
        [DefaultValue(null)]
        [Range(Minimum =1,ExclusiveMinimum =false)]
        public int? WalCapacityMb {  get; set; } = null;

        /// <summary>
        /// Number of WAL segments to create ahead of actually used ones
        /// </summary>
        [DefaultValue(null)]
        [Range(Minimum = 0, ExclusiveMinimum = false)]
        public int? WalSegmentsAhead { get; set; } = null;

        /// <summary>
        /// Converts to GRPC type.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public Qdrant.Client.Grpc.WalConfigDiff ToGrpc(WalConfigDiff other = null)
        {
            var result = new Qdrant.Client.Grpc.WalConfigDiff();

            if (WalCapacityMb.HasValue && 
                WalCapacityMb != other?.WalCapacityMb)
            {
                result.WalCapacityMb = (ulong)WalCapacityMb.Value;
            }

            if (WalSegmentsAhead.HasValue &&
                WalSegmentsAhead != other?.WalSegmentsAhead)
            {
                result.WalSegmentsAhead = (ulong)WalSegmentsAhead.Value;
            }

            return result;
        }
    }
}
