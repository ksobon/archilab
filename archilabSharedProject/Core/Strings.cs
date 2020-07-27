using System.Collections.Generic;

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
        public static string Replace(string str, List<string> searchFor, List<string> replaceWith)
        {
            for (var i = 0; i < searchFor.Count; i++)
            {
                str = str.Replace(searchFor[i], replaceWith[i]);
            }

            return str;
        }
    }
}
