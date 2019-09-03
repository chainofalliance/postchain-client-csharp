using System;
using System.Collections.Generic;
using Chromia.PostchainClient.GTX;
using Chromia.PostchainClient.GTX.ASN1Messages;

namespace Chromia.PostchainClient.GTV.Merkle
{
    public class CryptoSystem
    {
        public byte[] Digest(byte[] buffer)
        {
            return Chromia.PostchainClient.Util.Sha256(System.Text.Encoding.UTF8.GetBytes(Chromia.PostchainClient.Util.ByteArrayToString(buffer)));
        }
    }

    public class MerkleHashCalculator
    {
        private CryptoSystem CryptoSystem;

        public MerkleHashCalculator(CryptoSystem cryptoSystem)
        {
            this.CryptoSystem = cryptoSystem;
        }

        private byte[] HashingFun(byte[] buffer)
        {
            if(CryptoSystem == null)
            {
                throw new Exception("In this case we need the CryptoSystem to calculate the hash");
            }
            else
            {
                return CryptoSystem.Digest(buffer);
            }
        }

        public byte[] CalculateNodeHash(byte prefix, byte[] hashLeft, byte[] hashRight)
        {
            return CalculateNodeHashInternal(prefix, hashLeft, hashRight);
        }

        public byte[] CalculateLeafHash(dynamic value)
        {
            System.Console.WriteLine("LEAF VALUE: " + value);
            var gtxValue = Gtx.ArgToGTXValue(value);
            return CalculateHashOfValueInternal(gtxValue);
        }

        private byte[] CalculateNodeHashInternal(byte prefix, byte[] hashLeft, byte[] hashRight)
        {
            Console.WriteLine("hash left: " + PostchainClient.Util.ByteArrayToString(hashLeft));
            Console.WriteLine("hash right: " + PostchainClient.Util.ByteArrayToString(hashRight));
            var buf = new List<byte>(){prefix};
            buf.AddRange(hashLeft);
            buf.AddRange(hashRight);
            
            var nodehash = HashingFun(buf.ToArray());
            Console.WriteLine("Node Hash: " + PostchainClient.Util.ByteArrayToString(nodehash));
            return nodehash;
        }

        private byte[] CalculateHashOfValueInternal(GTXValue gtxValue)
        {
            var buf = new List<byte>(){(byte) HashPrefix.Leaf};
            buf.AddRange(gtxValue.Encode());

            System.Console.WriteLine("CalculateHashOfValueInternal:: " + Chromia.PostchainClient.Util.ByteArrayToString(buf.ToArray()));

            var tmp = HashingFun(buf.ToArray());
            //tmp = HashingFun(tmp);

            System.Console.WriteLine("CalculateHashOfValueInternal AFTER HASH:: " + Chromia.PostchainClient.Util.ByteArrayToString(tmp));

            return tmp;
        }

        public bool IsContainerProofValueLeaf(dynamic value)
        {
            // todo
            return false;
        }
    }
}