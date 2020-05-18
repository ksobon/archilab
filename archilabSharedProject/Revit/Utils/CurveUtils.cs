using System;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;

namespace archilab.Revit.Utils
{
    /// <summary>
    /// 
    /// </summary>
    [SupressImportIntoVM]
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="compareTo"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static XYZ Flatten(this XYZ pt)
        {
            return new XYZ(pt.X, pt.Y, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static Curve Offset(this Curve curve, double offset)
        {
            switch (curve)
            {
                case Line line:
                    var start = line.GetEndPoint(0);
                    var end = line.GetEndPoint(1);

                    var oStart = new XYZ(start.X, start.Y, start.Z + offset);
                    var oEnd = new XYZ(end.X, end.Y, end.Z + offset);

                    return Line.CreateBound(oStart, oEnd);
                case Arc arc:
                    var start1 = arc.Evaluate(0, true);
                    var mid = arc.Evaluate(0.5, true);
                    var end1 = arc.Evaluate(1, true);

                    var oStart1 = new XYZ(start1.X, start1.Y, start1.Z + offset);
                    var oMid = new XYZ(mid.X, mid.Y, mid.Z + offset);
                    var oEnd1 = new XYZ(end1.X, end1.Y, end1.Z + offset);

                    return Arc.Create(oStart1, oEnd1, oMid);
                case CylindricalHelix unused:
                case Ellipse unused1:
                case HermiteSpline unused2:
                case NurbSpline unused3:
                    return curve;
                default:
                    return curve;
            }
        }
    }
}
