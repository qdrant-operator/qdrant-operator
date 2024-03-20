using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace QdrantOperator.Models
{
    /// <summary>
    /// Represents a named vector.
    /// </summary>
    public class NamedVectorSpec : VectorSpecBase
    {
        /// <summary>
        /// The name of the vector.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Converts this instance to a <see cref="Qdrant.Client.Grpc.VectorParamsDiff"/>.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
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
