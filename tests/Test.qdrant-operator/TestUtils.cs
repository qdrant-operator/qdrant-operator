using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QdrantOperator;
using QdrantOperator.Util;
using QdrantOperator.Models;

using k8s;
using k8s.Models;

namespace Test.QdrantOperator
{
    public class TestUtils
    {
        [Theory]
        [InlineData("1ms", 1)]
        [InlineData("1s", 1000)]
        [InlineData("1m", 60000)]
        [InlineData("1h", 3600000)]
        [InlineData("1d", 86400000)]
        [InlineData("1w", 604800000)]
        [InlineData("1y", 31536000000)]
        public void CanParseSimple(string duration, long expectedMs)
        {
            var result = DurationHelper.ParseDuration(duration).TotalMilliseconds;

            Assert.Equal(expectedMs, result);
        }

        [Theory]
        [InlineData("1s1ms", 1001)]
        [InlineData("3m3s100ms", 183100)]
        [InlineData("1y1w1d1h1m1s1ms", 32230861001)]
        public void CanParseComplex(string duration, long expectedMs)
        {
            var result = DurationHelper.ParseDuration(duration).TotalMilliseconds;

            Assert.Equal(expectedMs, result);
        }

        [Theory]
        [InlineData(32230861001, "1y1w1d1h1m1s1ms")]
        [InlineData(183100, "3m3s100ms")]
        [InlineData(1001, "1s1ms")]
        public void CanSerialize(long inputMs, string expected)
        {
            var result = DurationHelper.ToDurationString(TimeSpan.FromMilliseconds(inputMs));

            Assert.Equal(expected, result);
        }

        [Fact]
        public void FormatDashboard()
        {
            var d2 = string.Format(Dashboard.DashboardJson, DurationHelper.ToDurationString(TimeSpan.FromMinutes(2)));
        }

        [Fact]
        public void CreateYaml()
        {
            var collection = new V1QdrantCollection().Initialize();

            collection.Metadata.Name = "instep";
            collection.Metadata.NamespaceProperty = "qdrant";

            collection.Spec = new V1QdrantCollection.V1QdrantCollectionSpec();

            collection.Spec.VectorSpec = new VectorSpec();
            collection.Spec.VectorSpec.NamedVectors = new List<NamedVectorSpec>()
            {
                new NamedVectorSpec()
                {
                    Name = "accelerometer-phone",
                    Distance = DistanceFunction.Cosine,
                    Size = 18,
                    OnDisk = true
                },
                new NamedVectorSpec()
                {
                    Name = "accelerometer-headphones",
                    Distance = DistanceFunction.Cosine,
                    Size = 18,
                    OnDisk = true
                },
                new NamedVectorSpec()
                {

                    Name = "supermodel-headphones-acceleration-x",
                    Distance = DistanceFunction.Cosine,
                    Size = 117,
                    OnDisk = true
                },
                new NamedVectorSpec()
                {
                    Name = "supermodel-headphones-acceleration-y",
                    Distance = DistanceFunction.Cosine,
                    Size = 117,
                    OnDisk = true
                },
                new NamedVectorSpec()
                {
                    Name = "supermodel-headphones-acceleration-z",
                    Distance = DistanceFunction.Cosine,
                    Size = 117,
                    OnDisk = true
                },
                new NamedVectorSpec()
                {
                    Name = "supermodel-headphones-rotation-x",
                    Distance = DistanceFunction.Cosine,
                    Size = 117,
                    OnDisk = true
                },
                new NamedVectorSpec()
                {
                    Name = "supermodel-headphones-rotation-y",
                    Distance = DistanceFunction.Cosine,
                    Size = 117,
                    OnDisk = true
                },
                new NamedVectorSpec()
                {
                    Name = "supermodel-headphones-rotation-z",
                    Distance = DistanceFunction.Cosine,
                    Size = 117,
                    OnDisk = true
                },
                new NamedVectorSpec()
                {
                    Name = "supermodel-headphones-pitch",
                    Distance = DistanceFunction.Cosine,
                    Size = 117,
                    OnDisk = true
                },
                new NamedVectorSpec()
                {
                    Name = "supermodel-headphones-roll",
                    Distance = DistanceFunction.Cosine,
                    Size = 117,
                    OnDisk = true
                },
                new NamedVectorSpec()
                {
                    Name = "supermodel-headphones-all",
                    Distance = DistanceFunction.Cosine,
                    Size = 936,
                    OnDisk = true
                }
            };

            collection.Spec.ShardNumber = 10;
            collection.Spec.ReplicationFactor = 3;
            collection.Spec.WriteConsistencyFactor = 1;
            collection.Spec.OnDiskPayload = false;
            collection.Spec.HnswConfig = new HnswConfig()
            {
                M = 16,
                EfConstruct = 100,
                FullScanThreshold = 10000,
                MaxIndexingThreads = 0,
                OnDisk = false
            };
            collection.Spec.OptimizersConfig = new OptimizersConfigDiff()
            {
                DeletedThreshold = 0.2,
                VacuumMinVectorNumber = 1000,
                DefaultSegmentNumber = 0,
                MaxSegmentSize = null,
                MemmapThreshold = null,
                IndexingThreshold = 20000,
                FlushIntervalSec = 5,
                MaxOptimizationThreads = 1
            };

            collection.Spec.WalConfig = new WalConfigDiff()
            {
                WalCapacityMb = 32,
                WalSegmentsAhead = 0
            };

            var yaml = KubernetesYaml.Serialize(collection);
        }
    }
}
