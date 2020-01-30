using System;
using System.Linq;

namespace Chromia.Postchain.Client.GTX.ASN1Messages
{
    enum Asn1TagValues
    {
        ContextSpecific = 1,
        Integer = 2,
        OctetString = 4,
        Null = 5,
        UTF8String = 12,
        Sequence = 16
    }

    public static class ASN1Util
    {
        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                            .Where(x => x % 2 == 0)
                            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                            .ToArray();
        }

        public static int GetMaxAmountOfBytesForInteger(long value)
        {
            int maxAmount = 0;

            if (value == 0)
            {
                return 1;
            }

            while (value > 0)
            {
                maxAmount += 1;
                value >>= 8;
            }

            return maxAmount;
        }

        public static bool IsNumericType(this object o)
        {   
            switch (Type.GetTypeCode(o.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return true;
                default:
                    return false;
            }
        }

        // public static List<T> SequenceToList<T>(AsnReader sequence, Func<byte[], T> callback)
        // {
        //     var returnList = new List<T>();

        //     while (true)
        //     {
        //         try
        //         {
        //             if (sequence.PeekTag().TagValue == (int)Asn1TagValues.Sequence)
        //             {
        //                 returnList.Add(callback(sequence.ReadEncodedValue().ToArray().ToArray()));
        //             }
        //             // The "ContextSpecific" AsnTag has the same value as Boolean (1). Thats why we check for the tag string.
        //             else if (sequence.PeekTag().TagClass.ToString() == "ContextSpecific")
        //             {
        //                 returnList.Add(callback(sequence.ReadEncodedValue().ToArray()));
        //             }
        //             else if (sequence.PeekTag().TagValue == (int)Asn1TagValues.OctetString)
        //             {
        //                 var ret_val = sequence.ReadOctetString();
        //                 returnList.Add((T)(object)ret_val);
        //             }
        //             else
        //             {
        //                 break;
        //             }
        //         }
        //         catch (System.Security.Cryptography.CryptographicException)
        //         {
        //             break;
        //         }
        //     }

        //     return returnList;
        // }
    }
}