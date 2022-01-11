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
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static List<object> RemoveItem(List<object> list, object item)
        {
            return list.Where(x => !x.Equals(item)).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static List<object> ReplaceNull(List<object> list, object replacement)
        {
            var results = new List<object>();
            for (var i = 0; i < list.Count; i++)
            {
                var current = list[i];
                results.Add(current ?? replacement);
            }

            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static List<List<object>> ReplaceEmptyList(List<List<object>> list, object replacement)
        {
            var results = new List<List<object>>();
            for (var i = 0; i < list.Count; i++)
            {
                var current = list[i];
                results.Add(current.Count == 0 
                    ? new List<object> {replacement} 
                    : current);
            }

            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="ifTrue"></param>
        /// <param name="ifFalse"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static object Weave(List<bool> condition, List<object> ifTrue, List<object> ifFalse)
        {
            var trueQueue = new Queue(ifTrue);
            var falseQueue = new Queue(ifFalse);

            return condition.Select(x => x ? trueQueue.Dequeue() : falseQueue.Dequeue()).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="ifTrue"></param>
        /// <param name="ifFalse"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static object Weave2(List<bool> condition, List<object> ifTrue, List<object> ifFalse)
        {
            return condition.Select((t, i) => t ? ifTrue[i] : ifFalse[i]).ToList();
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static List<object> Clean(List<object> list)
        {
            for (var i = list.Count - 1; i >= 0; i--)
            {
                switch (list[i])
                {
                    case null:
                    case string s when string.IsNullOrWhiteSpace(s):
                        list.RemoveAt(i);
                        break;
                }
            }

            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        [MultiReturn("index", "value")]
        public static Dictionary<string, object> MinIndex(double[] list) 
        {
            var min = list[0];
            var minIndex = 0;

            for (int i = 1; i < list.Length; ++i)
            {
                if (list[i] < min)
                {
                    min = list[i];
                    minIndex = i;
                }
            }

            return new Dictionary<string, object>
            {
                { "index", minIndex},
                { "value", min}
            };
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
