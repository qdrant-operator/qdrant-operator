using Neon.Operator.Attributes;
using System.ComponentModel;

namespace QdrantOperator.Models
{
    public class Index
    {
        [DefaultValue(null)]
        [Range(Minimum = 0, ExclusiveMinimum = false)]
        public int FullScanThreshold { get; set; }

        [DefaultValue(false)]
        public bool? OnDisk { get; set; } = false;
    }
}
