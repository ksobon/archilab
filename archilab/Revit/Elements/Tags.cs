using System;
using Autodesk.DesignScript.Geometry;
using Revit.Elements;
using Revit.Elements.Views;
using Revit.GeometryConversion;
using RevitServices.Persistence;

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
        /// <returns></returns>
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
