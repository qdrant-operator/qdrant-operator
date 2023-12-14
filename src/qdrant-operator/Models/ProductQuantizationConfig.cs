using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace QdrantOperator.Models
{
    public class QuantizationBase : QuantizationConfigBase
    {
        [Required]
        public CompressionRatio CompressionRatio { get; set; }
      
    }
}
