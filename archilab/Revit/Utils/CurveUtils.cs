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

        public static bool OverlapsWithIn2D(this Curve source, Curve compareTo)
        {
            const double tolerance = 0.001;
            if (Math.Abs(source.Length - compareTo.Length) > tolerance) return false;

            var sourceStart = source.GetEndPoint(0).Flatten();
            var sourceEnd = source.GetEndPoint(1).Flatten();
            var compareToStart = compareTo.GetEndPoint(0).Flatten();
            var compareToEnd = compareTo.GetEndPoint(1).Flatten();

            return (sourceStart.IsAlmostEqualTo(compareToStart, tolerance) || sourceStart.IsAlmostEqualTo(compareToEnd, tolerance)) &&
                   (sourceEnd.IsAlmostEqualTo(compareToStart, tolerance) || sourceEnd.IsAlmostEqualTo(compareToEnd, tolerance));
        }

        public static XYZ Flatten(this XYZ pt)
        {
            return new XYZ(pt.X, pt.Y, 0);
        }
    }
}
