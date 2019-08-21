using crypto = System.Security.Cryptography;
using secp256k1 = Cryptography.ECDSA;
using System.Collections.Generic;
using System;

namespace Chromia.PostchainClient
{
    public static class Util
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

            var digestBuffer = Sha256(content);

            Console.WriteLine("Bytes to digest:" + BitConverter.ToString(content));
            Console.WriteLine("Digest to sign:" + BitConverter.ToString(digestBuffer));
            Console.WriteLine("privKey: " + BitConverter.ToString(privKey));
            
            return Util.SignDigest(digestBuffer, privKey);
        }

        /**
        * @param digestBuffer to sign. It will not be digested before signing.
        * @param privKey The private key to sign the digest with
        *
        * @return the signature
        */
        public static byte[] SignDigest(byte[] digestBuffer, byte[] privKey)
        {
            return secp256k1.Secp256K1Manager.SignCompressedCompact(digestBuffer, privKey);
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

    }

}
