using System.Collections.Generic;
using System.Text.Json.Serialization;

using QdrantOperator.Models.Converters;

namespace QdrantOperator.Models
{
    [JsonConverter(typeof(VectorSpecConverter))]
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
