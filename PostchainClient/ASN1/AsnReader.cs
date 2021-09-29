using System;
using System.Collections.Generic;
using System.Linq;

namespace Chromia.Postchain.Client.ASN1
{
    public class AsnReader
    {
        public int RemainingBytes {get {return _bytes.Count;}}

        private List<byte> _bytes;
        private int _readBytes = 0;

        public AsnReader(byte[] bytes)
        {
            this._bytes = new List<byte>(bytes);
        }

        public byte ReadChoice()
        {
            return GetByte();
        }

        public AsnReader ReadSequence()
        {
            GetByte(0x30);
            
            int length = (int) ReadLength();
        	var sequence = this._bytes.Take(length).ToArray();
            this._bytes.RemoveRange(0, length);

            return new AsnReader(sequence);
        }

        public byte[] ReadOctetString()
        {
            GetByte(0x04);
            var length = ReadLength();

            var buffer = new List<byte>();
            for (int i = 0; i < length; i++)
            {
                buffer.Add(GetByte());
            }

            return buffer.ToArray();
        }

        public string ReadUTF8String()
        {
            GetByte(0x0c);
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
            GetByte(0x02);

            return ReadIntegerInternal(ReadLength());
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
            for (int i = 0; i < 8; i++)
            {
                if (i < byteAmount)
                    buffer.Add(GetByte());
                else
                    buffer.Insert(0, (byte) ((buffer[(int) byteAmount-1] >= 0x80) && !onlyPositive ? 0xff : 0x00));
            }

            if (BitConverter.IsLittleEndian)
                buffer.Reverse();

            return BitConverter.ToInt64(buffer.ToArray(), 0);
        }

        private byte GetByte(byte? expected = null)
        {
            if (_bytes.Count == 0)
            {
                throw new System.Exception("No bytes left to read");
            }

            var got = _bytes[0];
            if (expected != null && expected.Value != got)
            {
                throw new System.Exception("Expected byte " + expected.Value.ToString("X2") + ", got " + got.ToString("X2"));
            }

            _readBytes++;
            _bytes.RemoveAt(0);
            return  got;
        }
    }
}