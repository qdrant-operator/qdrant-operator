using System;

namespace QdrantOperator.Extensions
{
    public static class ModelExtensions
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
    }
}
