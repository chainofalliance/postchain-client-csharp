using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace Chromia.Encoding
{
    internal interface IGtv
    {
        Buffer Encode();
        void Decode(AsnReader reader);
    }

    internal static class Gtv
    {
        public static Type[] SupportedTypes = new Type[]
        {
            typeof(int),
            typeof(long),
            typeof(float),
            typeof(double),
            typeof(bool),
            typeof(string),
            typeof(BigInteger),
            typeof(Enum),
            typeof(Buffer),
            typeof(JToken),
            typeof(IList),
            typeof(IDictionary),
            typeof(IGtvSerializable)
        };

        public static bool IsOfValidType(object obj)
        {
            if (obj == null || obj.GetType().IsArray)
                return true;

            foreach (var t in SupportedTypes)
            {
                if (t.IsAssignableFrom(obj.GetType()))
                    return true;
            }

            return false;
        }

        public static JToken FromObject(object obj)
        {
            return JToken.FromObject(obj, JsonSerializer.Create(new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter> { new BigIntegerConverter() }
            }));
        }

        public static Buffer Encode(object obj)
        {
            if (obj == null)
                return new NullGtv().Encode();

            return EncodeFromJToken(FromObject(obj));
        }

        private static Buffer EncodeFromJToken(JToken obj)
        {
            return EncodeToGtv(obj).Encode();
        }

        public static object Decode(Buffer buffer)
        {
            var gtv = DecodeToGtv(buffer);
            return Decode(gtv);
        }

        public static IGtv DecodeToGtv(Buffer buffer)
        {
            var reader = new AsnReader(buffer.Bytes);
            return Decode(reader);
        }

        public static IGtv Decode(AsnReader reader)
        {
            IGtv gtv = reader.PeekChoice() switch
            {
                Asn1Choice.Null => new NullGtv(),
                Asn1Choice.ByteArray => new ByteArrayGtv(),
                Asn1Choice.String => new StringGtv(),
                Asn1Choice.Integer => new IntegerGtv(),
                Asn1Choice.BigInteger => new BigIntegerGtv(),
                Asn1Choice.Array => new ArrayGtv(),
                Asn1Choice.Dict => new DictGtv(),
                _ => throw new ChromiaException("cannot decode choice")
            };
            gtv.Decode(reader);
            return gtv;
        }

        private static object Decode(IGtv gtv)
        {
            object obj;
            if (gtv is NullGtv)
                obj = (gtv as NullGtv).Value;
            else if (gtv is ByteArrayGtv)
                obj = (gtv as ByteArrayGtv).Value;
            else if (gtv is StringGtv)
                obj = (gtv as StringGtv).Value;
            else if (gtv is IntegerGtv)
                obj = (gtv as IntegerGtv).Value;
            else if (gtv is BigIntegerGtv)
                obj = (gtv as BigIntegerGtv).Value;
            else if (gtv is ArrayGtv)
                obj = (gtv as ArrayGtv).Value.Aggregate(new List<object>(), (c, v) =>
                    {
                        c.Add(Decode(v));
                        return c;
                    }).ToArray();
            else if (gtv is DictGtv)
                obj = (gtv as DictGtv).Value.ToDictionary(k => k.Key, v => Decode(v.Value));
            else
                throw new ChromiaException("cannot decode choice");

            return obj;
        }

        public static IGtv EncodeToGtv(JToken obj)
        {
            if (obj == null || obj.Type == JTokenType.Null)
                return new NullGtv();
            else if (obj.Type == JTokenType.Bytes)
                return new ByteArrayGtv((byte[])obj);
            else if (obj.Type == JTokenType.String)
                return new StringGtv((string)obj);
            else if (obj.Type == JTokenType.Float)
                return new StringGtv(obj.ToObject<double>().ToString(CultureInfo.InvariantCulture));
            else if (obj.Type == JTokenType.Boolean)
                return new IntegerGtv(obj.ToObject<bool>() ? 1 : 0);
            else if (obj.Type == JTokenType.Integer)
                return new IntegerGtv(obj.ToObject<long>());
            else if (obj.Type == JTokenType.Raw)
                return new BigIntegerGtv(obj.ToObject<BigInteger>());
            else if (obj.Type == JTokenType.Array)
            {
                var gtvArray = new List<IGtv>();
                foreach (var subArg in obj)
                {
                    gtvArray.Add(EncodeToGtv(subArg));
                }
                return new ArrayGtv(gtvArray.ToArray());
            }
            else if (obj.Type == JTokenType.Object)
            {
                var gtvDict = new Dictionary<string, IGtv>();
                var jobj = obj.ToObject<JObject>();
                foreach (var entry in jobj)
                {
                    gtvDict.Add(entry.Key, EncodeToGtv(entry.Value));
                }
                return new DictGtv(gtvDict);
            }

            throw new ChromiaException("cannot encode object of type " + obj.Type.ToString());
        }

        public static Buffer Hash(object obj)
        {
            return Buffer.From(MerkleProof.MerkleHashSummary(obj, new MerkleHashCalculator(new CryptoSystem())).MerkleHash);
        }
    }

    internal abstract class Gtv<T> : IGtv
    {
        public T Value { get; protected set; }

        public Gtv() { }

        public Gtv(T value)
        {
            Value = value;
        }

        public Buffer Encode()
        {
            var writer = new AsnWriter();
            EncodeInternal(writer);
            return writer.Encode();
        }

        public abstract void Decode(AsnReader reader);

        protected abstract void EncodeInternal(AsnWriter writer);

        public override bool Equals(object obj)
        {
            if ((obj == null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                var gtv = (Gtv<T>)obj;
                return Value == null ? gtv.Value == null : Value.Equals(gtv.Value);
            }
        }

        public override int GetHashCode()
        {
            return Value == null ? 0 : Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value == null ? "<null>" : Value.ToString();
        }
    }

    internal class NullGtv : Gtv<object>
    {
        public NullGtv() : base(null) { }

        protected override void EncodeInternal(AsnWriter writer)
        {
            writer.WriteNull();
        }

        public override void Decode(AsnReader reader)
        {
            reader.ReadNull();
            Value = null;
        }
    }

    internal class ByteArrayGtv : Gtv<Buffer>
    {
        public ByteArrayGtv() { }
        public ByteArrayGtv(byte[] value) : base(Buffer.From(value))
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
        }
        public ByteArrayGtv(Buffer value) : base(value) { }

        protected override void EncodeInternal(AsnWriter writer)
        {
            writer.WriteOctetString(Value);
        }

        public override void Decode(AsnReader reader)
        {
            Value = reader.ReadOctetString();
        }
    }

    internal class StringGtv : Gtv<string>
    {
        public StringGtv() { }
        public StringGtv(string value) : base(value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
        }

        protected override void EncodeInternal(AsnWriter writer)
        {
            if (Value == null)
                throw new ArgumentNullException(nameof(Value));

            writer.WriteUTF8String(Value);
        }

        public override void Decode(AsnReader reader)
        {
            Value = reader.ReadUTF8String();
        }
    }

    internal class IntegerGtv : Gtv<long>
    {
        public IntegerGtv() { }
        public IntegerGtv(long value) : base(value) { }

        protected override void EncodeInternal(AsnWriter writer)
        {
            writer.WriteInteger(Value);
        }

        public override void Decode(AsnReader reader)
        {
            Value = reader.ReadInteger();
        }
    }

    internal class BigIntegerGtv : Gtv<BigInteger>
    {
        public BigIntegerGtv() { }
        public BigIntegerGtv(BigInteger value) : base(value) { }

        protected override void EncodeInternal(AsnWriter writer)
        {
            writer.WriteBigInteger(Value);
        }

        public override void Decode(AsnReader reader)
        {
            Value = reader.ReadBigInteger();
        }
    }

    internal class ArrayGtv : Gtv<IGtv[]>
    {
        public ArrayGtv() { }
        public ArrayGtv(IGtv[] value) : base(value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
        }

        protected override void EncodeInternal(AsnWriter writer)
        {
            if (Value == null)
                throw new ArgumentNullException(nameof(Value));

            writer.PushSequence(Asn1Choice.Array);
            foreach (var gtxValue in Value)
            {
                writer.WriteEncodedValue(gtxValue.Encode().Bytes);
            }
            writer.PopSequence();
        }

        public override void Decode(AsnReader reader)
        {
            var values = new List<IGtv>();
            var array = reader.ReadSequence(Asn1Choice.Array);
            while (array.HasData)
                values.Add(Gtv.Decode(array));

            Value = values.ToArray();
        }

        public List<K> ToList<T, K>() where T : Gtv<K>
        {
            return Value.ToList().Select(e => ((T)e).Value).ToList();
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                var gtv = (Gtv<IGtv[]>)obj;
                return Value == null ? gtv.Value == null : Enumerable.SequenceEqual(Value, gtv.Value);
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return Value == null ? "<null>" : $"[{string.Join(", ", Value.ToList())}]";
        }
    }

    internal class DictGtv : Gtv<Dictionary<string, IGtv>>
    {
        public DictGtv() { }
        public DictGtv(Dictionary<string, IGtv> value) : base(value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
        }

        protected override void EncodeInternal(AsnWriter writer)
        {
            if (Value == null)
                throw new ArgumentNullException(nameof(Value));

            writer.PushSequence(Asn1Choice.Dict);
            foreach (var entry in Value)
            {
                writer.PushSequence(Asn1Choice.None);
                writer.WriteDictKey(entry.Key);
                writer.WriteEncodedValue(entry.Value.Encode().Bytes);
                writer.PopSequence();
            }
            writer.PopSequence();
        }

        public override void Decode(AsnReader reader)
        {
            Value = new Dictionary<string, IGtv>();
            var dict = reader.ReadSequence(Asn1Choice.Dict);
            while (dict.HasData)
            {
                var entry = dict.ReadSequence(Asn1Choice.None);
                var key = entry.ReadDictKey();
                var value = Gtv.Decode(entry);
                Value.Add(key, value);
            }
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                var gtv = (Gtv<Dictionary<string, IGtv>>)obj;
                return Value == null ? gtv.Value == null : Value.Count == gtv.Value.Count && !Value.Except(gtv.Value).Any();
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return Value == null ? "<null>" : Value.Aggregate("{", (c, v) => $"{c}\"{v.Key}\": {v.Value}, ")[..^2] + "}";
        }
    }

    internal static class DictionaryExtenstion
    {
        internal static object[] ToGtv(this IDictionary source)
        {
            var arr = new List<object[]>();
            foreach (DictionaryEntry sourcePair in source)
                arr.Add(new object[] { sourcePair.Key, sourcePair.Value });
            return arr.ToArray();
        }
    }

    internal static class CollectionExtenstion
    {
        internal static object[] ToGtv(this ICollection source)
        {
            var arr = new List<object>();
            foreach (var entry in source)
                arr.Add(entry);
            return arr.ToArray();
        }
    }
}