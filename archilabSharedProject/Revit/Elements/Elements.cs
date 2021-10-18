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
using Category = Revit.Elements.Category;
using Element = Revit.Elements.Element;
using Point = Autodesk.DesignScript.Geometry.Point;
using View = Revit.Elements.Views.View;
// ReSharper disable UnusedMember.Global

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
        /// <param name="translation"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static List<Element> MoveElements(List<Element> elements, Vector translation)
        {
            if (elements == null || !elements.Any())
                throw new ArgumentNullException(nameof(elements));
            if (translation == null)
                throw new ArgumentNullException(nameof(translation));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            TransactionManager.Instance.EnsureInTransaction(doc);
            Autodesk.Revit.DB.ElementTransformUtils.MoveElements(doc,
                elements.Select(x => x.InternalElement.Id).ToList(), translation.ToXyz());
            TransactionManager.Instance.TransactionTaskDone();

            return elements;
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
            foreach (var e in elements)
            {
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
        public static List<Element> SubComponents(Element element)
        {
            var e = element.InternalElement;
            var subComponents = new List<Autodesk.Revit.DB.Element>();

            GetSubComponents(e, ref subComponents);

            return subComponents.Select(x => x.ToDSType(true)).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static bool IsSubComponent(Element element)
        {
            var e = element.InternalElement;
            if (e is Autodesk.Revit.DB.FamilyInstance fi)
                return fi.SuperComponent != null;

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="includeSubComponents"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static Point GetCentroid(Element element, bool includeSubComponents = true)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            var rvtElement = element.InternalElement;
            var elements = new List<Autodesk.Revit.DB.Element> { rvtElement };
            if (includeSubComponents)
                GetSubComponents(rvtElement, ref elements);

            var opt = new Autodesk.Revit.DB.Options
            {
                DetailLevel = Autodesk.Revit.DB.ViewDetailLevel.Fine,
                IncludeNonVisibleObjects = true,
                ComputeReferences = true
            };

            var centroids = new List<Autodesk.Revit.DB.XYZ>();
            foreach (var e in elements)
            {
                var geo = e.get_Geometry(opt);
                if (geo == null)
                    continue;

                var a = new List<Autodesk.Revit.DB.XYZ>();
                Autodesk.Revit.DB.GeometryInstance inst = null;
                foreach (var obj in geo)
                {
                    switch (obj)
                    {
                        case Autodesk.Revit.DB.GeometryInstance geometryInstance:
                            inst = geometryInstance;
                            break;
                        case Autodesk.Revit.DB.Solid solid:
                            if (solid.Faces.Size > 0 && solid.Volume > 0)
                            {
                                try
                                {
                                    var c = solid.ComputeCentroid();
                                    a.Add(c);
                                }
                                catch
                                {
                                    // ignore
                                }
                            }
                            break;
                        default:
                            continue;
                    }
                }

                if (a.Count == 0 && inst != null)
                {
                    geo = inst.GetInstanceGeometry();

                    foreach (var obj in geo)
                    {
                        var s = obj as Autodesk.Revit.DB.Solid;

                        if (s == null || s.Faces.Size <= 0 || s.Volume <= 0)
                            continue;
                        
                        try
                        {
                            var c = s.ComputeCentroid();
                            a.Add(c);
                        }
                        catch
                        {
                            // ignore
                        }
                    }
                }

                if (!a.Any())
                    continue;

                var finalSubCentroid = new Autodesk.Revit.DB.XYZ();
                foreach (var centroidVolume in a)
                    finalSubCentroid += centroidVolume;

                finalSubCentroid = finalSubCentroid /= a.Count;
                centroids.Add(finalSubCentroid);
            }

            var finalCentroid = new Autodesk.Revit.DB.XYZ();
            if (includeSubComponents)
            {
                foreach (var centroid in centroids)
                    finalCentroid += centroid;

                finalCentroid = finalCentroid /= centroids.Count;
            }
            else
            {
                // (Konrad) We can grab existing centroid. There should be one only.
                finalCentroid = centroids.FirstOrDefault();
            }

            return finalCentroid?.ToPoint();
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
        /// <param name="view"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static bool IsHidden(Element element, View view)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            if (view == null || !(view.InternalElement is Autodesk.Revit.DB.View v))
                throw new ArgumentNullException(nameof(view));

            return element.InternalElement.IsHidden(v);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        [MultiReturn("Point", "Line")]
        public static Dictionary<string, object> Location(Element element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            var loc = element.InternalElement.Location;
            if (loc == null)
                return new Dictionary<string, object>();

            Curve line = null;
            Point pt = null;
            switch (loc)
            {
                case Autodesk.Revit.DB.LocationCurve locationCurve:
                    line = locationCurve.Curve.ToProtoType();
                    break;
                case Autodesk.Revit.DB.LocationPoint locationPoint:
                    pt = locationPoint.Point.ToPoint();
                    break;
                default:
                    break;
            }

            return new Dictionary<string, object>
            {
                {"Point", pt},
                {"Line", line}
            };
        }

        #region Utilities

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="subComponents"></param>
        private static void GetSubComponents(Autodesk.Revit.DB.Element element, ref List<Autodesk.Revit.DB.Element> subComponents)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            switch (element)
            {
                case Autodesk.Revit.DB.FamilyInstance fi:
                    var subs = fi.GetSubComponentIds().Select(x => doc.GetElement(x)).ToList();
                    if (subs.Any())
                    {
                        subComponents.AddRange(subs);
                        foreach (var sub in subs)
                        {
                            GetSubComponents(sub, ref subComponents);
                        }
                    }
                    break;
                case Autodesk.Revit.DB.Architecture.Stairs s:
                    var stairComponents = s.GetStairsLandings().Select(x => doc.GetElement(x)).ToList();
                    stairComponents.AddRange(s.GetStairsRuns().Select(x => doc.GetElement(x)));
                    stairComponents.AddRange(s.GetStairsSupports().Select(x => doc.GetElement(x)));
                    if (stairComponents.Any())
                    {
                        subComponents.AddRange(stairComponents);
                        foreach (var stairComponent in stairComponents)
                        {
                            GetSubComponents(stairComponent, ref subComponents);
                        }
                    }
                    break;
                case Autodesk.Revit.DB.Architecture.Railing r:
                    var railComponents = r.GetHandRails().Select(x => doc.GetElement(x)).ToList();
                    railComponents.Add(doc.GetElement(r.TopRail));
                    if (railComponents.Any())
                    {
                        subComponents.AddRange(railComponents);
                        foreach (var railComponent in railComponents)
                        {
                            GetSubComponents(railComponent, ref subComponents);
                        }
                    }
                    break;
                case Autodesk.Revit.DB.BeamSystem b:
                    var beams = b.GetBeamIds().Select(x => doc.GetElement(x)).ToList();
                    if (beams.Any())
                    {
                        subComponents.AddRange(beams);
                        foreach (var beam in beams)
                        {
                            GetSubComponents(beam, ref subComponents);
                        }
                    }
                    break;
            }
        }

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
#if !Revit2018 && !Revit2019
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

    //internal class CentroidVolume
    //{
    //    public Autodesk.Revit.DB.XYZ Centroid { get; set; } = Autodesk.Revit.DB.XYZ.Zero;
    //    public double Volume { get; set; }

    //    public static CentroidVolume GetCentroid2(Autodesk.Revit.DB.Solid solid)
    //    {
    //        var cv = new CentroidVolume
    //        {
    //            Centroid = solid.ComputeCentroid(), 
    //            Volume = solid.Volume
    //        };
    //        return cv;
    //    }

    //    public static CentroidVolume GetCentroid(Autodesk.Revit.DB.Solid solid)
    //    {
    //        var cv = new CentroidVolume();
    //        var controls = new Autodesk.Revit.DB.SolidOrShellTessellationControls
    //        {
    //            LevelOfDetail = 0
    //        };

    //        Autodesk.Revit.DB.TriangulatedSolidOrShell triangulation;

    //        try
    //        {
    //            triangulation = Autodesk.Revit.DB.SolidUtils.TessellateSolidOrShell(solid, controls);
    //        }
    //        catch (Autodesk.Revit.Exceptions.InvalidOperationException)
    //        {
    //            return null;
    //        }

    //        var n = triangulation.ShellComponentCount;

    //        for (var i = 0; i < n; ++i)
    //        {
    //            var component = triangulation.GetShellComponent(i);

    //            var m = component.TriangleCount;

    //            for (var j = 0; j < m; ++j)
    //            {
    //                var t = component.GetTriangle(j);

    //                var v0 = component.GetVertex(t.VertexIndex0);
    //                var v1 = component.GetVertex(t.VertexIndex1);
    //                var v2 = component.GetVertex(t.VertexIndex2);

    //                var v = v0.X * (v1.Y * v2.Z - v2.Y * v1.Z)
    //                        + v0.Y * (v1.Z * v2.X - v2.Z * v1.X)
    //                        + v0.Z * (v1.X * v2.Y - v2.X * v1.Y);

    //                cv.Centroid += v * (v0 + v1 + v2);
    //                cv.Volume += v;
    //            }
    //        }

    //        cv.Centroid /= 4 * cv.Volume;

    //        var diffCentroid = cv.Centroid - solid.ComputeCentroid();

    //        Debug.Assert(0.6 > diffCentroid.GetLength(),
    //            "expected centroid approximation to be similar to solid ComputeCentroid result");

    //        cv.Volume /= 6;

    //        var diffVolume = cv.Volume - solid.Volume;

    //        Debug.Assert(0.3 > Math.Abs(diffVolume / cv.Volume),
    //            "expected volume approximation to be similar to solid Volume property value");

    //        return cv;
    //    }

    //    public override string ToString()
    //    {
    //        return Volume + "@" + Centroid;
    //    }
    //}
}
