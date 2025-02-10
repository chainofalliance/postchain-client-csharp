namespace Chromia.Encoding
{
    internal static class MerkleProof
    {
        private static MerkleProofTreeFactory proofFactory = new MerkleProofTreeFactory();

        public static MerkleHashSummary MerkleHashSummary(object value, MerkleHashCalculator calculator, int hashVersion)
        {
            var treeFactory = new BinaryTreeFactory(hashVersion);
            var summaryFactory = new MerkleHashSummaryFactory(treeFactory, proofFactory);

            return summaryFactory.CalculateMerkleRoot(value, calculator);
        }

        private static MerkleHashSummary MerkleProofHashSummary(MerkleProofTree tree, MerkleHashCalculator calculator, int hashVersion)
        {
            var treeFactory = new BinaryTreeFactory(hashVersion);
            var summaryFactory = new MerkleHashSummaryFactory(treeFactory, proofFactory);

            return summaryFactory.CalculateMerkleTreeRoot(tree, calculator);
        }

        public static MerkleProofTree GenerateProof(object value, PathSet pathSet, MerkleHashCalculator calculator, int hashVersion)
        {
            var treeFactory = new BinaryTreeFactory(hashVersion);
            var binaryTree = treeFactory.BuildWithPath(value, pathSet);

            return proofFactory.BuildFromBinaryTree(binaryTree, calculator);
        }
    }
}