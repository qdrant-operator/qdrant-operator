using System.ComponentModel;

using Neon.Operator.Attributes;

using Qdrant.Client.Grpc;

namespace QdrantOperator.Models
{
    public class Index
    {
        [DefaultValue(null)]
        [Range(Minimum = 0, ExclusiveMinimum = false)]
        public int? FullScanThreshold { get; set; } = null;

        [DefaultValue(false)]
        public bool? OnDisk { get; set; } = false;

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
