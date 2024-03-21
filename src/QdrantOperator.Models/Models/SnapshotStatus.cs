using System.Collections.Generic;

namespace QdrantOperator.Models
{
    /// <summary>
    /// snapshot Status represents the node and the status response from the snapshot 
    /// </summary>
    public class SnapshotStatus
    {
        /// <summary>
        /// Nodes saves the status of the snapshot of each node.
        /// </summary>
        public Dictionary<string, SnapshotNodeStatus> Nodes { get; set; }
    }
}
