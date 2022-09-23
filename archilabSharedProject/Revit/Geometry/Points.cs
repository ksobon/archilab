using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;
using Org.BouncyCastle.Asn1.Crmf;

// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Geometry
{
    /// <summary>
    /// 
    /// </summary>
    public class Points
    {
        internal Points()
        { 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="plane"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static Point PullOntoPlane(Point point, Plane plane)
        {
            var n = plane.Normal;
            var o = plane.Origin;
            var d = n.Dot(o.AsVector()) - n.Dot(point.AsVector());
            
            return point.Add(n.Scale(d));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static Point RoundToPrecision(Point point, double precision)
        {
            var x = Math.Round(point.X / precision) * precision;
            var y = Math.Round(point.Y / precision) * precision;
            var z = Math.Round(point.Z / precision) * precision;

            return Point.ByCoordinates(x, y, z);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        public static List<Point> UnifyZ(List<Point> points, double precision)
        {
            var resultList = new List<Point>(new Point[points.Count]);
            var comparer = new DoubleEqualityComparer(precision);
            var wrappedPoints = points.Select((t, i) => new PointWrapper(i, t)).ToList();
            var groups = wrappedPoints.GroupBy(x => x.Point.Z, comparer);
            foreach (var group in groups)
            {
                var averageZ = group.Select(x => x.Point.Z).Average();
                foreach (var pw in group)
                {
                    var newPoint = Point.ByCoordinates(pw.Point.X, pw.Point.Y, averageZ);
                    resultList[pw.Index] = newPoint;
                }
            }

            return resultList;
        }
    }

    [IsVisibleInDynamoLibrary(false)]
    public class PointWrapper
    {
        public int Index { get; set; }
        public Point Point { get; set; }

        public PointWrapper(int index, Point pt)
        {
            Index = index;
            Point = pt;
        }
    }

    [IsVisibleInDynamoLibrary(false)]
    public class DoubleEqualityComparer : IEqualityComparer<double>
    {
        private readonly double _t;

        public DoubleEqualityComparer(double tolerance)
        {
            _t = tolerance;
        }

        public bool Equals(double d1, double d2)
        {
            return EQ(d1, d2, _t);
        }

        public int GetHashCode(double d)
        {
            return 1.GetHashCode();
        }
        public bool EQ(double dbl, double compareDbl, double tolerance)
        {
            return Math.Abs(dbl - compareDbl) < tolerance;
        }
    }
}
