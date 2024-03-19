namespace QdrantOperator.Models
{
    /// <summary>
    ///  Snapshot Restore Spec
    /// </summary>
    public class SnapshotRestoreSpec
    {
        /// <summary>
        /// Cluster Name
        /// </summary>
        public string Cluster { get; set; }

        /// <summary>
        /// Collection Name
        /// </summary>
        public string Collection { get; set;  }

        /// <summary>
        /// Snapshot Id
        /// </summary>
        public string SnapshotId { get; set; }

        /// <summary>
        /// S3 Spec
        /// </summary>
        public S3Spec S3 { get; set; }

    }
}
