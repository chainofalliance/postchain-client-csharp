using System;
using System.Linq;

namespace Chromia.Postchain.Client
{
    internal class MerkleHashSummary
    {
        public  byte[] MerkleHash {get;}
        
        public MerkleHashSummary(byte[] merkleHash)
        {
            this.MerkleHash = merkleHash;
        }

        public override bool Equals(object obj)
        {
            if(this == obj)
            {
                return true;
            }
            if(! this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            MerkleHashSummary p = (MerkleHashSummary) obj;
            if(this.MerkleHash.SequenceEqual(p.MerkleHash))
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return MerkleHash.GetHashCode();
        }

    }
}