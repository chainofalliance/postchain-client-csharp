using Newtonsoft.Json;
using System;
using System.Numerics;

namespace Chromia.Encoding
{
    /// <inheritdoc />
    internal class BigIntegerConverter : JsonConverter<BigInteger>
    {
        /// <inheritdoc />
        public BigIntegerConverter()
        {

        }

        /// <inheritdoc />
        public override BigInteger ReadJson(JsonReader reader, Type objectType, BigInteger existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var val = reader.Value;
            if (reader.TokenType == JsonToken.String)
                return BigInteger.Parse((string)val);
            else if (reader.TokenType == JsonToken.Integer)
                return new BigInteger((long)val);
            else if (reader.TokenType == JsonToken.Bytes)
                return new BigInteger((byte[])val);

            throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing BigInteger.");
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, BigInteger value, JsonSerializer serializer)
        {
            writer.WriteRawValue(value.ToString());
        }
    }
}
