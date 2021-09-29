using System.Collections.Generic;

namespace Chromia.Postchain.Client
{
    internal class MerkleProofTreeFactory
    {
        public MerkleProofTreeFactory(){}

        public MerkleProofTree BuildFromBinaryTree(BinaryTree originalTree, MerkleHashCalculator calculator)
        {
            var rootElem = this.BuildFromBinaryTreeInternal(originalTree.Root, calculator);

            return new MerkleProofTree(rootElem);
        }

        public MerkleProofElement BuildFromBinaryTreeInternal(BinaryTreeElement currentElement, MerkleHashCalculator calculator)
        {
            if (currentElement is EmptyLeaf)
            {
                return new ProofHashedLeaf(new byte[32]);
            }
            else if (currentElement is Leaf)
            {
                var leafElement = (Leaf) currentElement;
                var pathElem = currentElement.PathElem;
                if (!(pathElem is null))
                {
                    if (pathElem is PathLeafElement)
                    {
                        return new ProofValueLeaf(leafElement.Content, pathElem.Previous);
                    }
                    else
                    {
                        throw new System.Exception("The path and structure don't match. We are at a leaf, but path elem is not a leaf: " + pathElem);
                    }
                }
                else
                {
                    var hash = calculator.CalculateLeafHash(leafElement.Content);
                    
                    return new ProofHashedLeaf(hash);
                }
            }
            else if (currentElement is SubTreeRootNode<object>)
            {
                var pathElem = currentElement.PathElem;
                if (!(pathElem is null))
                {
                    if (pathElem is PathLeafElement)
                    {
                        var treeNodeElement = (SubTreeRootNode<object>) currentElement;
                        return new ProofValueLeaf(treeNodeElement.Content, pathElem.Previous);
                    }
                    else
                    {
                        var node = (Node) currentElement;
                        return this.ConvertNode(node, calculator);
                    }
                }
                else
                {
                    var node = (Node) currentElement;
                    return this.ConvertNode(node, calculator);
                }
            }
            else if (currentElement is Node)
            {
                var node = (Node) currentElement;
                return this.ConvertNode(node, calculator);
            }
            else
            {
                throw new System.Exception("Cannot handle " + currentElement);
            }
        }

        public MerkleProofElement ConvertNode(Node node, MerkleHashCalculator calculator)
        {
            var left = this.BuildFromBinaryTreeInternal(node.Left, calculator);
            var right = this.BuildFromBinaryTreeInternal(node.Right, calculator);

            if (left is ProofHashedLeaf && right is ProofHashedLeaf)
            {
                var leftConverted = (ProofHashedLeaf) left;
                var rightConverted = (ProofHashedLeaf) right;                
                var addedHash = calculator.CalculateNodeHash((byte) node.GetPrefixByte(), leftConverted.MerkleHash, rightConverted.MerkleHash);

                return new ProofHashedLeaf(addedHash);
            }
            else
            {
                return BuildNodeOfCorrectType(node, left, right);
            }
        }

        public SearchablePathElement ExtractSearchablePathElement(Node node)
        {
            var pathElem = node.PathElem;
            if (!(pathElem is null))
            {
                return pathElem.Previous;
            }
            else
            {
                return null;
            }
        }

        public ProofNode BuildNodeOfCorrectType(Node node, MerkleProofElement left, MerkleProofElement right)
        {
            if (node is ArrayHeadNode<object[]>)
            {
                return new ProofNodeArrayHead(left, right, this.ExtractSearchablePathElement(node));
            }
            else if (node is DictHeadNode<Dictionary<string, object>>)
            {
                return new ProofNodeDictHead(left, right, this.ExtractSearchablePathElement(node));
            }
            else if (node is Node)
            {
                return new ProofNodeSimple(left, right);
            }
            else
            {
                throw new System.Exception("Should have taken care of this node type: " + node);
            }
        }
    }
}