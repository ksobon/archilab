using System;
using Autodesk.Revit.DB;

namespace archilab.Revit.Utils
{
    public static class CurveExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="compareTo"></param>
        /// <returns></returns>
        public static bool OverlapsWith(this Curve source, Curve compareTo)
        {
            if (Math.Abs(source.Length - compareTo.Length) > 0.001) return false;

            var sourceStart = source.GetEndPoint(0);
            var sourceEnd = source.GetEndPoint(1);
            var compareToStart = compareTo.GetEndPoint(0);
            var compareToEnd = compareTo.GetEndPoint(1);

            return (sourceStart.IsAlmostEqualTo(compareToStart) || sourceStart.IsAlmostEqualTo(compareToEnd)) &&
                   (sourceEnd.IsAlmostEqualTo(compareToStart) || sourceEnd.IsAlmostEqualTo(compareToEnd));
        }
    }
}
