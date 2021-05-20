using Autodesk.DesignScript.Geometry;
using Dynamo.Graph.Nodes;

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
    }
}
