using System;

namespace QdrantOperator.Models
{
    /// <summary>
    /// Snapshot Node Status class represents the status of a snapshot node.
    /// </summary>
    public class SnapshotNodeStatus
    {
        /// <summary>
        /// Time spent to process this request.
        /// </summary>
        public double Time {  get; set; }
        /// <summary>
        /// status of the snapshot node.
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// Name of the snapshot node.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Creation time of the snapshot node.
        /// </summary>
        public DateTime CreationTime { get; set; }
        /// <summary>
        /// Size of the snapshot node.
        /// </summary>
        public long  Size { get; set; }

        /// <summary>
        /// Checksum of the snapshot node.
        /// </summary>
        public string Checksum { get; set; }

        /// <summary>
        /// Job name of the snapshot node.
        /// </summary>
        public string JobName { get; set; }

    }
}
