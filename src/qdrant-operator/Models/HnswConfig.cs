using System.ComponentModel;

using Neon.Operator.Attributes;

namespace QdrantOperator.Models
{
    public class HnswConfig
    {
        [Range(Minimum = 0, ExclusiveMinimum = false)]
        [DefaultValue(null)]
        public int? M { get; set; } = null;

        [Range(Minimum = 4, ExclusiveMinimum = false)]
        [DefaultValue(null)]
        public int? EfConstruct { get; set; } = null;

        [Range(Minimum = 10, ExclusiveMinimum = false)]
        [DefaultValue(null)]
        public int? FullScanThreshold { get; set; } = null;

        [Range(Minimum = 0, ExclusiveMinimum = false)]
        [DefaultValue(null)]
        public int? MaxIndexingThreads { get; set; } = null;

        [DefaultValue(false)]
        public bool? OnDisk { get; set; } = false;

        [Range(Minimum = 0, ExclusiveMinimum = false)]
        [DefaultValue(null)]
        public int? PayloadM { get; set; } = null;

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
