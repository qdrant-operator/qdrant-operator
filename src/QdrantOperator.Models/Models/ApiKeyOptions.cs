using System.ComponentModel;

using k8s.Models;

namespace QdrantOperator.Models
{
    /// <summary>
    /// Specifies the api key options.
    /// </summary>
    public class ApiKeyOptions
    {
        /// <summary>
        /// Whether the api secret is enabled.
        /// </summary>
        [DefaultValue(false)]
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// The reference to the secret. If not specified the secret will be qdrant-[cluster-name].
        /// </summary>
        public V1SecretKeySelector Secret { get; set; }

        /// <summary>
        /// The Api Key length.
        /// </summary>
        [DefaultValue(50)]
        public int KeyLength { get; set; } = 50;
    }
}
