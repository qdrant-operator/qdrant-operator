

using k8s.Models;

namespace QdrantOperator.Models
{
    /// <summary>
    /// S3 Spec
    /// </summary>
    public class S3Spec
    {
        /// <summary>
        /// S3 Provider
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// S3 Access Key Information
        /// </summary>
        public V1SecretKeySelector AccessKey { get; set; }

        /// <summary>
        /// S3 Secret Key Information
        /// </summary>
        public V1SecretKeySelector SecretAccessKey { get; set; }

        /// <summary>
        /// S3 Region
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// S3 Bucket Name
        /// </summary>
        public string Bucket { get; set; }

        /// <summary>
        /// S3 Bucket Prefix
        /// </summary>
        public string Prefix { get; set; }

    }
}
