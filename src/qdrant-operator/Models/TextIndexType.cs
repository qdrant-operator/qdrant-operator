using System.ComponentModel;

using Neon.Operator.Attributes;

using Qdrant.Client.Grpc;

using QdrantOperator.Extensions;

namespace QdrantOperator.Models
{
    /// <summary>
    /// Represents a text index type.
    /// </summary>
    public class TextIndexType
    {
        /// <summary>
        /// Full-text index configuration is a bit more complex than other indexes, as you can specify the tokenization parameters.
        /// Tokenization is the process of splitting a string into tokens, which are then indexed in the inverted index.
        /// </summary>
        [DefaultValue(TokenizerType.Word)]
        public TokenizerType Tokenizer { get; set; } = TokenizerType.Word;

        /// <summary>
        /// The minimum token length.
        /// </summary>
        [Range(Minimum = 0, ExclusiveMinimum = false)]
        [DefaultValue(null)]
        public int? MinTokenLen { get; set; } = null;

        /// <summary>
        /// The maximum token length.
        /// </summary>
        [Range(Minimum = 0, ExclusiveMinimum = false)]
        [DefaultValue(null)]
        public int? MaxTokenLen { get; set; } = null;

        /// <summary>
        /// Whether to convert to lowercase.
        /// </summary>
        [DefaultValue(true)]
        public bool? Loweracase { get; set; } = true;

        /// <summary>
        /// Converts to GRPC type.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
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
