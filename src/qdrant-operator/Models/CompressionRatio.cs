using System.Runtime.Serialization;

namespace QdrantOperator.Models
{
    public enum CompressionRatio
    {
        [EnumMember(Value = "x4")]
        x4 = 0,
        [EnumMember(Value = "x8")]
        x8,
        [EnumMember(Value = "x16")]
        x16,
        [EnumMember(Value = "x32")]
        x32,
        [EnumMember(Value = "x64")]
        x64
    }
}
