using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QdrantOperator
{
    /// <summary>
    /// Kubernetes conditions constants.
    /// </summary>
    public static class Conditions
    {
        /// <summary>
        /// Represents the condition for creating a snapshot.
        /// </summary>
        public const string CreatingSnapshot = $"{Constants.KubernetesGroup}/CreatingSnapshot";

        /// <summary>
        /// Represents the true status.
        /// </summary>
        public const string TrueStatus = "True";
    }
}
