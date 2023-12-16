using System.Collections.Generic;
using System.ComponentModel;

namespace QdrantOperator.Models
{
    public class VectorSpec
    {
        [DefaultValue(1)]
        public long Size { get; set; } = 1;

        [DefaultValue(DistanceFunction.Cosine)]
        public DistanceFunction Distance { get; set; } = DistanceFunction.Cosine;
        public HnswConfig HnswConfig { get; set; }

        public QuantizationConfig QuantizationConfig { get; set; }

        [DefaultValue(false)]
        public bool? OnDisk { get; set; } = false;

        public Dictionary<string, VectorSpec> NamedVectors { get; set; }

        public Qdrant.Client.Grpc.VectorParamsDiff ToGrpcDiff(VectorSpec other)
        {
            var result = new Qdrant.Client.Grpc.VectorParamsDiff();

            result.HnswConfig = HnswConfig?.ToGrpc(other.HnswConfig);
            result.QuantizationConfig = QuantizationConfig?.ToGrpc(other.QuantizationConfig);

            if (OnDisk.HasValue 
                && OnDisk != other?.OnDisk)
            {
                result.OnDisk = OnDisk.Value;
            }

            return result;
        }
    }
}
