using Autodesk.DesignScript.Runtime;
using System;

namespace archilab.Revit.Utils
{
    /// <summary>
    /// 
    /// </summary>
    [SupressImportIntoVM]
    public static class StringExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="toCheck"></param>
        /// <param name="comp"></param>
        /// <returns></returns>
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }
    }
}
