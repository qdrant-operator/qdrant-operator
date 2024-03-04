using System;

namespace QdrantOperator.Extensions
{
    internal static class ModelExtensions
    {
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

        public static Qdrant.Client.Grpc.ShardingMethod ToGrpcShardingMethod(this Models.ShardingMethod? value)
        {
            if (value == null)
            {
                return Qdrant.Client.Grpc.ShardingMethod.Auto;
            }

            return value.Value.ToGrpcShardingMethod();
        }

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
