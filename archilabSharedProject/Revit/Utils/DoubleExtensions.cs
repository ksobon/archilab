using System;
using Autodesk.DesignScript.Runtime;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Utils
{
    /// <summary>
    /// 
    /// </summary>
    [SupressImportIntoVM]
    public static class DoubleExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public static bool AlmostEqualTo(this double value1, double value2)
        {
            return Math.Abs(value1 - value2) < 0.001;
        }
    }
}
