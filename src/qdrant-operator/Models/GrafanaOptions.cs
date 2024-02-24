using System.ComponentModel;

using k8s.Models;
namespace QdrantOperator.Models
{
    public class GrafanaOptions
    {
        public bool DashboardEnabled { get; set; } = false;
        public string DatasourceName { get; set; }

        public V1LabelSelector InstanceSelector { get; set; }

    }
}
