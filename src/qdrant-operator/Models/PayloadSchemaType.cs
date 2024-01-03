using System.Runtime.Serialization;

namespace QdrantOperator.Models
{
    public enum PayloadSchemaType
    {
        
        [EnumMember(Value = "keyword")]
        Keyword = 0,
        [EnumMember(Value = "integer")]
        Integer,
        [EnumMember(Value = "float")]
        Float,
        [EnumMember(Value = "geo")]
        Geo,
        [EnumMember(Value = "text")]
        Text,
        [EnumMember(Value = "bool")]
        Bool,
    }
}
