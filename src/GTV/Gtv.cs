using Chromia.Postchain.Client.GTV.Merkle.Proof;
using Chromia.Postchain.Client.GTV.Merkle;

namespace Chromia.Postchain.Client.GTV
{
    public class Gtv
    {
        private static MerkleHashCalculator HashCalculator = new MerkleHashCalculator(new CryptoSystem());
        public static byte[] Hash (dynamic obj)
        {
            return MerkleProof.MerkleHashSummary(obj, HashCalculator).MerkleHash;
        }
    }    
}