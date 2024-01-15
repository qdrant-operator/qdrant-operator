using System;
using System.Collections.Generic;

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

        public static void AddRange<T, S>(this IDictionary<T, S> source, IDictionary<T, S> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("Collection is null");
            }

            foreach (var item in collection)
            {
                source[item.Key] = item.Value;
            }
        }
    }
}
