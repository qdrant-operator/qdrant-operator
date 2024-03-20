using System;

using Qdrant.Client;

namespace QdrantOperator.Extensions
{
    /// <summary>
    /// Model extensions.
    /// </summary>
    public static class ModelExtensions
    {
        /// <summary>
        /// Converts the <see cref="Models.ShardingMethod"/> enum value to the corresponding <see cref="Qdrant.Client.Grpc.ShardingMethod"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Models.ShardingMethod"/> value to convert.</param>
        /// <returns>The converted <see cref="Qdrant.Client.Grpc.ShardingMethod"/> value.</returns>
        public static Qdrant.Client.Grpc.ShardingMethod ToGrpcShardingMethod(this Models.ShardingMethod value)
        {
            switch (value)
            {
                case Models.ShardingMethod.Custom:
                    return Qdrant.Client.Grpc.ShardingMethod.Custom;
                case Models.ShardingMethod.Auto:
                default:
                    return Qdrant.Client.Grpc.ShardingMethod.Auto;
            }
        }

        /// <summary>
        /// Converts the nullable <see cref="Models.ShardingMethod"/> enum value to the corresponding <see cref="Qdrant.Client.Grpc.ShardingMethod"/> value.
        /// </summary>
        /// <param name="value">The nullable <see cref="Models.ShardingMethod"/> value to convert.</param>
        /// <returns>The converted <see cref="Qdrant.Client.Grpc.ShardingMethod"/> value.</returns>
        public static Qdrant.Client.Grpc.ShardingMethod ToGrpcShardingMethod(this Models.ShardingMethod? value)
        {
            if (value == null)
            {
                return Qdrant.Client.Grpc.ShardingMethod.Auto;
            }

            return value.Value.ToGrpcShardingMethod();
        }

        /// <summary>
        /// Converts the <see cref="Models.CompressionRatio"/> enum value to the corresponding <see cref="Qdrant.Client.Grpc.CompressionRatio"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Models.CompressionRatio"/> value to convert.</param>
        /// <returns>The converted <see cref="Qdrant.Client.Grpc.CompressionRatio"/> value.</returns>
        public static Qdrant.Client.Grpc.CompressionRatio ToGrpcCompressionRatio(this Models.CompressionRatio value)
        {
            switch (value)
            {
                case Models.CompressionRatio.x4:
                    return Qdrant.Client.Grpc.CompressionRatio.X4;
                case Models.CompressionRatio.x8:
                    return Qdrant.Client.Grpc.CompressionRatio.X8;
                case Models.CompressionRatio.x16:
                    return Qdrant.Client.Grpc.CompressionRatio.X16;
                case Models.CompressionRatio.x32:
                    return Qdrant.Client.Grpc.CompressionRatio.X32;
                case Models.CompressionRatio.x64:
                    return Qdrant.Client.Grpc.CompressionRatio.X64;
                default:
                    throw new ArgumentException("Unknown value");
            }
        }

        /// <summary>
        /// Converts the <see cref="Models.DistanceFunction"/> enum value to the corresponding <see cref="Qdrant.Client.Grpc.Distance"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Models.DistanceFunction"/> value to convert.</param>
        /// <returns>The converted <see cref="Qdrant.Client.Grpc.Distance"/> value.</returns>
        public static Qdrant.Client.Grpc.Distance ToGrpcDistance(this Models.DistanceFunction value)
        {
            switch (value)
            {
                case Models.DistanceFunction.Cosine:
                    return Qdrant.Client.Grpc.Distance.Cosine;
                case Models.DistanceFunction.Euclid:
                    return Qdrant.Client.Grpc.Distance.Euclid;
                case Models.DistanceFunction.Manhattan:
                    return Qdrant.Client.Grpc.Distance.Manhattan;
                default:
                    throw new ArgumentException("Unknown value");
            }
        }

        /// <summary>
        /// Converts the <see cref="Models.TokenizerType"/> enum value to the corresponding <see cref="Qdrant.Client.Grpc.TokenizerType"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Models.TokenizerType"/> value to convert.</param>
        /// <returns>The converted <see cref="Qdrant.Client.Grpc.TokenizerType"/> value.</returns>
        public static Qdrant.Client.Grpc.TokenizerType ToGrpcTokenizer(this Models.TokenizerType value)
        {
            switch (value)
            {
                case Models.TokenizerType.Prefix:
                    return Qdrant.Client.Grpc.TokenizerType.Prefix;
                case Models.TokenizerType.Word:
                    return Qdrant.Client.Grpc.TokenizerType.Word;
                case Models.TokenizerType.WhiteSpace:
                    return Qdrant.Client.Grpc.TokenizerType.Whitespace;
                case Models.TokenizerType.Multilingual:
                    return Qdrant.Client.Grpc.TokenizerType.Multilingual;
                default:
                    throw new ArgumentException("Unknown value");
            }
        }

        /// <summary>
        /// Converts the <see cref="Models.PayloadSchemaType"/> enum value to the corresponding <see cref="Qdrant.Client.Grpc.PayloadSchemaType"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Models.PayloadSchemaType"/> value to convert.</param>
        /// <returns>The converted <see cref="Qdrant.Client.Grpc.PayloadSchemaType"/> value.</returns>
        public static Qdrant.Client.Grpc.PayloadSchemaType ToGrpcType(this Models.PayloadSchemaType value)
        {
            switch (value)
            {
                case Models.PayloadSchemaType.Bool:
                    return Qdrant.Client.Grpc.PayloadSchemaType.Bool;
                case Models.PayloadSchemaType.Float:
                    return Qdrant.Client.Grpc.PayloadSchemaType.Float;
                case Models.PayloadSchemaType.Keyword:
                    return Qdrant.Client.Grpc.PayloadSchemaType.Keyword;
                case Models.PayloadSchemaType.Text:
                    return Qdrant.Client.Grpc.PayloadSchemaType.Text;
                case Models.PayloadSchemaType.Integer:
                    return Qdrant.Client.Grpc.PayloadSchemaType.Integer;
                case Models.PayloadSchemaType.Geo:
                    return Qdrant.Client.Grpc.PayloadSchemaType.Geo;
                default:
                    throw new ArgumentException("Unknown value");
            }
        }

    }
}
