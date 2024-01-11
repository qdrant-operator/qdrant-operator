using System.ComponentModel;

namespace QdrantOperator.Models
{
    public class MetricsOptions
    {
        [DefaultValue(false)]
        public bool ServiceMonitorEnabled { get; set; } = false;

        [DefaultValue("60s")]
        public string Interval { get; set; } = "60s";

        [DefaultValue("10s")]
        public string ScrapeTimeout { get; set; } = "10s";

        [DefaultValue(true)]
        public bool HonorLabels { get; set; } = true;
    }
}
