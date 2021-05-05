using System;
using Autodesk.DesignScript.Geometry;
using Dynamo.Graph.Nodes;
using Revit.Elements;
using Revit.Elements.Views;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class Tags
    {
        internal Tags()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static Element SetHeadPosition(Element tag, Point point)
        {
            if (tag == null)
                throw new ArgumentNullException(nameof(tag));
            if (point == null)
                throw new ArgumentNullException(nameof(point));

            if (!(tag.InternalElement is Autodesk.Revit.DB.IndependentTag t))
                throw new ArgumentNullException(nameof(tag));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            TransactionManager.Instance.EnsureInTransaction(doc);
            t.TagHeadPosition = point.ToXyz();
            TransactionManager.Instance.TransactionTaskDone();

            return tag;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static bool IsOrphaned(Element tag)
        {
            if (tag == null)
                throw new ArgumentNullException(nameof(tag));

            if (!(tag.InternalElement is Autodesk.Revit.DB.IndependentTag t))
                throw new ArgumentNullException(nameof(tag));

            return t.IsOrphaned;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static string TagText(Element tag)
        {
            if (tag == null)
                throw new ArgumentNullException(nameof(tag));

            if (!(tag.InternalElement is Autodesk.Revit.DB.IndependentTag t))
                throw new ArgumentNullException(nameof(tag));

            return t.TagText;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static Point TagHeadPosition(Element tag)
        {
            if (tag == null)
                throw new ArgumentNullException(nameof(tag));

            if (!(tag.InternalElement is Autodesk.Revit.DB.IndependentTag t))
                throw new ArgumentNullException(nameof(tag));

            return t.TagHeadPosition.ToPoint();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static bool IsMaterialTag(Element tag)
        {
            if (tag == null)
                throw new ArgumentNullException(nameof(tag));

            if (!(tag.InternalElement is Autodesk.Revit.DB.IndependentTag t))
                throw new ArgumentNullException(nameof(tag));

            return t.IsMaterialTag;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static View OwnerView(Element tag)
        {
            if (tag == null)
                throw new ArgumentNullException(nameof(tag));

            if (!(tag.InternalElement is Autodesk.Revit.DB.IndependentTag t))
                throw new ArgumentNullException(nameof(tag));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            return doc.GetElement(t.OwnerViewId).ToDSType(true) as View;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static View OwnerView(int id)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var eId = new Autodesk.Revit.DB.ElementId(id);
            if (!(doc.GetElement(eId) is Autodesk.Revit.DB.IndependentTag t))
                throw new ArgumentNullException(nameof(id));

            return doc.GetElement(t.OwnerViewId).ToDSType(true) as View;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static bool IsTaggingLinkDoc(int id)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var eId = new Autodesk.Revit.DB.ElementId(id);
            if (!(doc.GetElement(eId) is Autodesk.Revit.DB.IndependentTag t))
                throw new ArgumentNullException(nameof(id));

            return t.TaggedLocalElementId == Autodesk.Revit.DB.ElementId.InvalidElementId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static bool IsOrphaned(int id)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var eId = new Autodesk.Revit.DB.ElementId(id);
            if (!(doc.GetElement(eId) is Autodesk.Revit.DB.IndependentTag t))
                throw new ArgumentNullException(nameof(id));

            return t.IsOrphaned;
        }
    }
}
