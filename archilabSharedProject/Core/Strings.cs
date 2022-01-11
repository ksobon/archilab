using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Dynamo.Graph.Nodes;
// ReSharper disable UnusedMember.Global

namespace archilab.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class Strings
    {
        internal Strings()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="searchFor"></param>
        /// <param name="replaceWith"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static string Replace(string str, List<string> searchFor, List<string> replaceWith)
        {
            for (var i = 0; i < searchFor.Count; i++)
            {
                str = str.Replace(searchFor[i], replaceWith[i]);
            }

            return str;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="pattern"></param>
        /// <param name="regexOption"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static bool RegexMatch(string str, string pattern, string regexOption)
        {
            if (string.IsNullOrWhiteSpace(str))
                throw new ArgumentNullException(nameof(str));
            if (string.IsNullOrWhiteSpace(pattern))
                throw new ArgumentNullException(nameof(pattern));
            if (string.IsNullOrWhiteSpace(regexOption))
                throw new ArgumentNullException(nameof(regexOption));

            var o = (RegexOptions)Enum.Parse(typeof(RegexOptions), regexOption);
            var match = Regex.Match(str, pattern, o);
            
            return match.Success;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="number"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static string ToString(int number, string format)
        {
            return number.ToString(format);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static bool IsNullOrWhiteSpace(string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static bool IsNullOrEmpty(string str)
        {
            return string.IsNullOrEmpty(str);
        }
    }
}
