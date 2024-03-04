using System.Runtime.Serialization;

namespace QdrantOperator.Models
{
    /// <summary>
    /// Enumerates sharding methods.
    /// </summary>
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
