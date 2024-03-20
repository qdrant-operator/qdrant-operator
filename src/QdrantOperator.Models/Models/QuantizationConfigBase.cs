using System.ComponentModel;

namespace QdrantOperator.Models
{
    /// <summary>
    /// Represents common quantizationconfig parameters.
    /// </summary>
    public class QuantizationConfigBase
    {
        /// <summary>
        /// If true - quantized vectors always will be stored in RAM,
        /// ignoring the config of main storage
        /// </summary>
        [DefaultValue(false)]
        public bool? AlwaysRam { get; set; } = false;
    }
}
