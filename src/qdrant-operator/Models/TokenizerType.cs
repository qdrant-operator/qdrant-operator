using System.Runtime.Serialization;

namespace QdrantOperator.Models
{
    /// <summary>
    /// Enumerates tokenizer types.
    /// </summary>
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumMemberConverter))]
    public enum TokenizerType
    {
        /// <summary>
        /// <para>
        /// Splits the string into words, separated by spaces, punctuation marks, and special characters,
        /// and then creates a prefix index for each word. For example: hello will be indexed
        /// as h, he, hel, hell, hello.
        /// </para>
        /// </summary>
        [EnumMember(Value = "prefix")]
        Prefix = 0,

        /// <summary>
        /// Splits the string into words, separated by spaces.
        /// </summary>
        [EnumMember(Value = "whitespace")]
        WhiteSpace,

        /// <summary>
        /// Splits the string into words, separated by spaces, punctuation marks, and special characters.
        /// </summary>
        [EnumMember(Value = "word")]
        Word,

        /// <summary>
        /// <para>
        /// Special type of tokenizer based on charabia package. It allows proper tokenization and lemmatization
        /// for multiple languages, including those with non-latin alphabets and non-space delimiters. See charabia
        /// documentation for full list of supported languages supported normalization options. In the default
        /// build configuration, qdrant does not include support for all languages, due to the increasing size of
        /// the resulting binary. Chinese, Japanese and Korean languages are not enabled by default, but can be
        /// enabled by building qdrant from source with --features multiling-chinese,multiling-japanese,multiling-korean
        /// flags.
        /// </para>
        /// </summary>
        [EnumMember(Value = "multilingual")]
        Multilingual,
    }
}
