using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using Revit.GeometryConversion;
using Revit.Elements;
using Revit.Elements.Views;
using archilab.Utilities;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class Walls
    {
        internal Walls()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wall"></param>
        /// <returns name="wallType"></returns>
        [NodeCategory("Query")]
        public static Element WallType(Element wall)
        {
            if (wall == null)
                throw new ArgumentException(nameof(wall));

            if (!(wall.InternalElement is Autodesk.Revit.DB.Wall w))
                throw new Exception("Element is not a Wall.");

            return w.WallType.ToDSType(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wall"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static bool IsLineBased(Element wall)
        {
            if (wall == null)
                throw new ArgumentException(nameof(wall));

            if (!(wall.InternalElement is Autodesk.Revit.DB.Wall w))
                throw new Exception("Element is not a Wall.");

            if (!(w.Location is Autodesk.Revit.DB.LocationCurve loc))
                return false;

            var curve = loc.Curve;
            return curve is Autodesk.Revit.DB.Line;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static BoundingBox GetAlignedBoundingBox(Element wall, Offset offset)
        {
            if (wall == null)
                throw new ArgumentException(nameof(wall));

            if (!(wall.InternalElement is Autodesk.Revit.DB.Wall w))
                throw new Exception("Element is not a Wall.");

            if (!(w.Location is Autodesk.Revit.DB.LocationCurve loc) || !(loc.Curve is Autodesk.Revit.DB.Line))
                throw new Exception("This functionality only supports Line based Walls.");

            var bb = w.get_BoundingBox(null);
            var minScaled = bb.Min.ToPoint(); // converts units
            var maxScaled = bb.Max.ToPoint(); // converts units
            var min = Point.ByCoordinates(minScaled.X - offset.Left, minScaled.Y - offset.Bottom, minScaled.Z);
            var max = Point.ByCoordinates(maxScaled.X + offset.Right, maxScaled.Y + offset.Top, maxScaled.Z);

            return BoundingBox.ByCorners(min, max);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wall"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static CurtainGrids CurtainGrid(Element wall)
        {
            if (wall == null)
                throw new ArgumentException(nameof(wall));

            if (!(wall.InternalElement is Autodesk.Revit.DB.Wall w))
                throw new Exception("Element is not a Wall.");

            if (w.WallType.Kind != Autodesk.Revit.DB.WallKind.Curtain)
                throw new Exception("Wall must be a Curtain Wall.");

            return new CurtainGrids(w.CurtainGrid); 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wall"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        [MultiReturn("Faces", "References")]
        public static Dictionary<string, object> GetFaces(Element wall)
        {
            if (wall == null)
                throw new ArgumentException(nameof(wall));

            if (!(wall.InternalElement is Autodesk.Revit.DB.Wall w))
                throw new Exception("Element is not a Wall.");

            var opt = new Autodesk.Revit.DB.Options
            {
                ComputeReferences = true,
                IncludeNonVisibleObjects = true
            };

            var faces = new List<Surface>();
            var references = new List<References>();
            var geomElement = w.get_Geometry(opt);
            foreach (var go in geomElement)
            {
                if (!(go is Autodesk.Revit.DB.Solid s) || s.Faces.Size == 0) continue;

                var face = s.Faces.get_Item(0);
                faces.AddRange(face.ToProtoType());
                references.Add(new References(face.Reference));
            }

            return new Dictionary<string, object>
            {
                { "Faces", faces},
                { "References", references}
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="view"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static Line GetDimensionLine(Element wall, View view, double offset = 5)
        {
            if (wall == null)
                throw new ArgumentNullException(nameof(wall));
            if (!(wall.InternalElement is Autodesk.Revit.DB.Wall w))
                throw new Exception("Element is not a Wall.");
            if (!(w.Location is Autodesk.Revit.DB.LocationCurve loc) || !(loc.Curve is Autodesk.Revit.DB.Line l))
                throw new Exception("This functionality only supports Line based Walls.");
            if (view == null || !(view.InternalElement is Autodesk.Revit.DB.View v))
                throw new ArgumentNullException(nameof(view));

            var startPt = l.GetEndPoint(0);
            var endPt = l.GetEndPoint(1);
            var upDir = v.UpDirection;
            var newStart = startPt + (-upDir * offset);
            var newEnd = endPt - (upDir * offset);

            return Line.ByStartPointEndPoint(newStart.ToPoint(), newEnd.ToPoint());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wall"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        [MultiReturn("Start", "End")]
        public static Dictionary<string, bool> IsWallJoinAllowed(Element wall)
        {
            if (wall == null)
                throw new ArgumentNullException(nameof(wall));
            if (!(wall.InternalElement is Autodesk.Revit.DB.Wall w))
                throw new Exception("Element is not a Wall.");

            return new Dictionary<string, bool>
            {
                { "Start", Autodesk.Revit.DB.WallUtils.IsWallJoinAllowedAtEnd(w, 0)},
                { "End", Autodesk.Revit.DB.WallUtils.IsWallJoinAllowedAtEnd(w, 1)}
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wall"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        [MultiReturn("Start", "End")]
        public static Dictionary<string, string> JoinType(Element wall)
        {
            if (wall == null)
                throw new ArgumentNullException(nameof(wall));
            if (!(wall.InternalElement is Autodesk.Revit.DB.Wall w))
                throw new Exception("Element is not a Wall.");

            if (!(w.Location is Autodesk.Revit.DB.LocationCurve loc)) return null;

            return new Dictionary<string, string>
            {
                { "Start", loc.get_JoinType(0).ToString()},
                { "End", loc.get_JoinType(1).ToString()}
            };
        }
    }
}
