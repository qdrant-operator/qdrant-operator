using System.Collections.Generic;

using k8s;
using k8s.Models;

using Neon.K8s;

namespace QdrantOperator.Entities
{
    public static class EntityHelper
    {
        public static string GetFullName(this IKubernetesObject<V1ObjectMeta> kubernetesObject)
        {
            return $"qdrant-{kubernetesObject.Metadata.Name}";
        }

        public static void SetLabels(this V1ObjectMeta metadata, Dictionary<string, string> labels)
        {
            foreach (var label in labels)
            {
                metadata.SetLabel(label.Key, label.Value);
            }
        }
    }
}
