#region References

using System;
using System.Collections.Generic;
using System.Linq;
using archilab.Revit.Utils;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Element = Revit.Elements.Element;
using View = Revit.Elements.Views.View;

// ReSharper disable UnusedMember.Global

#endregion

namespace archilab.Revit.Elements
{
    /// <summary>
    /// Methods and properties typically associated with Elements in Revit
    /// </summary>
    public class Elements
    {
        internal Elements()
        {
        }

        /// <summary>
        /// Delete element from Revit DB.
        /// </summary>
        /// <param name="element">Element to delete.</param>
        /// <returns></returns>
        /// <search>delete, remove, element</search>
        [NodeCategory("Action")]
        public static bool Delete(Element element)
        {
            if (element == null)
                return false;

            try
            {
                var doc = DocumentManager.Instance.CurrentDBDocument;

                TransactionManager.Instance.EnsureInTransaction(doc);
                doc.Delete(element.InternalElement.Id);
                TransactionManager.Instance.TransactionTaskDone();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="searchString"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        [MultiReturn("first", "in", "out")]
        public static Dictionary<string, object> FilterByName(Element[] elements, string searchString, bool ignoreCase = true)
        {
            object first = null;
            var listIn = new List<Element>();
            var listOut = new List<Element>();
            for (int i = 0; i < elements.Length; i++)
            {
                var e = elements[i];
                if (ignoreCase)
                {
                    if (e.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    {
                        listIn.Add(e);
                        if (first == null)
                            first = e;
                    }
                    else
                        listOut.Add(e);
                }
                else
                {
                    if (e.Name.Contains(searchString))
                    {
                        listIn.Add(e);
                        if (first == null)
                            first = e;
                    }
                    else
                        listOut.Add(e);
                }
            }

            return new Dictionary<string, object>
            {
                { "first", first},
                { "in", listIn},
                { "out", listOut}
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static Category GetCategory(Element element)
        {
            if (!(element.InternalElement is Autodesk.Revit.DB.Element e))
                throw new ArgumentNullException(nameof(element));

            switch (e)
            {
                case Autodesk.Revit.DB.ViewSchedule vs:
                    return Category.ById(vs.Definition.CategoryId.IntegerValue);
                case Autodesk.Revit.DB.Family f:
                    return Category.ById(f.FamilyCategoryId.IntegerValue);
                case Autodesk.Revit.DB.GraphicsStyle gs:
                    return Category.ById(gs.GraphicsStyleCategory.Id.IntegerValue);
                default:
                    return Category.ById(e.Category.Id.IntegerValue);
            }
        }

        /// <summary>
        /// Returns worksharing information about element.
        /// </summary>
        /// <param name="element">Element to analyze.</param>
        /// <returns>Information about the Elements Owner, Creator etc.</returns>
        [NodeCategory("Query")]
        [MultiReturn("Creator", "Owner", "LastChangedBy")]
        public static Dictionary<string, string> GetWorksharingTooltipInfo(Element element)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var tooltipInfo = Autodesk.Revit.DB.WorksharingUtils.GetWorksharingTooltipInfo(doc, element.InternalElement.Id);
            return new Dictionary<string, string>
            {
                { "Creator", tooltipInfo.Creator},
                { "Owner", tooltipInfo.Owner},
                { "LastChangedBy", tooltipInfo.LastChangedBy}
            };
        }

        /// <summary>
        /// Demolished Phase assigned to Element.
        /// </summary>
        /// <param name="element"></param>
        /// <returns name="Phase"></returns>
        [NodeCategory("Query")]
        public static int PhaseDemolished(Element element)
        {
            return element.InternalElement.DemolishedPhaseId.IntegerValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static Element Assembly(Element element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            var assId = element.InternalElement.AssemblyInstanceId;
            if (assId == null || assId == Autodesk.Revit.DB.ElementId.InvalidElementId)
                return null;

            return doc.GetElement(assId).ToDSType(true);
        }

        /// <summary>
        /// Get Element Type.
        /// </summary>
        /// <param name="element"></param>
        /// <returns name="Type"></returns>
        /// <search>element, type</search>
        [NodeCategory("Query")]
        public static Element Type(Element element)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var e = element.InternalElement;

            return doc.GetElement(e.GetTypeId()).ToDSType(true);
        }

        /// <summary>
        /// Checks whether an Element is visible in given View. 
        /// </summary>
        /// <param name="element">Element to check.</param>
        /// <param name="view">View to check visibility in.</param>
        /// <returns>True if Element is visible in View, otherwise false.</returns>
        [NodeCategory("Query")]
        public static bool IsVisible(Element element, View view)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            if (view == null || !(view.InternalElement is Autodesk.Revit.DB.View v))
                throw new ArgumentNullException(nameof(view));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            var e = element.InternalElement;

            var found = new Autodesk.Revit.DB.FilteredElementCollector(doc, v.Id)
                .OfCategoryId(e.Category.Id)
                .WhereElementIsNotElementType()
                .Where(x => x.Id == e.Id);

            return found.Any();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static View OwnerView(Element element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            if (!(doc.GetElement(element.InternalElement.OwnerViewId) is Autodesk.Revit.DB.View e)) return null;

            return e.ToDSType(true) as View;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static BoundingBox BoundingBox(Element element, View view = null)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            var v = view?.InternalElement as Autodesk.Revit.DB.View;
            var bb = element.InternalElement.get_BoundingBox(v);

            return bb.ToProtoType();
        }

        /// <summary>
        /// Returns all views of given type that an element is visible in.
        /// </summary>
        /// <param name="element">Element to check.</param>
        /// <param name="viewType">View Type to check for.</param>
        /// <returns>List of views that an element is visible in.</returns>
        [NodeCategory("Query")]
        public static List<Element> AllViewsVisibleIn(Element element, string viewType = null)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            var doc = DocumentManager.Instance.CurrentDBDocument;

            List<Autodesk.Revit.DB.View> viewsToCheck;
            if (string.IsNullOrWhiteSpace(viewType))
            {
                // (Konrad) Get all valid views by Type
                var filter = new Autodesk.Revit.DB.ElementMulticlassFilter(
                    new List<Type>
                    {
                        typeof(Autodesk.Revit.DB.View3D),
                        typeof(Autodesk.Revit.DB.ViewPlan),
                        typeof(Autodesk.Revit.DB.ViewSection)
                    });

                viewsToCheck = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                    .WherePasses(filter)
                    .Cast<Autodesk.Revit.DB.View>()
                    .Where(x => !x.IsTemplate)
                    .ToList();
            }
            else
            {
                var vType = (Autodesk.Revit.DB.ViewType)Enum.Parse(typeof(Autodesk.Revit.DB.ViewType), viewType);
                if (!CheckType(vType))
                {
                    throw new ArgumentException($"{vType.ToString()} does not display elements hence is not valid for this use.");
                }

                viewsToCheck = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                    .OfClass(typeof(Autodesk.Revit.DB.View))
                    .Cast<Autodesk.Revit.DB.View>()
                    .Where(x => x.ViewType == vType && !x.IsTemplate)
                    .ToList();
            }

            return !viewsToCheck.Any() 
                ? new List<Element>() 
                : FindAllViewsWhereElementIsVisible(element, viewsToCheck);
        }

        /// <summary>
        /// Checks if element is visible in a view.
        /// </summary>
        /// <param name="element">Element to check.</param>
        /// <param name="view">View to check for.</param>
        /// <returns>True if element is visible in a view.</returns>
        [NodeCategory("Query")]
        public static bool IsVisibleInView(Element element, View view)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            return FindAllViewsWhereElementIsVisible(element,
                new List<Autodesk.Revit.DB.View> {view.InternalElement as Autodesk.Revit.DB.View}).Any();
        }

        #region Utilities

        /// <summary>
        /// Checks if Elements is visible in any of the supplied views.
        /// </summary>
        /// <param name="element">Element to check.</param>
        /// <param name="viewsToCheck">Views to check.</param>
        /// <returns>List of views that the element is visible in.</returns>
        private static List<Element> FindAllViewsWhereElementIsVisible(Element element, IEnumerable<Autodesk.Revit.DB.View> viewsToCheck)
        {
            var idsToCheck = new List<Autodesk.Revit.DB.ElementId>
            {
                new Autodesk.Revit.DB.ElementId(element.Id)
            };
            var doc = DocumentManager.Instance.CurrentDBDocument;

            return (
                from v in viewsToCheck
                let idList = new Autodesk.Revit.DB.FilteredElementCollector(doc, v.Id)
                    .WhereElementIsNotElementType()
                    .ToElementIds()
                where !idsToCheck.Except(idList).Any()
                select v.ToDSType(true)).ToList();
        }

        /// <summary>
        /// Checks if View Type is valid for checking if Element is visible in it.
        /// </summary>
        /// <param name="vt">View Type</param>
        /// <returns>True if view can have Elements.</returns>
        private static bool CheckType(Autodesk.Revit.DB.ViewType vt)
        {
            switch (vt)
            {
                case Autodesk.Revit.DB.ViewType.FloorPlan:
                case Autodesk.Revit.DB.ViewType.EngineeringPlan:
                case Autodesk.Revit.DB.ViewType.AreaPlan:
                case Autodesk.Revit.DB.ViewType.CeilingPlan:
                case Autodesk.Revit.DB.ViewType.Elevation:
                case Autodesk.Revit.DB.ViewType.Section:
                case Autodesk.Revit.DB.ViewType.Detail:
                case Autodesk.Revit.DB.ViewType.ThreeD:
                case Autodesk.Revit.DB.ViewType.DraftingView:
                    return true;
                case Autodesk.Revit.DB.ViewType.DrawingSheet:
                case Autodesk.Revit.DB.ViewType.Undefined:
                case Autodesk.Revit.DB.ViewType.Schedule:
                case Autodesk.Revit.DB.ViewType.Legend:
                case Autodesk.Revit.DB.ViewType.Report:
                case Autodesk.Revit.DB.ViewType.ProjectBrowser:
                case Autodesk.Revit.DB.ViewType.SystemBrowser:
                case Autodesk.Revit.DB.ViewType.CostReport:
                case Autodesk.Revit.DB.ViewType.LoadsReport:
                case Autodesk.Revit.DB.ViewType.PresureLossReport:
                case Autodesk.Revit.DB.ViewType.PanelSchedule:
                case Autodesk.Revit.DB.ViewType.ColumnSchedule:
                case Autodesk.Revit.DB.ViewType.Walkthrough:
                case Autodesk.Revit.DB.ViewType.Rendering:
#if !Revit2018
                case Autodesk.Revit.DB.ViewType.SystemsAnalysisReport:
#endif
                case Autodesk.Revit.DB.ViewType.Internal:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion
    }
}
