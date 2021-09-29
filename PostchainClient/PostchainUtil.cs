using secp256k1 = Cryptography.ECDSA;
using System.Collections.Generic;
using System;
using System.Text;
using System.Linq;

namespace Chromia.Postchain.Client
{
    public static class PostchainUtil
    {
        public static byte[] Sha256(byte[] buffer)
        {
            return secp256k1.Sha256Manager.GetHash(buffer);
        }

        /**
        * @param content to sign. It will be digested before signing.
        * @param privKey The private key to sign the content with
        *
        * @return the signature
        */
        public static byte[] Sign(byte[] content, byte[] privKey)
        {
            if(privKey == null)
            {
                throw new Exception("Programmer error, missing privKey");
            }

            if(privKey.Length != 32)
            {
                throw new Exception("Programmer error. Invalid key length. Expected 32, but got " + privKey.Length);
            }
            
            return PostchainUtil.SignDigest(content, privKey);
        }

        /**
        * @param digestBuffer to sign. It will not be digested before signing.
        * @param privKey The private key to sign the digest with
        *
        * @return the signature
        */
        private static byte[] SignDigest(byte[] digestBuffer, byte[] privKey)
        {
            var recoveryID = 0;
            return secp256k1.Secp256K1Manager.SignCompact(digestBuffer, privKey, out recoveryID);
        }

        /**
        * Creates a key pair (which usually represents one user)
        * @returns {{pubKey: Buffer, privKey: Buffer}}
        */
        public static Dictionary<string, byte[]> MakeKeyPair()
        {
            var privKey = secp256k1.Secp256K1Manager.GenerateRandomKey();
            var pubKey = secp256k1.Secp256K1Manager.GetPublicKey(privKey, true);

            Dictionary<string, byte[]> keys = new Dictionary<string, byte[]>();
            keys.Add("privKey", privKey);
            keys.Add("pubKey", pubKey);

            return keys;
        }

        /**
        * Verify that keypair is correct. Providing the private key, this function returns its associated public key
        * @param privKey: Buffer
        * @returns {pubKey: Buffer}
        */
        public static byte[] VerifyKeyPair(string privKey)
        {   
            var pubKey = secp256k1.Secp256K1Manager.GetPublicKey(HexStringToBuffer(privKey), true);
            return pubKey;
        }

        /**
        * Converts hex string to Buffer
        * @param key: string
        * @returns {Buffer}
        */
        public static byte[] HexStringToBuffer(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                            .Where(x => x % 2 == 0)
                            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                            .ToArray();
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }

        public static byte[] StringToByteArray(string str)
        {
            var retArr = new List<byte>();

            foreach (var c in str)
            {
                retArr.Add((byte) c);
            }
            
            return retArr.ToArray();
        }

        public static Gtx DeserializeGTX(byte[] encodedMessage)
        {
            return Gtx.Decode(encodedMessage);
        }

        public static byte[] HashGTV(object content)
        {
            return Gtv.Hash(content);
        }

        internal static Dictionary<string, object> QueryToDict(string queryName, (string name, object content)[] queryObject)
        {
            var queryDict = new Dictionary<string, object>();

            queryDict.Add("type", queryName);
            if (queryObject != null) 
            {
                foreach (var entry in queryObject)
                {
                    if (entry.content is byte[])
                    {
                        queryDict.Add(entry.name, PostchainUtil.ByteArrayToString((byte[]) entry.content));
                    }
                    else
                    {
                        queryDict.Add(entry.name, entry.content);
                    }
                }
            }

            return queryDict;
        }

        internal static int GetMaxAmountOfBytesForInteger(long value)
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

        internal static bool IsNumericType(this object o)
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
    }
}
