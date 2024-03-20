namespace QdrantOperator.Models
{
    /// <summary>
    /// Snapshot Spec
    /// </summary>
    public class SnapshotSpec
    {
        /// <summary>
        /// Cluster Name associated to the snapshot
        /// </summary>
        public string Cluster { get; set; }

        /// <summary>
        /// Collection Name associated to the snapshot
        /// </summary>
        public string Collection { get; set; }

        /// <summary>
        /// S3 specs
        /// </summary>
        public S3Spec S3 { get; set; }
    }
}
