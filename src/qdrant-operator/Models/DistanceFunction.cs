using System.Runtime.Serialization;

namespace QdrantOperator.Models
{
    /// <summary>
    /// Enumerates the distance functions.
    /// </summary>
    public enum DistanceFunction
    {
        /// <summary>
        /// Cosine
        /// </summary>
        [EnumMember(Value = "Cosine")]
        Cosine = 0,

        /// <summary>
        /// Euclid
        /// </summary>
        [EnumMember(Value = "Euclid")]
        Euclid,

        /// <summary>
        /// Dot
        /// </summary>
        [EnumMember(Value = "Dot")]
        Dot,

        /// <summary>
        /// Manhattan
        /// </summary>
        [EnumMember(Value = "Manhattan")]
        Manhattan
    }
}
