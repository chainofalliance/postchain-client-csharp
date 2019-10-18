namespace Chromia.Postchain.Client.GTV.Merkle.Proof
{
    public static class MerkleProof
    {
        private static BinaryTreeFactory treeFactory = new BinaryTreeFactory();
        private static MerkleProofTreeFactory proofFactory = new MerkleProofTreeFactory();

        public static byte[] MerkleHash(dynamic value, MerkleHashCalculator calculator)
        {
            return MerkleHashSummary(value, calculator).MerkleHash;
        } 

        public static byte[] MerkleTreeHash(MerkleProofTree tree, MerkleHashCalculator calculator)
        {
            return MerkleProofHashSummary(tree, calculator).MerkleHash;
        }

        public static MerkleHashSummary MerkleHashSummary(dynamic value, MerkleHashCalculator calculator)
        {
            var summaryFactory = new MerkleHashSummaryFactory(treeFactory, proofFactory);

            return summaryFactory.CalculateMerkleRoot(value, calculator);
        }

        private static MerkleHashSummary MerkleProofHashSummary(MerkleProofTree tree, MerkleHashCalculator calculator)
        {
            var summaryFactory = new MerkleHashSummaryFactory(treeFactory, proofFactory);

            return summaryFactory.CalculateMerkleTreeRoot(tree, calculator);
        }

        public static MerkleProofTree GenerateProof(dynamic value, PathSet pathSet, MerkleHashCalculator calculator)
        {
            var binaryTree = treeFactory.BuildWithPath(value, pathSet);

            return proofFactory.BuildFromBinaryTree(binaryTree, calculator);
        }
    }
}