using System.Runtime.Serialization;

namespace QdrantOperator.Models
{
    public enum ShardingMethod
    {
        [EnumMember(Value = "auto")]
        Auto = 0,
        [EnumMember(Value = "custom")]
        Custom,
       
    }
}
