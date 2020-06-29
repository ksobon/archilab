using Dynamo.Graph.Nodes;
using Autodesk.DesignScript.Geometry;
using Revit.GeometryConversion;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class Outline
    {
        internal Outline()
        {
        }

        internal Autodesk.Revit.DB.Outline InternalOutline { get; set; }

        internal Outline(Autodesk.Revit.DB.Outline o)
        {
            InternalOutline = o;
        }

        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public Point MinimumPoint
        {
            get { return InternalOutline.MinimumPoint.ToPoint(); }
        }

        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public Point MaximumPoint
        {
            get { return InternalOutline.MaximumPoint.ToPoint(); }
        }
    }
}
