using System;
using System.Linq;
using Dynamo.Graph.Nodes;
// ReSharper disable UnusedMember.Global

namespace archilab.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class Dictionaries
    {
        internal Dictionaries()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static object TryGetValueAtKey(DesignScript.Builtin.Dictionary dictionary, string key)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            return dictionary.Keys.Contains(key)
                ? dictionary.ValueAtKey(key)
                : null;
        }
    }
}
