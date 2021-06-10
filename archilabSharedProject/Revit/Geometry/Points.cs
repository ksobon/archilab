using System;
using Autodesk.DesignScript.Geometry;
using Dynamo.Graph.Nodes;
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
    }
}
