using System;
using System.Linq;
using System.Collections.Generic;

namespace Chromia.Postchain.Client
{
    internal class BinaryTreeFactory
    {
        public BinaryTreeElement HandleLeaf(object leaf, PathSet paths, bool IsRoot = false)
        {
            if (paths.IsEmpty() && !IsRoot)
            {
                return this.InnerHandleLeaf(leaf, this.GetEmptyPathSet());
            }
            else
            {
                return this.InnerHandleLeaf(leaf, paths);
            }
        }

        private PathSet GetEmptyPathSet()
        {
            return new PathSet(new Path[0]);
        }

        private BinaryTreeElement InnerHandleLeaf(object leaf, PathSet paths)
        {
            if (leaf is null)
            {
                return this.HandlePrimitiveLeaf(leaf, paths);
            }
            else if (leaf is byte[])
            {
                return this.HandlePrimitiveLeaf(leaf, paths); 
            }
            else if (leaf is string)
            {
                return this.HandlePrimitiveLeaf(leaf, paths); 
            }
            else if (PostchainUtil.IsNumericType(leaf))
            {
                return this.HandlePrimitiveLeaf(leaf, paths); 
            }
            else if (leaf is object[])
            {
                return this.BuildFromArray((object[]) leaf, paths); 
            }
            else if (leaf is Dictionary<string, object>)
            {
                return this.BuildFromDictionary((Dictionary<string, object>) leaf, paths); 
            }
            else
            {
                throw new System.Exception("Unsupporting data type: " + leaf.GetType());
            }
        }

        private BinaryTreeElement HandlePrimitiveLeaf(object leaf, PathSet paths)
        {
            var pathElem = paths.GetPathLeafOrElseAnyCurrentPathElement();
            
            if (pathElem != null && !(pathElem is PathLeafElement))
            {
                throw new System.Exception("Path does not match the tree structure. We are at a leaf " + leaf + " but found path element " + pathElem);
            }

            return new Leaf(leaf, pathElem);
        }

        private List<BinaryTreeElement> BuildHigherLayer(int layer, List<BinaryTreeElement> inList)
        {
            if (inList.Count == 0)
            {
                throw new System.Exception("Cannot work on empty arrays. Layer: " + layer);
            }
            else if (inList.Count == 1)
            {
                return inList;
            }

            var returnArray = new List<BinaryTreeElement>();
            var nrOfNodesToCreate = inList.Count / 2;
            BinaryTreeElement leftValue = null;
            var isLeft = true;
            foreach (var element in inList)
            {
                if (isLeft)
                {
                    leftValue = element;
                    isLeft = false;
                }
                else
                {
                    var tempNode = new Node(leftValue, element);
                    returnArray.Add(tempNode);
                    nrOfNodesToCreate--;
                    isLeft = true;
                    leftValue = null;
                }
            }

            if (!isLeft)
            {
                returnArray.Add(leftValue);
            }

            if (nrOfNodesToCreate != 0)
            {
                System.Console.WriteLine("Why didn't we build exactly the correct amount? Layer: " + layer + " , residue: " + nrOfNodesToCreate +" , input args size: " + inList.Count + ".");
            }

            return this.BuildHigherLayer(layer + 1, returnArray);
        }

        public BinaryTree Build(object data)
        {
            return this.BuildWithPath(data, this.GetEmptyPathSet());
        }

        public BinaryTree BuildWithPath(object data, PathSet paths)
        {
            var result = this.HandleLeaf(data, paths, true);
            return new BinaryTree(result);
        }

        private ArrayHeadNode<object[]> BuildFromArray(object[] array, PathSet paths)
        {
            var pathElem = paths.GetPathLeafOrElseAnyCurrentPathElement();

            if (array.Length == 0)
            {
                return new ArrayHeadNode<object[]>(new EmptyLeaf(), new EmptyLeaf(), array, 0, pathElem);
            }

            var leafArray = this.BuildLeafElements(array, paths);

            var result = this.BuildHigherLayer(1, leafArray);

            var orgRoot = result[0];
            if (orgRoot is Node)
            {
                var nodeRoot = (Node) orgRoot;
                return new ArrayHeadNode<object[]>(nodeRoot.Left, nodeRoot.Right, array, array.Length, pathElem);
            }
            
            if (orgRoot is Leaf)
            {
                return this.BuildFromOneLeaf(array, orgRoot, pathElem);
            }
            else
            {
                throw new System.Exception("Should not find element of this type here");
            }
        }

        private ArrayHeadNode<object[]> BuildFromOneLeaf(object[] array, BinaryTreeElement orgRoot, PathElement pathElem)
        {
            if (array.Length > 1)
            {
                throw new System.Exception("How come we got a leaf returned when we had " + array.Length + " elements is the args?");
            }
            else
            {
                return new ArrayHeadNode<object[]>(orgRoot, new EmptyLeaf(), array, array.Length, pathElem);
            }
        }

        private List<BinaryTreeElement> BuildLeafElements(object[] leafList, PathSet paths)
        {
            var leafArray = new List<BinaryTreeElement>();

            var onlyArrayPaths = paths.KeepOnlyArrayPaths();
            for (int i = 0; i < leafList.Length; i++)
            {
                var pathsRelevantForThisLeaf = onlyArrayPaths.GetTailIfFirstElementIsArrayOfThisIndexFromList(i);
                var leaf = leafList[i];
                var binaryTreeElement = this.HandleLeaf(leaf, pathsRelevantForThisLeaf);
                leafArray.Add(binaryTreeElement);
            }

            return leafArray;
        }

        private DictHeadNode<Dictionary<string,object>> BuildFromDictionary(Dictionary<string,object> dict, PathSet paths)
        {
            var pathElem = paths.GetPathLeafOrElseAnyCurrentPathElement();

            var keys = new List<string>(dict.Keys);
            if (keys.Count == 0)
            {
                return new DictHeadNode<Dictionary<string,object>>(new EmptyLeaf(), new EmptyLeaf(), dict, 0, pathElem);
            }
            keys.Sort();

            var leafArray = this.BuildLeafElementFromDict(keys, dict, paths);

            var result = this.BuildHigherLayer(1, leafArray);

            var orgRoot = result[0];
            if (orgRoot is Node)
            {
                var nodeRoot = (Node) orgRoot;
                return new DictHeadNode<Dictionary<string,object>>(nodeRoot.Left, nodeRoot.Right, dict, keys.Count, pathElem);
            }
            else
            {
                throw new System.Exception("Should not find element of this type here");
            }
        }

        private List<BinaryTreeElement> BuildLeafElementFromDict(List<string> keys, Dictionary<string, object> dict, PathSet paths)
        {
            var leafArray = new List<BinaryTreeElement>();
            var onlyDictPaths = paths.KeepOnlyDictPaths();

            for (int i = 0; i < keys.Count; i++)
            {
                var key = keys[i];
                var keyElement = this.HandleLeaf(key, this.GetEmptyPathSet());
                leafArray.Add(keyElement);

                var content = dict[key];
                var pathsRelevantForThisLeaf = onlyDictPaths.GetTailIfFirstElementIsDictOfThisKeyFromList(key);
                var contentElement = this.HandleLeaf(content, pathsRelevantForThisLeaf);
                leafArray.Add(contentElement);
            }

            return leafArray;
        }
    }
}