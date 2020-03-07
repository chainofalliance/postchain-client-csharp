namespace Chromia.Postchain.Client
{
    internal enum HashPrefix
    {
        Node = 0,
        Leaf = 1,
        NodeArray = 7,
        NodeDict = 8
    }

    internal abstract class BinaryTreeElement
    {
        public PathElement PathElem {get;set;} = null;

        public bool IsPath()
        {
            return this.PathElem != null;
        }

        public bool IsPathLeaf()
        {
            if (this.PathElem == null)
            {
                return false;
            }

            return this.PathElem is PathLeafElement;
        }

        public void SetPathElement(PathElement pathElem)
        {
            this.PathElem = pathElem;
        }

        public abstract int GetPrefixByte();
    }

    internal class Node: BinaryTreeElement
    {
        public BinaryTreeElement Left { get; }
        public BinaryTreeElement Right { get; }

        public Node(BinaryTreeElement left, BinaryTreeElement right)
        {
            this.Left = left;
            this.Right = right;
        }

        public override int GetPrefixByte()
        {
            return (int) HashPrefix.Node;
        }
    }

    internal class SubTreeRootNode<T>: Node
    {
        public T Content {get;}

        public SubTreeRootNode(BinaryTreeElement left, BinaryTreeElement right, T content, PathElement pathElem = null) : base(left, right)
        {
            this.Content = content;
            SetPathElement(pathElem);
        }
    }

    internal class Leaf: BinaryTreeElement
    {
        public object Content {get;}

        public Leaf(object content, PathElement pathElem = null)
        {
            this.Content = content;

            if (pathElem != null)
            {
                if (pathElem is PathLeafElement)
                {
                    SetPathElement(pathElem);
                }
                else
                {
                    throw new System.Exception("The path and object structure does not match! We are at a leaf, but the path expects a sub structure.");
                }
            }
        }

        public override int GetPrefixByte()
        {
            return (int) HashPrefix.Leaf;
        }
    }

    internal class EmptyLeaf: BinaryTreeElement
    {
        public EmptyLeaf(){}

        public override int GetPrefixByte()
        {
            return (int) HashPrefix.Node; //???
        }
    }

    internal class BinaryTree
    {
        public BinaryTreeElement Root {get;}

        public BinaryTree(BinaryTreeElement root)
        {
            this.Root = root;
        }

        public int MaxLevel()
        {
            return this.MaxLevelInternal(this.Root);
        }

        private int MaxLevelInternal(BinaryTreeElement node)
        {
            if (node is EmptyLeaf)
            {
                return 0;
            }
            else if (node is Leaf)
            {
                return 1;
            }
            else if (node is Node)
            {
                var castedNode = (Node) node;
                return System.Math.Max(this.MaxLevelInternal(castedNode.Left), this.MaxLevelInternal(castedNode.Right)) + 1;
            }
            else
            {
                throw new System.Exception("What is this type? " + node.GetType().ToString());
            }
        }
    }

    internal class ArrayHeadNode<T>: SubTreeRootNode<T>
    {
        private int Size;
        public ArrayHeadNode(BinaryTreeElement left, BinaryTreeElement right, T content, int size, PathElement pathElem = null) : base(left, right, content, pathElem)
        {
            this.Size = size;
        }

        public override int GetPrefixByte()
        {
            return (int) HashPrefix.NodeArray;
        }
    }

    internal class DictHeadNode<T>: SubTreeRootNode<T>
    {
        private int Size;

        public DictHeadNode(BinaryTreeElement left, BinaryTreeElement right, T content, int size, PathElement pathElem = null) : base(left, right, content, pathElem)
        {
            this.Size = size;
        }

        public override int GetPrefixByte()
        {
            return (int) HashPrefix.NodeDict;
        }
    }
}
