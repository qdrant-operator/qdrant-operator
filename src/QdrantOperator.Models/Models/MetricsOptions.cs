using System.ComponentModel;

namespace QdrantOperator.Models
{
    /// <summary>
    /// Metrics relation options.
    /// </summary>
    public class MetricsOptions
    {
        /// <summary>
        /// Whether to create a Prometheus ServiceMonitor for the cluster.
        /// </summary>
        [DefaultValue(false)]
        public bool ServiceMonitorEnabled { get; set; } = false;

        /// <summary>
        /// Grafana related options.
        /// </summary>
        public GrafanaOptions Grafana { get; set; } = new GrafanaOptions();

        /// <summary>
        /// The metrics scrape interval. <see cref="ServiceMonitorEnabled"/> must be enabled.
        /// </summary>
        [DefaultValue("60s")]
        public string Interval { get; set; } = "60s";

        /// <summary>
        /// The metrics scrape timeout. <see cref="ServiceMonitorEnabled"/> must be enabled.
        /// </summary>
        [DefaultValue("10s")]
        public string ScrapeTimeout { get; set; } = "10s";

        /// <summary>
        /// Whether to honor labels in the ServiceMonitor. <see cref="ServiceMonitorEnabled"/> must be enabled.
        /// </summary>
        [DefaultValue(true)]
        public bool HonorLabels { get; set; } = true;
    }
}
