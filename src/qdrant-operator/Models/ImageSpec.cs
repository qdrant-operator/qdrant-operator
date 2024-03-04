using System.ComponentModel;

namespace QdrantOperator.Models
{
    /// <summary>
    /// Options for defining the container image.
    /// </summary>
    public class ImageSpec
    {
        /// <summary>
        /// The image repository.
        /// </summary>
        [DefaultValue("qdrant/qdrant")]
        public string Repository { get; set; } = "qdrant/qdrant";

        /// <summary>
        /// The image pull policy.
        /// </summary>
        [DefaultValue("IfNotPresent")]
        public string PullPolicy { get; set; } = "IfNotPresent";

        /// <summary>
        /// The image tag.
        /// </summary>
        [DefaultValue("latest")]
        public string Tag { get; set; } = "latest";

    }
}
