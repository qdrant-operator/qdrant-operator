using k8s;
using k8s.Models;

namespace QdrantOperator.Entities
{
    public static class EntityHelper
    {
        public static string GetFullName(this IKubernetesObject<V1ObjectMeta> kubernetesObject)
        {
            return $"qdrant-{kubernetesObject.Metadata.Name}";
        }
    }
}
