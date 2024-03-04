using System.Collections.Generic;
using System.Text.Json.Serialization;

using QdrantOperator.Models.Converters;

namespace QdrantOperator.Models
{
    /// <summary>
    /// Represents vector parameters.
    /// </summary>
    [JsonConverter(typeof(VectorSpecConverter))]
    public class VectorSpec : VectorSpecBase
    {
        /// <summary>
        /// A map of named vectors.
        /// </summary>
        public List<NamedVectorSpec> NamedVectors { get; set; }

        /// <summary>
        /// Converts the vector spec to a <see cref="Qdrant.Client.Grpc.VectorParamsDiff"/>.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
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
