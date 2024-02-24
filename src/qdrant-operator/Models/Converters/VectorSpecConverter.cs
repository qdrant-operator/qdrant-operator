using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using Neon.Common;

namespace QdrantOperator.Models.Converters
{
    public class VectorSpecConverter : JsonConverter<VectorSpec>
    {
        public override VectorSpec Read(
            ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            // initialize

            var result = new VectorSpec();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return result;
                }

                // Get the key.
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                var propertyName = reader.GetString();

                switch (propertyName)
                {
                    case "namedVectors":

                        result.NamedVectors = JsonSerializer.Deserialize<List<NamedVectorSpec>>(ref reader, options);

                        break;

                    case "size":

                        reader.Read();
                        result.Size = reader.GetInt64();

                        break;

                    case "distance":

                        var value = (JsonElement)JsonSerializer.Deserialize<dynamic>(ref reader, options);

                        result.Distance = NeonHelper.ParseEnum<DistanceFunction>(value.ToString());

                        break;

                    case "hnswConfig":

                        result.HnswConfig = JsonSerializer.Deserialize<HnswConfig>(ref reader, options);

                        break;

                    case "quantizationConfig":

                        result.QuantizationConfig = JsonSerializer.Deserialize<QuantizationConfig>(ref reader, options);

                        break;

                    case "onDisk":

                        reader.Read();
                        result.OnDisk = reader.GetBoolean();

                        break;
                }
            }

            return result;
        }
        public override void Write(
            Utf8JsonWriter writer, VectorSpec vectorSpec, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            if (vectorSpec.HnswConfig != null)
            {
                writer.WritePropertyName("hnswConfig");
                JsonSerializer.Serialize(writer, vectorSpec.HnswConfig, options);
            }

            if (vectorSpec.QuantizationConfig != null)
            {
                writer.WritePropertyName("quantizationConfig");
                JsonSerializer.Serialize(writer, vectorSpec.QuantizationConfig, options);
            }


            if (vectorSpec.NamedVectors != null)
            {
                writer.WritePropertyName("namedVectors");
                JsonSerializer.Serialize(writer, vectorSpec.NamedVectors, options);
            }
            else
            {
                writer.WriteNumber("size", vectorSpec.Size);
                writer.WritePropertyName("distance");
                JsonSerializer.Serialize(writer, vectorSpec.Distance, options);

                if (vectorSpec.OnDisk.HasValue)
                {
                    writer.WriteBoolean("onDisk", vectorSpec.OnDisk.GetValueOrDefault());
                }
            }

            writer.WriteEndObject();
        }
    }
}
