using System.Runtime.Serialization;

namespace QdrantOperator.Models
{
    public enum DistanceFunction
    {
        [EnumMember(Value = "Cosine")]
        Cosine = 0,
        [EnumMember(Value = "Euclid")]
        Euclid,
        [EnumMember(Value = "Dot")]
        Dot,
        [EnumMember(Value = "Manhattan")]
        Manhattan
    }
}
