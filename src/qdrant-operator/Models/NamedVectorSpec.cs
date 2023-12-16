using System.Collections.Generic;
using System.ComponentModel;

namespace QdrantOperator.Models
{
    public class NamedVectorSpec : VectorSpecBase
    {
        public Qdrant.Client.Grpc.VectorParamsDiff ToGrpcDiff(NamedVectorSpec other)
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
