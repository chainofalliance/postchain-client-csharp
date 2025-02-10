using Newtonsoft.Json;
using System;
using System.Numerics;

namespace Chromia.Encoding
{
    /// <inheritdoc />
    public class BigIntegerConverter : JsonConverter<BigInteger>
    {
        /// <inheritdoc />
        public BigIntegerConverter()
        {

        }

        /// <inheritdoc />
        public override BigInteger ReadJson(JsonReader reader, Type objectType, BigInteger existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                return BigInteger.Parse((string)reader.Value);
            }
            else if (reader.TokenType == JsonToken.Integer)
            {
                return new BigInteger((long)reader.Value);
            }
            else if (reader.TokenType == JsonToken.Raw)
            {
                // Handle raw token by parsing it as string
                return BigInteger.Parse(reader.Value.ToString());
            }

            throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing BigInteger.");
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, BigInteger value, JsonSerializer serializer)
        {
            writer.WriteRawValue(value.ToString());
        }
    }
}
