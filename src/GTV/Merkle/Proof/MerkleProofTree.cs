using System;

namespace Chromia.Postchain.Client.GTV.Merkle.Proof
{
    public interface MerkleProofElement
    {
    }

    public class ProofNode: MerkleProofElement
    {
        public byte Prefix {get;}
        public MerkleProofElement Left;
        public MerkleProofElement Right;

        public ProofNode(byte prefix, MerkleProofElement left, MerkleProofElement right)
        {
            this.Prefix = prefix;
            this.Left = left;
            this.Right = right;
        }

    }

    public class ProofNodeSimple: ProofNode
    {

        public ProofNodeSimple(MerkleProofElement left, MerkleProofElement right): base((byte)HashPrefix.Node, left, right)
        {
        }

    }

    public class ProofValueLeaf: MerkleProofElement
    {
        public dynamic Content {get;}
        private SearchablePathElement PathElement;

        public ProofValueLeaf(dynamic content, SearchablePathElement pathElem)
        {
            this.Content = content;
            this.PathElement = pathElem;
        }
    }

    public class ProofHashedLeaf: MerkleProofElement
    {
        public byte[] MerkleHash {get;}

        public ProofHashedLeaf(byte[] merkleHash)
        {
            this.MerkleHash = merkleHash;
        }

        public override bool Equals(object obj)
        {
            if(obj is ProofHashedLeaf)
            {
                ProofHashedLeaf p = (ProofHashedLeaf) obj;
                return this.MerkleHash.Equals(p.MerkleHash);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return this.MerkleHash.GetHashCode();
        }
    }

    public class ProofNodeArrayHead: ProofNode
    {
        private SearchablePathElement PathElem;
        public ProofNodeArrayHead(MerkleProofElement left, MerkleProofElement right, SearchablePathElement pathElem = null): base((byte)HashPrefix.NodeArray, left, right)
        {
            this.PathElem = pathElem;
        }

    }

    public class ProofNodeDictHead: ProofNode
    {
        private SearchablePathElement PathElem;
        public ProofNodeDictHead(MerkleProofElement left, MerkleProofElement right, SearchablePathElement pathElem = null): base((byte)HashPrefix.NodeDict, left, right)
        {
            this.PathElem = pathElem;
        }
    }

    public class MerkleProofTree
    {
        public MerkleProofElement Root {get;}

        public MerkleProofTree(MerkleProofElement root)
        {
            this.Root = root;
        }

        public int MaxLevel()
        {
            return MaxLevelInternal(this.Root);
        }

        private int MaxLevelInternal(MerkleProofElement node)
        {
            if(node is ProofValueLeaf)
            {
                return 1;
            }
            else if(node is ProofHashedLeaf)
            {
                return 1;
            }
            else if(node is ProofNode)
            {   ProofNode p = (ProofNode) node;
                return Math.Max(this.MaxLevelInternal(p.Left), this.MaxLevelInternal(p.Right)) + 1;
            }
            else
            {
                throw new System.Exception("Should be able to handle node type: " + node.GetType());
            }
        }
    }

}