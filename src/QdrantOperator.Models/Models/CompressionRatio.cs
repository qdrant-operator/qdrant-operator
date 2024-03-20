using System.Runtime.Serialization;

namespace QdrantOperator.Models
{
    /// <summary>
    /// Enumerates compression ratios.
    /// </summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumMemberConverter))]
    public enum CompressionRatio
    {
        /// <summary>
        /// x4
        /// </summary>
        [EnumMember(Value = "x4")]
        x4 = 0,

        /// <summary>
        /// x8
        /// </summary>
        [EnumMember(Value = "x8")]
        x8,

        /// <summary>
        /// x16
        /// </summary>
        [EnumMember(Value = "x16")]
        x16,

        /// <summary>
        /// x32
        /// </summary>
        [EnumMember(Value = "x32")]
        x32,

        /// <summary>
        /// x64
        /// </summary>
        [EnumMember(Value = "x64")]
        x64
    }
}
