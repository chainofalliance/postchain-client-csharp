using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace Chromia.Encoding
{
    internal class BinaryTreeFactory
    {
        public BinaryTreeElement HandleLeaf(object leaf, PathSet paths, bool IsRoot = false)
        {
            if (paths.IsEmpty() && !IsRoot)
            {
                return InnerHandleLeaf(leaf, GetEmptyPathSet());
            }
            else
            {
                return InnerHandleLeaf(leaf, paths);
            }
        }

        private PathSet GetEmptyPathSet()
        {
            return new PathSet(Array.Empty<Path>());
        }

        private BinaryTreeElement InnerHandleLeaf(object leaf, PathSet paths)
        {
            if (leaf is null)
            {
                return HandlePrimitiveLeaf(leaf, paths);
            }
            else if (leaf is JToken token)
            {
                return InnerHandleLeaf(Gtv.Decode(Gtv.Encode(token)), paths);
            }
            else if (leaf is byte[])
            {
                return HandlePrimitiveLeaf(leaf, paths);
            }
            else if (leaf is Buffer b)
            {
                return HandlePrimitiveLeaf(b.Bytes, paths);
            }
            else if (leaf is string)
            {
                return HandlePrimitiveLeaf(leaf, paths);
            }
            else if (leaf is bool)
            {
                return HandlePrimitiveLeaf(leaf, paths);
            }
            else if (leaf is Enum)
            {
                return InnerHandleLeaf((int)leaf, paths);
            }
            else if (IsNumericType(leaf))
            {
                return HandlePrimitiveLeaf(leaf, paths);
            }
            else if (leaf is double d)
            {
                return HandlePrimitiveLeaf(d.ToString(CultureInfo.InvariantCulture), paths);
            }
            else if (leaf is float f)
            {
                return HandlePrimitiveLeaf(f.ToString(CultureInfo.InvariantCulture), paths);
            }
            else if (leaf is BigInteger)
            {
                return HandlePrimitiveLeaf(leaf, paths);
            }
            else if (leaf is Array array)
            {
                return BuildFromArray(array.Cast<object>().ToArray(), paths);
            }
            else if (leaf is IList list)
            {
                var arr = new object[list.Count];
                list.CopyTo(arr, 0);
                return BuildFromArray(arr, paths);
            }
            else if (leaf is IDictionary dict)
            {
                return BuildFromDictionary(dict, paths);
            }
            else if (IsDictionary(leaf))
            {
                return BuildFromArray((leaf as IDictionary).ToGtv(), paths);
            }
            else if (IsObjectType(leaf))
            {
                return InnerHandleLeaf(Gtv.FromObject(leaf), paths);
            }
            else
            {
                throw new Exception("Unsupporting data type: " + leaf.GetType());
            }
        }

        private BinaryTreeElement HandlePrimitiveLeaf(object leaf, PathSet paths)
        {
            var pathElem = paths.GetPathLeafOrElseAnyCurrentPathElement();

            if (pathElem != null && !(pathElem is PathLeafElement))
            {
                throw new Exception("Path does not match the tree structure. We are at a leaf " + leaf + " but found path element " + pathElem);
            }

            return new Leaf(leaf, pathElem);
        }

        private List<BinaryTreeElement> BuildHigherLayer(int layer, List<BinaryTreeElement> inList)
        {
            if (inList.Count == 0)
            {
                throw new Exception("Cannot work on empty arrays. Layer: " + layer);
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

            return this.BuildHigherLayer(layer + 1, returnArray);
        }

        public BinaryTree Build(object data)
        {
            return BuildWithPath(data, GetEmptyPathSet());
        }

        public BinaryTree BuildWithPath(object data, PathSet paths)
        {
            var result = HandleLeaf(data, paths, true);
            return new BinaryTree(result);
        }

        private ArrayHeadNode<object[]> BuildFromArray(object[] array, PathSet paths)
        {
            var pathElem = paths.GetPathLeafOrElseAnyCurrentPathElement();

            if (array.Length == 0)
            {
                return new ArrayHeadNode<object[]>(new EmptyLeaf(), new EmptyLeaf(), array, 0, pathElem);
            }

            var leafArray = BuildLeafElements(array, paths);

            // If we have just a single leaf that is a node we can return immediately (version 2)
            if (leafArray.Count == 1 && leafArray[0] is Node && Gtv.HashVersion > 1)
            {
                return new ArrayHeadNode<object[]>(leafArray[0], new EmptyLeaf(), array, array.Length, pathElem);
            }

            var result = BuildHigherLayer(1, leafArray);
            var orgRoot = result[0];
            if (orgRoot is Node)
            {
                var nodeRoot = (Node)orgRoot;
                return new ArrayHeadNode<object[]>(nodeRoot.Left, nodeRoot.Right, array, array.Length, pathElem);
            }

            if (orgRoot is Leaf)
            {
                return BuildFromOneLeaf(array, orgRoot, pathElem);
            }
            else
            {
                throw new Exception("Should not find element of this type here");
            }
        }

        private ArrayHeadNode<object[]> BuildFromOneLeaf(object[] array, BinaryTreeElement orgRoot, PathElement pathElem)
        {
            if (array.Length > 1)
            {
                throw new Exception("How come we got a leaf returned when we had " + array.Length + " elements is the args?");
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
                var binaryTreeElement = HandleLeaf(leaf, pathsRelevantForThisLeaf);
                leafArray.Add(binaryTreeElement);
            }

            return leafArray;
        }

        private DictHeadNode<Dictionary<string, object>> BuildFromDictionary(IDictionary dict, PathSet paths)
        {
            var pathElem = paths.GetPathLeafOrElseAnyCurrentPathElement();

            if (dict.Count == 0)
            {
                return new DictHeadNode<Dictionary<string, object>>(new EmptyLeaf(), new EmptyLeaf(), new Dictionary<string, object>(), 0, pathElem);
            }

            var leafArray = BuildLeafElementFromDict(dict, paths);

            var result = BuildHigherLayer(1, leafArray);


            var orgRoot = result[0];
            if (orgRoot is Node nodeRoot)
            {
                var specificDict = new Dictionary<string, object>();
                foreach (var key in specificDict.Keys)
                {
                    specificDict[key] = dict[key];
                }

                return new DictHeadNode<Dictionary<string, object>>(nodeRoot.Left, nodeRoot.Right, specificDict, dict.Count, pathElem);
            }
            else
            {
                throw new Exception("Should not find element of this type here");
            }
        }

        private List<BinaryTreeElement> BuildLeafElementFromDict(IDictionary dict, PathSet paths)
        {
            var leafArray = new List<BinaryTreeElement>();
            var onlyDictPaths = paths.KeepOnlyDictPaths();

            var castDict = new List<(string key, object value)>();
            foreach (var key in dict.Keys)
            {
                castDict.Add((key is Buffer b ? b.Parse() : key.ToString(), dict[key]));
            }
            castDict.Sort((a, b) => string.Compare(a.key, b.key));

            foreach (var (key, value) in castDict)
            {
                var keyElement = HandleLeaf(key, GetEmptyPathSet());
                leafArray.Add(keyElement);

                var pathsRelevantForThisLeaf = onlyDictPaths.GetTailIfFirstElementIsDictOfThisKeyFromList(key);
                var contentElement = HandleLeaf(value, pathsRelevantForThisLeaf);
                leafArray.Add(contentElement);
            }

            return leafArray;
        }

        internal static bool IsNumericType(object o)
        {
            switch (Type.GetTypeCode(o.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return true;
                default:
                    return false;
            }
        }

        internal static bool IsObjectType(object o)
        {
            var type = o.GetType();
            return type.IsValueType && !type.IsPrimitive || type.IsClass;
        }

        internal bool IsDictionary(object o)
        {
            if (o == null) return false;
            return o is IDictionary &&
                   o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>));
        }
    }
}