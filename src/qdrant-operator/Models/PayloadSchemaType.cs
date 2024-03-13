using System.Runtime.Serialization;

namespace QdrantOperator.Models
{
    /// <summary>
    /// Enumerates payload schema types.
    /// </summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumMemberConverter))]
    public enum PayloadSchemaType
    {
        /// <summary>
        /// Keyword
        /// </summary>
        
        [EnumMember(Value = "keyword")]
        Keyword = 0,

        /// <summary>
        /// Integer
        /// </summary>
        [EnumMember(Value = "integer")]
        Integer,

        /// <summary>
        /// Float
        /// </summary>
        [EnumMember(Value = "float")]
        Float,

        /// <summary>
        /// Geo
        /// </summary>
        [EnumMember(Value = "geo")]
        Geo,

        /// <summary>
        /// Text
        /// </summary>
        [EnumMember(Value = "text")]
        Text,

        /// <summary>
        /// Bool
        /// </summary>
        [EnumMember(Value = "bool")]
        Bool,
    }
}
