using System;
using System.Linq;
using System.Collections.Generic;
using Chromia.Postchain.Client;

namespace Chromia.Postchain.Client
{
    internal enum Asn1TagValues
    {
        ContextSpecific = 1,
        Integer = 2,
        OctetString = 4,
        Null = 5,
        UTF8String = 12,
        Sequence = 16
    }
    
    internal class AsnWriter
    {
        private List<byte> _buffer;
        private List<AsnWriter> _sequences;

        public AsnWriter()
        {
            _buffer = new List<byte>();
            _sequences = new List<AsnWriter>();
        }

        public void WriteNull()
        {
            var buffer = CurrentWriter()._buffer;

            buffer.Add((byte) 0x05);    // tag
            buffer.Add((byte) 0x00);    // content
        }

        public void WriteOctetString(byte[] octetString)
        {
            var buffer = CurrentWriter()._buffer;
            var content = octetString.ToList();

            buffer.Add((byte) 0x04);                        // tag
            buffer.AddRange(GetLengthBytes(content.Count));      // length
            buffer.AddRange(content);   // content
        }

        public void WriteUTF8String(string characterString)
        {
            var buffer = CurrentWriter()._buffer;
            var content = System.Text.Encoding.UTF8.GetBytes(characterString).ToList();
            
            buffer.Add((byte) 0x0c);                        // tag
            buffer.AddRange(GetLengthBytes(content.Count));      // length 
            buffer.AddRange(content);   // content
        }

        public void WriteInteger(long number)
        {
            var buffer = CurrentWriter()._buffer;
            var content = IntegerToBytes(number);
           
            buffer.Add((byte) 0x02);                        // tag
            buffer.AddRange(GetLengthBytes(content.Count));      // length 
            buffer.AddRange(content);// content
        }

        public void PushSequence()
        {
            _sequences.Add(new AsnWriter());
        }

        public void PopSequence()
        {
            var writer = CurrentWriter();
            _sequences.Remove(writer);

            var buffer = CurrentWriter()._buffer;
            var content = writer.Encode().ToList();
            
            buffer.Add((byte) 0x30);                        // tag
            buffer.AddRange(GetLengthBytes(content.Count));      // length
            buffer.AddRange(content);// content
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

        public byte[] Encode()
        {
            if (_sequences.Count != 0)
            {
                throw new System.Exception("Tried to encode with open Sequence.");
            }

            return _buffer.ToArray();
        }

        private AsnWriter CurrentWriter()
        {
            return _sequences.Count == 0 ? this : _sequences[_sequences.Count - 1];
        }

        private List<byte> GetLengthBytes(int length)
        {
            var lengthBytes = new List<byte>();
            if (length < 128)
            {
                lengthBytes.Add((byte) length);
            }
            else
            {
                var sizeInBytes = IntegerToBytes(length, true);
                
                var sizeLength = (byte) sizeInBytes.Count;

                lengthBytes.Add((byte) (0x80 + sizeLength));
                lengthBytes.AddRange(sizeInBytes);
            }

            return lengthBytes;
        }

        private byte[] GetByteList(long integer)
        {
            var byteList = BitConverter.GetBytes(integer);

            List<byte> trimmedBytes = new List<byte>();
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

                if (trimmedBytes.Count == 0 || trimmedBytes[trimmedBytes.Count - 1] < 128)
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

        private List<byte> IntegerToBytes(long integer, bool asLength = false)
        {
            var sizeInBytes = GetByteList(integer);
                
            if (BitConverter.IsLittleEndian)
            {
                sizeInBytes = sizeInBytes.Reverse().ToArray();
            }

            var sizeInBytesList = sizeInBytes.ToList();
            if (sizeInBytesList.Count == 0)
            {
                sizeInBytesList.Add(0x00);
            }
            else if (!asLength && integer >= 0 && sizeInBytesList.First() >= 128)
            {
                sizeInBytesList.Insert(0, 0x00);
            }
            
            return sizeInBytesList;
        }
    }
}