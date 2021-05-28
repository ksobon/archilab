using System;
using Autodesk.DesignScript.Geometry;
using Dynamo.Graph.Nodes;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Views
{
    /// <summary>
    /// 
    /// </summary>
    public class Viewports
    {
        internal Viewports()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewport"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static Point GetBoxCenter(Element viewport)
        {
            if (viewport == null)
                throw new ArgumentNullException(nameof(viewport));

            return ((Autodesk.Revit.DB.Viewport) viewport.InternalElement).GetBoxCenter().ToPoint();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewport"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static Element SetBoxCenter(Element viewport, Point point)
        {
            if (viewport == null)
                throw new ArgumentNullException(nameof(viewport));
            if (point == null)
                throw new ArgumentNullException(nameof(point));

            var doc = DocumentManager.Instance.CurrentDBDocument;

            TransactionManager.Instance.EnsureInTransaction(doc);
            ((Autodesk.Revit.DB.Viewport)viewport.InternalElement).SetBoxCenter(point.ToXyz());
            TransactionManager.Instance.TransactionTaskDone();

            return viewport;
        }
    }
}
