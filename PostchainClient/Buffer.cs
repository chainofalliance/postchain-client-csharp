using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Chromia
{
    /// <summary>
    /// Wrapper for byte[] containing helper functionalities and parser.
    /// </summary>
    [JsonConverter(typeof(BufferConverter))]
    public readonly struct Buffer
    {
        private readonly byte[] _bytes;

        /// <summary>
        /// Bytes contained in the buffer.
        /// </summary>
        public byte[] Bytes => _bytes;

        /// <summary>
        /// Amount of bytes stored in the buffer.
        /// </summary>
        public int Length => _bytes.Length;

        /// <summary>
        /// Whether the buffer contains any bytes or is empty.
        /// </summary>
        public bool IsEmpty => _bytes.Length == 0;

        /// <summary>
        /// Parses a <see cref="Buffer"/> from the given string of raw bytes.
        /// To parse a string representing an byte array (e.g. "AFFE") use <see cref="From(string)"/>.
        /// </summary>
        /// <param name="byteString">The string containing the bytes to be parsed.</param>
        /// <returns>The <see cref="Buffer"/> containing the bytes.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Buffer Parse(string byteString)
        {
            if (byteString == null) 
                throw new ArgumentNullException(nameof(byteString));

            return From(byteString.Select(c => (byte)c));
        }

        /// <summary>
        /// Parses a <see cref="Buffer"/> out of a given string representing an byte array (e.g. "AFFE").
        /// To parse a string containing the raw bytes use <see cref="Parse(string)"/>.
        /// Accepts and removes strings prefixed by "0x".
        /// </summary>
        /// <param name="hexString">The string containing the bytes to be parsed. May be prefixed by "0x".</param>
        /// <returns>The <see cref="Buffer"/> containing the bytes.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static Buffer From(string hexString)
        {
            if (hexString == null)
                throw new ArgumentNullException(nameof(hexString));
            else if (hexString.Length % 2 == 1)
                throw new ArgumentException($"has to contain an even number of characters (got \"{hexString}\" ({hexString.Length}))", nameof(hexString));

            hexString = hexString.Trim().Replace("0x", "");
            var bytes = Enumerable.Range(0, hexString.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hexString.Substring(x, 2), 16))
                .ToArray();
            return new Buffer(bytes);
        }

        /// <summary>
        /// Creates <see cref="Buffer"/> by repeating a given character <paramref name="count"/> times.
        /// </summary>
        /// <param name="c">The character to repeat.</param>
        /// <param name="count">How often to repeat the byte.</param>
        /// <returns>The <see cref="Buffer"/> containing the bytes.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Buffer Repeat(char c, int count)
        {
            if (count < 0) 
                throw new ArgumentOutOfRangeException(nameof(count), "has to be positive");

            return From(Enumerable.Repeat((byte)c, count));
        }

        /// <summary>
        /// Creates <see cref="Buffer"/> out of the given <paramref name="bytes"/>.
        /// </summary>
        /// <param name="bytes">The bytes to warp in a buffer.</param>
        /// <returns>The <see cref="Buffer"/> containing the bytes.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Buffer From(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            return new Buffer(bytes);
        }

        /// <summary>
        /// Creates <see cref="Buffer"/> out of the given <paramref name="bytes"/>.
        /// </summary>
        /// <param name="bytes">The bytes to warp in a buffer.</param>
        /// <returns>The <see cref="Buffer"/> containing the bytes.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Buffer From(IEnumerable<byte> bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            return new Buffer(bytes.ToArray());
        }

        /// <summary>
        /// Creates and empty <see cref="Buffer"/>.
        /// </summary>
        /// <returns>The empty <see cref="Buffer"/></returns>
        public static Buffer Empty()
        {
            return new Buffer(Array.Empty<byte>());
        }

        /// <summary>
        /// Creates <see cref="Buffer"/> out of the given <paramref name="bytes"/>.
        /// </summary>
        public Buffer(byte[] bytes)
        {
            _bytes = bytes;
        }

        /// <summary>
        /// Parses the bytes to a readable string.
        /// </summary>
        /// <returns>A readable string of the bytes.</returns>
        public string Parse()
        {
            if (_bytes == null)
                return "<null>";
            var hex = new StringBuilder(_bytes.Length * 2);
            foreach (byte b in _bytes)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }

        /// <summary>
        /// Encodes the bytes in UTF8 format.
        /// </summary>
        /// <returns>The encoded UTF8 string.</returns>
        public string ParseUTF8()
        {
            return System.Text.Encoding.UTF8.GetString(_bytes);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            else if (obj is string)
                obj = From(obj as string);
            else if (!GetType().Equals(obj.GetType()))
                return false;

            var b = (Buffer)obj;
            if (_bytes == null || b._bytes == null)
                return _bytes == null && b._bytes == null;

            return Enumerable.SequenceEqual(_bytes, b._bytes);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Parse().GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"<Buffer {Parse()}>";
        }

        /// <inheritdoc />
        public static bool operator ==(Buffer left, Buffer right)
        {
            return left.Equals(right);
        }

        /// <inheritdoc />
        public static bool operator ==(Buffer left, string right)
        {
            return left.Equals(From(right));
        }

        /// <inheritdoc />
        public static bool operator ==(string left, Buffer right)
        {
            return right == left;
        }

        /// <inheritdoc />
        public static bool operator !=(Buffer left, Buffer right)
        {
            return !(left == right);
        }

        /// <inheritdoc />
        public static bool operator !=(Buffer left, string right)
        {
            return !(left == right);
        }

        /// <inheritdoc />
        public static bool operator !=(string left, Buffer right)
        {
            return !(left == right);
        }

        /// <inheritdoc />
        public static implicit operator string(Buffer b) {
            return b.Parse();
        }

        /// <inheritdoc />
        public static explicit operator Buffer(byte[] bytes)
        {
            return bytes == null ? Empty() : From(bytes);
        }

        /// <inheritdoc />
        public static explicit operator Buffer(string s)
        {
            return s == null ? Empty() : From(s);
        }
    }

    /// <inheritdoc />
    public class BufferConverter : JsonConverter<Buffer>
    {
        /// <inheritdoc />
        public BufferConverter()
        {

        }

        /// <inheritdoc />
        public override Buffer ReadJson(JsonReader reader, Type objectType, Buffer existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var val = reader.Value;
            if (reader.TokenType == JsonToken.String)
                return Buffer.From(Convert.FromBase64String((string)val));
            else
                return Buffer.From((byte[])val);
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, Buffer value, JsonSerializer serializer)
        {
            writer.WriteToken(JsonToken.Bytes, value.Bytes);
        }
    }
}
