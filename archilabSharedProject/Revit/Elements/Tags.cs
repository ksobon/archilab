using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Graph.Nodes;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Element = Revit.Elements.Element;
using Point = Autodesk.DesignScript.Geometry.Point;
using View = Revit.Elements.Views.View;
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
        /// <param name="leaderEnd"></param>
        /// <param name="leaderElbow"></param>
        /// <param name="leaderEndCondition"></param>
        /// <param name="hasLeader"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static Element SetLeader(Element tag, Point leaderEnd, Point leaderElbow, string leaderEndCondition, bool hasLeader = true)
        {
            if (tag == null)
                throw new ArgumentNullException(nameof(tag));

            if (!(tag.InternalElement is Autodesk.Revit.DB.IndependentTag t))
                throw new ArgumentNullException(nameof(tag));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            var lec = (Autodesk.Revit.DB.LeaderEndCondition)Enum.Parse(typeof(Autodesk.Revit.DB.LeaderEndCondition), leaderEndCondition);
            
            TransactionManager.Instance.EnsureInTransaction(doc);
            t.HasLeader = hasLeader;
            if (hasLeader)
            {
                if (t.CanLeaderEndConditionBeAssigned(lec))
                    t.LeaderEndCondition = lec;
                if (t.LeaderEndCondition == Autodesk.Revit.DB.LeaderEndCondition.Free)
                {
                    t.LeaderEnd = leaderEnd.ToXyz();
                    t.LeaderElbow = leaderElbow.ToXyz();
                }
            }
            TransactionManager.Instance.TransactionTaskDone();

            return tag;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="element"></param>
        /// <param name="tagMode"></param>
        /// <param name="tagOrientation"></param>
        /// <param name="addLeader"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static Element ByElement(View view, Element element, string tagMode, string tagOrientation, bool addLeader = false)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            if (string.IsNullOrWhiteSpace(tagMode))
                throw new ArgumentNullException(nameof(tagMode));
            if (string.IsNullOrWhiteSpace(tagOrientation))
                throw new ArgumentNullException(nameof(tagOrientation));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            var e = element.InternalElement;
            var r = new Autodesk.Revit.DB.Reference(e);
            var v = view.InternalElement;
            var m = (Autodesk.Revit.DB.TagMode)Enum.Parse(typeof(Autodesk.Revit.DB.TagMode), tagMode);
            var o = (Autodesk.Revit.DB.TagOrientation)Enum.Parse(typeof(Autodesk.Revit.DB.TagOrientation), tagOrientation);

            Autodesk.Revit.DB.XYZ l;
            switch (e.Location)
            {
                case Autodesk.Revit.DB.LocationCurve lc:
                    l = lc.Curve.Evaluate(0.5, true);
                    break;
                case Autodesk.Revit.DB.LocationPoint lp:
                    l = lp.Point;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (l == null)
                throw new Exception("Could not retrieve valid Location Point.");

            TransactionManager.Instance.EnsureInTransaction(doc);
            var tag = Autodesk.Revit.DB.IndependentTag.Create(doc, v.Id, r, addLeader, m, o, l);
            TransactionManager.Instance.TransactionTaskDone();

            return tag.ToDSType(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="elements"></param>
        /// <param name="tagMode"></param>
        /// <param name="tagOrientation"></param>
        /// <param name="addLeader"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static List<Element> ByElements(View view, List<Element> elements, string tagMode, string tagOrientation, bool addLeader = false)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));
            if (elements == null || !elements.Any())
                throw new ArgumentNullException(nameof(elements));
            if (string.IsNullOrWhiteSpace(tagMode))
                throw new ArgumentNullException(nameof(tagMode));
            if (string.IsNullOrWhiteSpace(tagOrientation))
                throw new ArgumentNullException(nameof(tagOrientation));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            var v = view.InternalElement;
            var m = (Autodesk.Revit.DB.TagMode)Enum.Parse(typeof(Autodesk.Revit.DB.TagMode), tagMode);
            var o = (Autodesk.Revit.DB.TagOrientation)Enum.Parse(typeof(Autodesk.Revit.DB.TagOrientation), tagOrientation);
            var results = new List<Element>();

            TransactionManager.Instance.EnsureInTransaction(doc);
            foreach (var element in elements)
            {
                var e = element.InternalElement;
                var r = new Autodesk.Revit.DB.Reference(e);

                Autodesk.Revit.DB.XYZ l;
                switch (e.Location)
                {
                    case Autodesk.Revit.DB.LocationCurve lc:
                        l = lc.Curve.Evaluate(0.5, true);
                        break;
                    case Autodesk.Revit.DB.LocationPoint lp:
                        l = lp.Point;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (l == null)
                    continue;
                
                var tag = Autodesk.Revit.DB.IndependentTag.Create(doc, v.Id, r, addLeader, m, o, l);
                r.Dispose();
                e.Dispose();

                results.Add(tag.ToDSType(true));
            }
            TransactionManager.Instance.TransactionTaskDone();

            return results;
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

            switch (tag.InternalElement)
            {
                case Autodesk.Revit.DB.IndependentTag t:
                    return t.TagText;
                case Autodesk.Revit.DB.SpatialElementTag st:
                    return st.TagText;
                default:
                    throw new ArgumentException(nameof(tag));
            }
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
