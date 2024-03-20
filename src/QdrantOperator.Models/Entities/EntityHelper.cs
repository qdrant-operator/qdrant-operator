using System;
using System.Collections.Generic;

using k8s;
using k8s.Models;

namespace QdrantOperator.Entities
{

    /// <summary>
    /// Helper class for working with entities.
    /// </summary>
    public static class EntityHelper
    {
        /// <summary>
        /// Gets the full name of a Kubernetes object.
        /// </summary>
        /// <param name="kubernetesObject">The Kubernetes object.</param>
        /// <returns>The full name of the Kubernetes object.</returns>
        public static string GetFullName(this IKubernetesObject<V1ObjectMeta> kubernetesObject)
        {
            return $"qdrant-{kubernetesObject.Metadata.Name}";
        }

        /// <summary>
        /// Adds the elements of a collection to the source dictionary.
        /// </summary>
        /// <typeparam name="T">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="S">The type of the values in the dictionary.</typeparam>
        /// <param name="source">The source dictionary.</param>
        /// <param name="collection">The collection to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when the collection is null.</exception>
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
