using System.Collections.Generic;
using System.ComponentModel;

namespace QdrantOperator.Models
{
    public class VectorSpec : VectorSpecBase
    {
        public List<NamedVectorSpec> NamedVectors { get; set; }

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
