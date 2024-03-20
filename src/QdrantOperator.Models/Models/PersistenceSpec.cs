using System.ComponentModel;

namespace QdrantOperator.Models
{
    /// <summary>
    /// Represents the persistence options for the cluster.
    /// </summary>
    public class PersistenceSpec
    {
        /// <summary>
        /// The disk size for each cluster node.
        /// </summary>
        [DefaultValue("1Gi")]
        public string Size { get; set; } = "1Gi";

        /// <summary>
        /// The storage class to use when creating volumes.
        /// </summary>
        [DefaultValue(null)]
        public string StorageClassName { get; set; } = null;
    }
}