namespace QdrantOperator.Util
{
    /// <summary>
    /// Label constants.
    /// </summary>
    public static class Labels
    {
        /// <summary>
        /// Represents the label for the application.
        /// </summary>
        public const string App = "app";

        /// <summary>
        /// Represents the label for the instance of the application.
        /// </summary>
        public const string Instance = "app.kubernetes.io/instance";

        /// <summary>
        /// Represents the label for the name of the application.
        /// </summary>
        public const string Name = "app.kubernetes.io/name";

        /// <summary>
        /// Represents the label for the version of the application.
        /// </summary>
        public const string Version = "app.kubernetes.io/version";

        /// <summary>
        /// Represents the label for the hostname of the Kubernetes node.
        /// </summary>
        public const string KubernetesHostname = "kubernetes.io/hostname";
    }
}
