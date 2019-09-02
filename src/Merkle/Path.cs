using System.Collections.Generic;
using System.Linq;
using System;

namespace Chromia.PostchainClient.GTV
{

    public class Path
    {
        private List<Path> PathElements;

        public Path(List<Path> pathElements)
        {
            this.PathElements = pathElements;
        }

        public Path GetCurrentPathElement()
        {
            return PathElements[0];
        }

        public int Size()
        {
            return PathElements.Count;
        }

        public Path Tail()
        {
            if(PathElements.Count == 0)
            {
                throw new System.Exception("Impossible to tail this array");
            }
            else
            {
                return new Path(PathElements.Skip(1).ToList());
            }
        }

        public string DebugString()
        {
            return "todo";
        }

        public override bool Equals(Object obj)
        {
            if((obj == null) || ! this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            Path p = (Path) obj;
            return this.PathElements.SequenceEqual(p.PathElements);
        }

        public override int GetHashCode()
        {
            return PathElements.GetHashCode();
        }

    }

    public class PathElement
    {
        private SearchablePathElement Previous;

        public PathElement(SearchablePathElement previous)
        {
            this.Previous = previous;
        }
    }

    public class PathLeafElement: PathElement
    {
        public PathLeafElement(SearchablePathElement previous): base(previous)
        {
        }

        public override bool Equals(Object obj)
        {
            if(this == obj)
            {
                return true;
            }
            if(this.GetType() != obj.GetType())
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return this.GetHashCode();
        }

    }

    public abstract class SearchablePathElement: PathElement
    {
        public SearchablePathElement(SearchablePathElement previous): base(previous)
        {
        }

        public abstract int GetSearchKey();
    }

    public class ArrayPathElement: SearchablePathElement
    {
        private int Index;
        public ArrayPathElement(SearchablePathElement previous, int index): base(previous)
        {
            this.Index = index;
        }

        public override int GetSearchKey()
        {
            return Index;
        }

    }

    public class DictPathElement: SearchablePathElement
    {

    }

    public class PathSet
    {

    }

    public static class Util
    {
        public static void BuildPathFromArray()
        {
            
        }

        public static void GetTailIfFirstElementIsArrayOfThisIndex()
        {

        }

    }
}