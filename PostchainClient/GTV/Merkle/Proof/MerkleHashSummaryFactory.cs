namespace Chromia.Postchain.Client
{
    internal class MerkleHashSummaryFactory
    {
        public BinaryTreeFactory TreeFactory;
        public MerkleProofTreeFactory ProofFactory;

        public MerkleHashSummaryFactory(BinaryTreeFactory treeFactory, MerkleProofTreeFactory proofFactory)
        {
            this.TreeFactory = treeFactory;
            this.ProofFactory = proofFactory;
        }

        public MerkleHashSummary CalculateMerkleRoot(object value, MerkleHashCalculator calculator)
        {
            var binaryTree = this.TreeFactory.Build(value);
            var proofTree = this.ProofFactory.BuildFromBinaryTree(binaryTree, calculator);

            return this.CalculateMerkleRootOfTree(proofTree, calculator);
        }

        public MerkleHashSummary CalculateMerkleTreeRoot(MerkleProofTree tree, MerkleHashCalculator calculator)
        {
            return this.CalculateMerkleRootOfTree(tree, calculator);
        }

        public MerkleHashSummary CalculateMerkleRootOfTree(MerkleProofTree proofTree, MerkleHashCalculator calculator)
        {
            var calculatedSummary = this.CalculateMerkleRootInternal(proofTree.Root, calculator);

            return new MerkleHashSummary(calculatedSummary);
        }

        public byte[] CalculateMerkleRootInternal(MerkleProofElement currentElement, MerkleHashCalculator calculator)
        {
            if (currentElement is ProofHashedLeaf)
            {
                var leafElement = (ProofHashedLeaf) currentElement;
                return leafElement.MerkleHash;
            }
            else if (currentElement is ProofValueLeaf)
            {
                var valueElement = (ProofValueLeaf) currentElement;
                var value = valueElement.Content;
                if (calculator.IsContainerProofValueLeaf(value))
                {
                    var merkleProofTree = this.BuildProofTree(value, calculator);
                    return this.CalculateMerkleRootInternal(merkleProofTree.Root, calculator);
                }
                else
                {
                    return calculator.CalculateLeafHash(value);
                }
            }
            else if (currentElement is ProofNode)
            {
                var proofElement = (ProofNode) currentElement;
                var left = this.CalculateMerkleRootInternal(proofElement.Left, calculator);
                var right = this.CalculateMerkleRootInternal(proofElement.Right, calculator);

                return calculator.CalculateNodeHash(proofElement.Prefix, left, right);
            }
            else
            {
                throw new System.Exception("Should have handled this type? " + currentElement.GetType());
            }
        }

        public MerkleProofTree BuildProofTree(object value, MerkleHashCalculator calculator)
        {
            var root = this.TreeFactory.Build(value);

            return this.ProofFactory.BuildFromBinaryTree(root, calculator);
        }
    }
}