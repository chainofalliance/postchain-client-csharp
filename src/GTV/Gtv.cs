namespace Chromia.Postchain.Client
{
    internal class Gtv
    {
        public static byte[] Hash (dynamic obj)
        {
            return MerkleProof.MerkleHashSummary(obj, new MerkleHashCalculator(new CryptoSystem())).MerkleHash;
        }
    }    
}