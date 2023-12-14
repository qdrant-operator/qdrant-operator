using Neon.Operator.Attributes;
using System.ComponentModel;

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
    }
}
