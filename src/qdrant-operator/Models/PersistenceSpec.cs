using System.ComponentModel;

namespace QdrantOperator.Models
{
    public class PersistenceSpec
    {
        [DefaultValue("1Gi")]
        public string Size { get; set; } = "1Gi";

        [DefaultValue(null)]
        public string StorageClassName { get; set; } = null;
    }
}