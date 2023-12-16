using System.ComponentModel;

using Neon.Operator.Attributes;

namespace QdrantOperator.Models
{
    public class WalConfigDiff
    {
        [DefaultValue(null)]
        [Range(Minimum =1,ExclusiveMinimum =false)]
        public int? WalCapacityMb {  get; set; } = null;

        [DefaultValue(null)]
        [Range(Minimum = 0, ExclusiveMinimum = false)]
        public int? WalSegmentsAhead { get; set; } = null;

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
