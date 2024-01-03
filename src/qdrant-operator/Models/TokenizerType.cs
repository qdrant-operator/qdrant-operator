using System.Runtime.Serialization;

namespace QdrantOperator.Models
{
    public enum TokenizerType
    {
        [EnumMember(Value = "prefix")]
        Prefix = 0,
        [EnumMember(Value = "whitespace")]
        WhiteSpace,
        [EnumMember(Value = "word")]
        Word,
        [EnumMember(Value = "multilingual")]
        Multilingual,
    }
}
