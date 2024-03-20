namespace QdrantOperator.Models
{
    /// <summary>
    /// Snapshot Schedule spec 
    /// </summary>
    public class SnapshotScheduleSpec
    {
        /// <summary>
        /// Schedule
        /// </summary>
        public string Schedule { get; set; }

        /// <summary>
        /// Pause
        /// </summary>
        public bool Pause { get; set; }

        /// <summary>
        /// Snapshot Spec
        /// </summary>
        public SnapshotSpec Snapshot { get; set; }
    }
}
