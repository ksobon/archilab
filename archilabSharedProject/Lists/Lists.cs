using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Graph.Nodes;
using Autodesk.DesignScript.Runtime;
// ReSharper disable UnusedMember.Global

namespace archilab.Lists
{
    /// <summary>
    /// List related utilities.
    /// </summary>
    public class Lists
    {
        internal Lists()
        {
        }

        /// <summary>
        /// Returns a list of Duplicate item indices as well as unique item indices.
        /// </summary>
        /// <param name="list">List of items to analyze.</param>
        /// <returns>List of Unique and Duplicate item indices.</returns>
        [NodeCategory("Action")]
        [MultiReturn("unique", "duplicate")]
        public static Dictionary<string, object> DuplicateItemIndicies(List<object> list)
        {
            if (!list.Any())
                return new Dictionary<string, object>();

            var duplicate = new List<int>();
            var unique = new List<int>();
            var seen = new HashSet<object>();
            for (var i = 0; i < list.Count; i++)
            {
                var item = list[i];
                if (!seen.Contains(item))
                {
                    seen.Add(item);
                    unique.Add(i);
                }
                else
                {
                    duplicate.Add(i);
                }
            }

            return new Dictionary<string, object>
            {
                { "unique", unique},
                { "duplicate", duplicate}
            };
        }

        /// <summary>
        /// Replace items in a list based on list of index numbers.
        /// </summary>
        /// <param name="list">List to operate on.</param>
        /// <param name="index">List of index values.</param>
        /// <param name="item">List of items to be used as replacements.</param>
        /// <returns name="list">Modified list.</returns>
        [NodeCategory("Action")]
        public static IEnumerable<object> ReplaceItemAtIndex(List<object> list, List<int> index, List<object> item)
        {
            for (var i = 0; i < index.Count; i++)
            {
                list[index[i]] = item[i];
            }
            return list;
        }

        /// <summary>
        /// Sort list by another list.
        /// </summary>
        /// <param name="list">List to be sorted.</param>
        /// <param name="sortBy">List of numbers that will be used to sort by.</param>
        /// <param name="reversed">If true order will be set to descending.</param>
        /// <returns></returns>
        [NodeCategory("Action")]
        [MultiReturn("list", "sortBy")]
        public static Dictionary<string, object> SortListByAnother(object[] list, double[] sortBy, bool reversed = false)
        {
            IComparer revComparer = new ReverseComparer();

            if (reversed)
            {
                Array.Sort(sortBy, list, revComparer);
            }
            else
            {
                Array.Sort(sortBy, list);
            }

            return new Dictionary<string, object>
            {
                { "list", list},
                { "sortBy", sortBy}
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pass"></param>
        /// <param name="objectToPass"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static object BooleanGate(bool pass, object objectToPass)
        {
            return pass ? objectToPass : null;
        }
    }

    /// <summary>
    /// Comparer class that reverses the order of sorting.
    /// </summary>
    internal class ReverseComparer : IComparer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(object x, object y)
        {
            return new CaseInsensitiveComparer().Compare(y, x);
        }
    }
}
