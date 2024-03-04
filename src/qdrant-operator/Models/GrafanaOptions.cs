using System.ComponentModel;

using k8s.Models;
namespace QdrantOperator.Models
{
    /// <summary>
    /// Represents Grafana options.
    /// </summary>
    public class GrafanaOptions
    {
        /// <summary>
        /// Whether to create a GrafanaDashboard resource. This depends on the Grafana
        /// Operator being installed.
        /// </summary>
        public bool DashboardEnabled { get; set; } = false;

        /// <summary>
        /// The datasource name to use for the dashboard.
        /// </summary>
        public string DatasourceName { get; set; }

        /// <summary>
        /// The Grafana instance selector.
        /// </summary>
        public V1LabelSelector InstanceSelector { get; set; }

    }
}
