using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using FluentAssertions;

using k8s;

using QdrantOperator;
using QdrantOperator.Models;
using QdrantOperator.Util;

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
        public void VectorSpecConverterTest()
        {
            var options = new JsonSerializerOptions();

            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.Converters.Add(new JsonStringEnumConverter());
            options.WriteIndented = true;

            var specString = $@"{{
  ""namedVectors"": [
    {{
      ""name"": ""accelerometer-phone"",
      ""size"": 18,
      ""distance"": ""Cosine"",
      ""onDisk"": true
    }},
    {{
      ""name"": ""accelerometer-headphones"",
      ""size"": 18,
      ""distance"": ""Cosine"",
      ""onDisk"": true
    }}
  ]
}}";

            //var spec = JsonSerializer.Deserialize<VectorSpec>(specString, options);

            //var specStringOut = JsonSerializer.Serialize(spec, options);

            //specString.Should().Be(specStringOut);
            //Assert.Equal(specString, specStringOut);

            var s2 = $@"{{
				""distance"": ""Cosine"",
				""namedVectors"": [
					{{
						""distance"": ""Cosine"",
						""name"": ""accelerometer-phone"",
						""onDisk"": true,
						""size"": 18
					}},
					{{
						""distance"": ""Cosine"",
						""name"": ""accelerometer-headphones"",
						""onDisk"": true,
						""size"": 18
					}},
					{{
						""distance"": ""Cosine"",
						""name"": ""supermodel-headphones-acceleration-x"",
						""onDisk"": true,
						""size"": 17
					}},
					{{
						""distance"": ""Cosine"",
						""name"": ""supermodel-headphones-acceleration-y"",
						""onDisk"": true,
						""size"": 17
					}},
					{{
						""distance"": ""Cosine"",
						""name"": ""supermodel-headphones-acceleration-z"",
						""onDisk"": true,
						""size"": 17
					}},
					{{
						""distance"": ""Cosine"",
						""name"": ""supermodel-headphones-rotation-x"",
						""onDisk"": true,
						""size"": 17
					}},
					{{
						""distance"": ""Cosine"",
						""name"": ""supermodel-headphones-rotation-y"",
						""onDisk"": true,
						""size"": 17
					}},
					{{
						""distance"": ""Cosine"",
						""name"": ""supermodel-headphones-rotation-z"",
						""onDisk"": true,
						""size"": 17
					}},
					{{
						""distance"": ""Cosine"",
						""name"": ""supermodel-headphones-pitch"",
						""onDisk"": true,
						""size"": 17
					}},
					{{
						""distance"": ""Cosine"",
						""name"": ""supermodel-headphones-roll"",
						""onDisk"": true,
						""size"": 17
					}},
					{{
						""distance"": ""Cosine"",
						""name"": ""supermodel-headphones-all"",
						""onDisk"": true,
						""size"": 136
					}}
				],
				""onDisk"": false,
				""size"": 1
			}}";

        var spec = JsonSerializer.Deserialize<VectorSpec>(s2, options);

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
