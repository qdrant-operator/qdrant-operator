using System.ComponentModel;

namespace QdrantOperator.Models
{
    public class ImageSpec
    {
        [DefaultValue("qdrant/qdrant")]
        public string Repository { get; set; } = "qdrant/qdrant";

        [DefaultValue("IfNotPresent")]
        public string PullPolicy { get; set; } = "IfNotPresent";

        [DefaultValue("latest")]
        public string Tag { get; set; } = "latest";

    }
}
