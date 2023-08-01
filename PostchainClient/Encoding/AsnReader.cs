using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Chromia.Encoding
{
    internal class AsnReader
    {
        public bool HasData { get { return _bytes.Count > 0;} }

        private readonly List<byte> _bytes;

        public AsnReader(byte[] bytes)
        {
            _bytes = new List<byte>(bytes);
        }

        public Asn1Choice PeekChoice()
        {
            return (Asn1Choice)GetByte(peek: true);
        }

        private void ReadChoice(Asn1Choice choice)
        {
            GetByte((byte)choice);
            ReadLength();
        }

        public void ReadNull()
        {
            ReadChoice(Asn1Choice.Null);
            GetByte((byte)Asn1Tag.Null);
            GetByte();
        }

        public AsnReader ReadSequence(Asn1Choice choice)
        {
            if (choice != Asn1Choice.None)
                ReadChoice(choice);

            GetByte((byte)Asn1Tag.Sequence);
            
            int length = (int) ReadLength();
        	var sequence = this._bytes.Take(length).ToArray();
            this._bytes.RemoveRange(0, length);

            return new AsnReader(sequence);
        }

        public Buffer ReadOctetString()
        {
            ReadChoice(Asn1Choice.ByteArray);
            GetByte((byte)Asn1Tag.OctetString);
            var length = ReadLength();

            var buffer = new List<byte>();
            for (int i = 0; i < length; i++)
            {
                buffer.Add(GetByte());
            }

            return Buffer.From(buffer);
        }

        public string ReadUTF8String()
        {
            ReadChoice(Asn1Choice.String);
            return ReadDictKey();
        }

        public string ReadDictKey()
        {
            GetByte((byte)Asn1Tag.UTF8String);
            var length = ReadLength();

            var buffer = new List<byte>();
            for (int i = 0; i < length; i++)
            {
                buffer.Add(GetByte());
            }

            return System.Text.Encoding.UTF8.GetString(buffer.ToArray());
        }

        public long ReadInteger()
        {
            ReadChoice(Asn1Choice.Integer);
            GetByte((byte)Asn1Tag.Integer);

            return ReadIntegerInternal(ReadLength());
        }

        public BigInteger ReadBigInteger()
        {
            ReadChoice(Asn1Choice.BigInteger);
            GetByte((byte)Asn1Tag.Integer);
            var length = ReadLength();

            var buffer = Read(length);
            return new BigInteger(buffer.ToArray());
        }

        public long ReadLength()
        {
            var first = GetByte();

            if (first < 128)
            {
                return first;
            }
            else
            {
                return ReadIntegerInternal(first - 0x80, true);
            }
        }

        private long ReadIntegerInternal(long byteAmount, bool onlyPositive = false)
        {
            var buffer = new List<byte>();
            var signByte = 0;
            var isSignByteSet = false;
            for (int i = 0; i < 8; i++)
            {
                if (i < byteAmount)
                {
                    Read(buffer);
                }
                else
                {
                    if (!isSignByteSet)
                    {
                        isSignByteSet = true;
                        signByte = buffer[(int)byteAmount - 1];
                    }

                    var value = (byte)((signByte >= 0x80) && !onlyPositive ? 0xff : 0x00);
                    Add(buffer, value, true);
                }
            }
            return BitConverter.ToInt64(buffer.ToArray(), 0);
        }

        private byte GetByte(byte? expected = null, bool peek = false)
        {
            if (_bytes.Count == 0)
            {
                throw new Exception("No bytes left to read");
            }

            var got = _bytes[0];
            if (expected != null && expected.Value != got)
            {
                throw new InvalidOperationException("Expected byte " + expected.Value.ToString("X2") + ", got " + got.ToString("X2"));
            }

            if (!peek)
                _bytes.RemoveAt(0);
            return  got;
        }

        private byte[] Read(long length)
        {
            var buffer = new List<byte>();
            for (int i = 0; i < length; i++)
                Read(buffer);
            return buffer.ToArray();
        }

        private void Read(List<byte> bytes)
        {
            Add(bytes, GetByte(), false);
        }

        private void Add(List<byte> bytes, byte value, bool atEnd)
        {
            if (BitConverter.IsLittleEndian && !atEnd)
                bytes.Insert(0, value);
            else
                bytes.Add(value);
        }
    }
}