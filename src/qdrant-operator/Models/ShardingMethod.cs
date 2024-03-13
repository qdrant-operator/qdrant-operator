using System.Runtime.Serialization;

namespace QdrantOperator.Models
{
    /// <summary>
    /// Enumerates sharding methods.
    /// </summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumMemberConverter))]
    public enum ShardingMethod
    {
        /// <summary>
        /// Auto
        /// </summary>
        [EnumMember(Value = "auto")]
        Auto = 0,

        /// <summary>
        /// Custom
        /// </summary>
        [EnumMember(Value = "custom")]
        Custom,
       
    }
}
