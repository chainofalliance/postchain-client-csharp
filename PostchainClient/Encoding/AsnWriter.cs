using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Chromia.Encoding
{
    internal class AsnWriter
    {
        private class Sequence
        {
            public AsnWriter Writer = new AsnWriter();
            public Asn1Choice Choice = Asn1Choice.None;

            public Sequence(Asn1Choice choice)
            {
                Choice = choice;
            }

            public Sequence(AsnWriter writer)
            {
                Writer = writer;
            }
        }

        private readonly List<byte> _buffer = new();
        private readonly List<Sequence> _sequences = new();

        public void WriteNull()
        {
            var buffer = CurrentWriter()._buffer;

            buffer.Add((byte)Asn1Choice.Null);
            buffer.AddRange(GetLengthBytes(2));

            buffer.Add((byte)Asn1Tag.Null);
            buffer.Add(0x00);
        }

        public void WriteOctetString(Buffer buffer)
        {
            WriteOctetString(buffer.Bytes);
        }

        public void WriteOctetString(byte[] octetString)
        {
            var buffer = CurrentWriter()._buffer;
            var content = octetString.ToList();
            var contentSize = GetLengthBytes(content.Count);
            var contentByteSize = content.Count + contentSize.Count + 1;

            buffer.Add((byte)Asn1Choice.ByteArray);
            buffer.AddRange(GetLengthBytes(contentByteSize));

            buffer.Add((byte)Asn1Tag.OctetString);
            buffer.AddRange(contentSize);
            buffer.AddRange(content);
        }

        public void WriteDictKey(string dictKey)
        {
            WriteUTF8String(dictKey, true);
        }

        public void WriteUTF8String(string characterString)
        {
            WriteUTF8String(characterString, false);
        }

        private void WriteUTF8String(string characterString, bool skipChoice)
        {
            var buffer = CurrentWriter()._buffer;
            var content = System.Text.Encoding.UTF8.GetBytes(characterString).ToList();
            var contentSize = GetLengthBytes(content.Count);
            if(!skipChoice)
            { 
                var contentByteSize = content.Count + contentSize.Count + 1;

                buffer.Add((byte)Asn1Choice.String);
                buffer.AddRange(GetLengthBytes(contentByteSize));
            }

            buffer.Add((byte)Asn1Tag.UTF8String);
            buffer.AddRange(contentSize);
            buffer.AddRange(content);
        }

        public void WriteInteger(long number)
        {
            var buffer = CurrentWriter()._buffer;
            var content = LongToBytes(number);
            var contentSize = GetLengthBytes(content.Count);
            var contentByteSize = content.Count + contentSize.Count + 1;

            buffer.Add((byte)Asn1Choice.Integer);
            buffer.AddRange(GetLengthBytes(contentByteSize));

            buffer.Add((byte)Asn1Tag.Integer);
            buffer.AddRange(contentSize);
            buffer.AddRange(content);
        }

        public void WriteBigInteger(BigInteger number)
        {
            var buffer = CurrentWriter()._buffer;
            var content = number.ToByteArray().ToList();
            if (BitConverter.IsLittleEndian)
                content.Reverse();

            var contentSize = GetLengthBytes(content.Count);
            var contentByteSize = content.Count + contentSize.Count + 1;

            buffer.Add((byte)Asn1Choice.BigInteger);
            buffer.AddRange(GetLengthBytes(contentByteSize));

            buffer.Add((byte)Asn1Tag.Integer);
            buffer.AddRange(contentSize);
            buffer.AddRange(content);
        }

        public void PushSequence(Asn1Choice choice)
        {
            _sequences.Add(new(choice));
        }

        public void PopSequence()
        {
            var sequence = CurrentSequence();
            _sequences.Remove(sequence);

            var buffer = CurrentWriter()._buffer;
            var content = sequence.Writer.Encode();
            var contentSize = GetLengthBytes(content.Length);

            if (sequence.Choice != Asn1Choice.None)
            {
                var contentByteSize = content.Length + contentSize.Count + 1; 

                buffer.Add((byte)sequence.Choice);
                buffer.AddRange(GetLengthBytes(contentByteSize));
            }

            buffer.Add((byte)Asn1Tag.Sequence);
            buffer.AddRange(contentSize);
            buffer.AddRange(content.Bytes);
        }

        public void WriteEncodedValue(byte[] encodedValue)
        {
            var buffer = CurrentWriter()._buffer;

            buffer.AddRange(encodedValue);
        }

        public int GetEncodedLength()
        {
            var buffer = CurrentWriter()._buffer;
            return buffer.Count;
        }

        public Buffer Encode()
        {
            if (_sequences.Count != 0)
                throw new Exception("Tried to encode with open Sequence.");

            return Buffer.From(_buffer);
        }

        private AsnWriter CurrentWriter()
        {
            return CurrentSequence()?.Writer ?? this;
        }

        private Sequence CurrentSequence()
        {
            return _sequences.Count == 0 ? new Sequence(this) : _sequences[^1];
        }

        private static List<byte> GetLengthBytes(int length)
        {
            var lengthBytes = new List<byte>();
            if (length < 128)
            {
                lengthBytes.Add((byte) length);
            }
            else
            {
                var sizeInBytes = LongToBytes(length, true);
                
                var sizeLength = (byte) sizeInBytes.Count;

                lengthBytes.Add((byte) (0x80 + sizeLength));
                lengthBytes.AddRange(sizeInBytes);
            }

            return lengthBytes;
        }

        private static byte[] GetByteList(long integer)
        {
            var byteList = BitConverter.GetBytes(integer);

            var trimmedBytes = new List<byte>();
            if (integer >= 0)
            {
                for (int i = byteList.Length - 1; i >= 0; i--)
                {
                    if (byteList[i] != 0)
                    {
                        for (int j = 0; j <= i; j++)
                        {
                            trimmedBytes.Add(byteList[j]);
                        }

                        break;
                    }
                }
            }
            else
            {
                for (int i = byteList.Length - 1; i >= 0; i--)
                {
                    if (byteList[i] != 0xff)
                    {
                        for (int j = 0; j <= i; j++)
                        {
                            trimmedBytes.Add(byteList[j]);
                        }

                        break;
                    }
                }

                if (trimmedBytes.Count == 0 || trimmedBytes[^1] < 128)
                {
                    trimmedBytes.Insert(0, 0xff);
                    if (integer < 0)
                    {
                        trimmedBytes.Reverse();
                    }
                }
            }

            return trimmedBytes.ToArray();
        }

        private static List<byte> LongToBytes(long integer, bool asLength = false)
        {
            var sizeInBytes = GetByteList(integer);
                
            if (BitConverter.IsLittleEndian)
                sizeInBytes = sizeInBytes.Reverse().ToArray();

            var sizeInBytesList = sizeInBytes.ToList();
            if (sizeInBytesList.Count == 0)
                sizeInBytesList.Add(0x00);
            else if (!asLength && integer >= 0 && sizeInBytesList.First() >= 128)
                sizeInBytesList.Insert(0, 0x00);

            return sizeInBytesList;
        }
    }
}