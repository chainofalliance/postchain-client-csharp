using System;
using System.Collections.Generic;

namespace Chromia.Postchain.Client
{
    internal class CryptoSystem
    {
        public byte[] Digest(byte[] buffer)
        {
            return Chromia.Postchain.Client.PostchainUtil.Sha256(buffer);
        }
    }

    internal class MerkleHashCalculator
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

        public byte[] CalculateLeafHash(object value)
        {
            var gtxValue = Gtx.ArgToGTXValue(value);
            
            return CalculateHashOfValueInternal(gtxValue);
        }

        private byte[] CalculateNodeHashInternal(byte prefix, byte[] hashLeft, byte[] hashRight)
        {
            var buf = new List<byte>(){prefix};
            buf.AddRange(hashLeft);
            buf.AddRange(hashRight);
            
            return HashingFun(buf.ToArray());
        }

        private byte[] CalculateHashOfValueInternal(GTXValue gtxValue)
        {
            var buf = new List<byte>(){(byte) HashPrefix.Leaf};
            buf.AddRange(gtxValue.Encode());

            return HashingFun(buf.ToArray());
        }

        public bool IsContainerProofValueLeaf(object value)
        {
            // todo
            return false;
        }
    }
}