using System.ComponentModel;

using Neon.Operator.Attributes;

using Qdrant.Client.Grpc;

using QdrantOperator.Extensions;

namespace QdrantOperator.Models
{
    public class TextIndexType
    {
        [DefaultValue(TokenizerType.Word)]
        public TokenizerType Tokenizer { get; set; } = TokenizerType.Word;

        [Range(Minimum = 0, ExclusiveMinimum = false)]
        [DefaultValue(null)]
        public int? MinTokenLen { get; set; } = null;

        [Range(Minimum = 0, ExclusiveMinimum = false)]
        [DefaultValue(null)]
        public int? MaxTokenLen { get; set; } = null;

        [DefaultValue(true)]
        public bool? Loweracase { get; set; } = true;

        public Qdrant.Client.Grpc.PayloadIndexParams ToGrpc(TextIndexType other = null)
        {
            var result = new Qdrant.Client.Grpc.PayloadIndexParams()
            {
                TextIndexParams = new TextIndexParams()
                {
                    Tokenizer = Tokenizer.ToGrpcTokenizer()
                }
            };
            
            if (MinTokenLen.HasValue &&
                MinTokenLen != other?.MinTokenLen)
            {
                result.TextIndexParams.MinTokenLen = (uint)MinTokenLen.Value;
            }

            if (MaxTokenLen.HasValue &&
                MaxTokenLen != other?.MaxTokenLen)
            {
                result.TextIndexParams.MaxTokenLen = (uint)MaxTokenLen.Value;
            }
            if (Loweracase.HasValue &&
                Loweracase != other?.Loweracase)
            {
                result.TextIndexParams.Lowercase = Loweracase.Value;
            }


            return result;
        }

    }
}
