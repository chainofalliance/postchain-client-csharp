using Chromia.PostchainClient.GTV.Merkle.Proof;
using Chromia.PostchainClient.GTV.Merkle;

namespace Chromia.PostchainClient.GTV
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