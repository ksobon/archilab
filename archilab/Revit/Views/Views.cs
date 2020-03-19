using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using DynamoServices;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Revit.Elements;
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;
using Element = Revit.Elements.Element;
using View = Revit.Elements.Views.View;
using Workset = archilab.Revit.Elements.Workset;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Views
{
    /// <summary>
    /// Wrapper class for Views.
    /// </summary>
    [RegisterForTrace]
    public class Views
    {
        internal Views()
        {
        }

        /// <summary>
        /// Remove view filter from view.
        /// </summary>
        /// <param name="view">View to remove view filter from.</param>
        /// <param name="viewFilter">View filter to be removed.</param>
        /// <returns name="view">View that filter was removed from.</returns>
        /// <search>view, filter, remove, delete</search>
        public static View RemoveFilter(View view, List<Element> viewFilter)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var v = (Autodesk.Revit.DB.View)view.InternalElement;

            // get all filters for a view
            var ids = v.GetFilters();

            TransactionManager.Instance.EnsureInTransaction(doc);
            foreach (var element in viewFilter)
            {
                var pfe = (Autodesk.Revit.DB.ParameterFilterElement)element.InternalElement;
                if (ids.Contains(pfe.Id))
                {
                    v.RemoveFilter(pfe.Id);
                }
            }
            TransactionManager.Instance.TransactionTaskDone();

            return view;
        }

        /// <summary>
        /// Get View Template applied to view.
        /// </summary>
        /// <param name="view">View to retrieve View Template from.</param>
        /// <returns name="view">View Template applied to view.</returns>
        /// <search>view, template</search>
        public static object ViewTemplate(View view)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var v = (Autodesk.Revit.DB.View)view.InternalElement;

            var id = v.ViewTemplateId;
            return id != Autodesk.Revit.DB.ElementId.InvalidElementId ? doc.GetElement(id).ToDSType(true) : null;
        }

        /// <summary>
        /// Set View Template for a View.
        /// </summary>
        /// <param name="view">View that template will be applied to.</param>
        /// <param name="viewTemplate">View Template that will be applied to View.</param>
        /// <returns name="view"></returns>
        /// <search>set, view, template</search>
        public static View SetViewTemplate(View view, View viewTemplate)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var v = (Autodesk.Revit.DB.View)view.InternalElement;
            var vt = (Autodesk.Revit.DB.View)viewTemplate.InternalElement;

            if (v.IsValidViewTemplate(vt.Id))
            {
                TransactionManager.Instance.EnsureInTransaction(doc);
                v.ViewTemplateId = vt.Id;
                TransactionManager.Instance.TransactionTaskDone();
            }
            else
            {
                throw new Exception("Specified View Template is not valid for this View.");
            }

            return view;
        }

        /// <summary>
        /// Removes View Template from given view.
        /// </summary>
        /// <param name="view">View to remove View Template from.</param>
        /// <returns name="view">View that template was removed from.</returns>
        /// <search>view, template, remove, delete</search>
        public static View RemoveViewTemplate(View view)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var v = (Autodesk.Revit.DB.View)view.InternalElement;

            try
            {
                // set "View Template" parameter to -1 to remove template
                TransactionManager.Instance.EnsureInTransaction(doc);
                var bip = v.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.VIEW_TEMPLATE_FOR_SCHEDULE);
                bip.Set(new Autodesk.Revit.DB.ElementId(-1));
                TransactionManager.Instance.TransactionTaskDone();
            }
            catch (Exception)
            {
                // ignored
            }

            return view;
        }

        /// <summary>
        /// Get all views by type.
        /// </summary>
        /// <param name="viewType">View type to retrieve all views for. If View Template selected, 
        /// 3D View Templates will be excluded from returned View Templates (currently a Dynamo limitation).</param>
        /// <returns name="view">Views that match view type.</returns>
        /// <search>view, get all views, view type</search>
        public static List<Element> GetByType(string viewType)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var vList = new List<Element>();

            if (viewType != "View Template")
            {
                var vType = (Autodesk.Revit.DB.ViewType)Enum.Parse(typeof(Autodesk.Revit.DB.ViewType), viewType);

                var allViews = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                    .OfClass(typeof(Autodesk.Revit.DB.View))
                    .Cast<Autodesk.Revit.DB.View>()
                    .Where(x => x.ViewType == vType && !x.IsTemplate)
                    .ToList();

                if (allViews.Count > 0)
                {
                    vList = allViews.Select(x => x.ToDSType(true)).ToList();
                }
            }
            else
            {
                var vTemplates = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                    .OfClass(typeof(Autodesk.Revit.DB.View))
                    .Cast<Autodesk.Revit.DB.View>()
                    .Where(x => x.IsTemplate && x.ViewType != Autodesk.Revit.DB.ViewType.ThreeD)
                    .ToList();

                if (vTemplates.Count > 0)
                {
                    vList = vTemplates.Select(x => x.ToDSType(true)).ToList();
                }
            }

            return vList;
        }

        /// <summary>
        /// Check if Schedule is Titleblock Schedule.
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public static bool IsTitleblockSchedule(Element view)
        {
            try
            {
                // cast to View Schedule, titleblock schedules will fail here
                var v = (Autodesk.Revit.DB.ViewSchedule)view.InternalElement;
                return v.IsTitleblockRevisionSchedule;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if View is placed on a Sheet.
        /// </summary>
        /// <param name="view">View to check.</param>
        /// <returns>True if View is on Sheet, otherwise False.</returns>
        /// <search>isOnSheet, is on sheet</search>
        public static bool IsOnSheet(View view)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var v = (Autodesk.Revit.DB.View)view.InternalElement;

            switch (v.ViewType)
            {
                case Autodesk.Revit.DB.ViewType.Undefined:
                case Autodesk.Revit.DB.ViewType.ProjectBrowser:
                case Autodesk.Revit.DB.ViewType.SystemBrowser:
                case Autodesk.Revit.DB.ViewType.Internal:
                case Autodesk.Revit.DB.ViewType.DrawingSheet:
                    return false;
                case Autodesk.Revit.DB.ViewType.FloorPlan:
                case Autodesk.Revit.DB.ViewType.EngineeringPlan:
                case Autodesk.Revit.DB.ViewType.AreaPlan:
                case Autodesk.Revit.DB.ViewType.CeilingPlan:
                case Autodesk.Revit.DB.ViewType.Elevation:
                case Autodesk.Revit.DB.ViewType.Section:
                case Autodesk.Revit.DB.ViewType.Detail:
                case Autodesk.Revit.DB.ViewType.ThreeD:
                case Autodesk.Revit.DB.ViewType.DraftingView:
                case Autodesk.Revit.DB.ViewType.Legend:
                case Autodesk.Revit.DB.ViewType.Report:
                case Autodesk.Revit.DB.ViewType.CostReport:
                case Autodesk.Revit.DB.ViewType.LoadsReport:
                case Autodesk.Revit.DB.ViewType.PresureLossReport:
                case Autodesk.Revit.DB.ViewType.Walkthrough:
                case Autodesk.Revit.DB.ViewType.Rendering:
                    
                    var sheet = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                        .OfClass(typeof(Autodesk.Revit.DB.ViewSheet))
                        .Cast<Autodesk.Revit.DB.ViewSheet>()
                        .FirstOrDefault(x => x.GetAllPlacedViews().FirstOrDefault(y => y == v.Id) != null);

                    return sheet != null;
                case Autodesk.Revit.DB.ViewType.Schedule:
                case Autodesk.Revit.DB.ViewType.PanelSchedule:
                case Autodesk.Revit.DB.ViewType.ColumnSchedule:

                    var schedule = v as Autodesk.Revit.DB.ViewSchedule;
                    if(schedule == null) throw new ArgumentException("Invalid View");

                    var sheetSchedule = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                        .OfClass(typeof(Autodesk.Revit.DB.ScheduleSheetInstance))
                        .Cast<Autodesk.Revit.DB.ScheduleSheetInstance>()
                        .FirstOrDefault(x => !x.IsTitleblockRevisionSchedule && x.ScheduleId == v.Id);

                    return sheetSchedule != null;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Sets Workset visibility for a View.
        /// </summary>
        /// <param name="view">View to set the visibility on.</param>
        /// <param name="worksets">Worksets to set the visibility for.</param>
        /// <param name="visibility">Visibility setting. Ex: Hide.</param>
        /// <returns name="view">View</returns>
        public static View SetWorksetVisibility(View view, List<Workset> worksets, string visibility)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var v = (Autodesk.Revit.DB.View)view.InternalElement;
            var vis = (Autodesk.Revit.DB.WorksetVisibility)Enum.Parse(typeof(Autodesk.Revit.DB.WorksetVisibility), visibility);

            TransactionManager.Instance.EnsureInTransaction(doc);
            foreach (var w in worksets)
            {
                v.SetWorksetVisibility(w.InternalWorkset.Id, vis);
            }
            TransactionManager.Instance.TransactionTaskDone();

            return view;
        }

        /// <summary>
        /// Duplicates an existing view.
        /// </summary>
        /// <param name="view">View to duplicate.</param>
        /// <param name="name">Name to be assigned to new view.</param>
        /// <param name="options">Duplicate options. Ex: Duplicate as Dependent.</param>
        /// <returns name="view">New View.</returns>
        public static View Duplicate(View view, string name, string options)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var v = (Autodesk.Revit.DB.View)view.InternalElement;
            var dupOptions = (Autodesk.Revit.DB.ViewDuplicateOption)Enum.Parse(typeof(Autodesk.Revit.DB.ViewDuplicateOption), options);

            TransactionManager.Instance.EnsureInTransaction(doc);
            var newView = doc.GetElement(v.Duplicate(dupOptions));
            newView.Name = name;
            TransactionManager.Instance.TransactionTaskDone();

            return newView.ToDSType(true) as View;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="viewFamilyType"></param>
        /// <param name="extents"></param>
        /// <returns></returns>
        public static View CreateCallout(View view,
            Element viewFamilyType, Autodesk.DesignScript.Geometry.Rectangle extents)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var v = (Autodesk.Revit.DB.View)view.InternalElement;

            var pt1 = extents.BoundingBox.MinPoint.ToXyz();
            var pt2 = extents.BoundingBox.MaxPoint.ToXyz();

            Autodesk.Revit.DB.View newView;

            TransactionManager.Instance.EnsureInTransaction(doc);
            switch (v.ViewType)
            {
                case Autodesk.Revit.DB.ViewType.FloorPlan:
                case Autodesk.Revit.DB.ViewType.CeilingPlan:
                case Autodesk.Revit.DB.ViewType.Elevation:
                case Autodesk.Revit.DB.ViewType.Section:
                case Autodesk.Revit.DB.ViewType.Detail:
                    newView = Autodesk.Revit.DB.ViewSection.CreateCallout(doc, v.Id,
                        viewFamilyType.InternalElement.Id, pt1, pt2);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(view));
            }
            TransactionManager.Instance.TransactionTaskDone();

            return (View)newView.ToDSType(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="referenceView"></param>
        /// <param name="extents"></param>
        /// <returns></returns>
        public static View CreateReferenceCallout(View view, View referenceView, 
            Autodesk.DesignScript.Geometry.Rectangle extents)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var v = (Autodesk.Revit.DB.View)view.InternalElement;
            var rv = (Autodesk.Revit.DB.View)referenceView.InternalElement;

            var pt1 = extents.BoundingBox.MinPoint.ToXyz();
            var pt2 = extents.BoundingBox.MaxPoint.ToXyz();

            TransactionManager.Instance.EnsureInTransaction(doc);
            switch (v.ViewType)
            {
                case Autodesk.Revit.DB.ViewType.FloorPlan:
                case Autodesk.Revit.DB.ViewType.CeilingPlan:
                case Autodesk.Revit.DB.ViewType.Elevation:
                case Autodesk.Revit.DB.ViewType.Section:
                case Autodesk.Revit.DB.ViewType.Detail:
                    Autodesk.Revit.DB.ViewSection.CreateReferenceCallout(doc, v.Id, rv.Id, pt1, pt2);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(view));
            }
            TransactionManager.Instance.TransactionTaskDone();

            return view;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callout"></param>
        /// <param name="reference"></param>
        /// <returns></returns>
        public static Element ChangeReferencedView(Element callout, View reference)
        {
            if (callout == null) throw new ArgumentNullException(nameof(callout));
            if (reference == null) throw new ArgumentNullException(nameof(reference));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            TransactionManager.Instance.EnsureInTransaction(doc);
            Autodesk.Revit.DB.ReferenceableViewUtils.ChangeReferencedView(doc, callout.InternalElement.Id, reference.InternalElement.Id);
            TransactionManager.Instance.TransactionTaskDone();

            return callout;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public static List<Element> GetReferenceCallouts(View view)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));
            if (!(view.InternalElement is Autodesk.Revit.DB.View v))
                throw new ArgumentException("View is not a valid type.");

            var doc = DocumentManager.Instance.CurrentDBDocument;

            return v.GetReferenceCallouts().Select(x => doc.GetElement(x).ToDSType(true)).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public static Autodesk.DesignScript.Geometry.Rectangle Outline(View view)
        {
            var v = (Autodesk.Revit.DB.View)view.InternalElement;
            if (v == null) throw new ArgumentNullException(nameof(view));

            
            var o = v.Outline;
            var pt1 = new Autodesk.Revit.DB.XYZ(o.Min.U, o.Min.V, 0);
            var pt2 = new Autodesk.Revit.DB.XYZ(o.Max.U, o.Min.V, 0);
            var pt3 = new Autodesk.Revit.DB.XYZ(o.Max.U, o.Max.V, 0);
            var pt4 = new Autodesk.Revit.DB.XYZ(o.Min.U, o.Max.V, 0);

            return Autodesk.DesignScript.Geometry.Rectangle.ByCornerPoints(pt1.ToPoint(), pt2.ToPoint(), pt3.ToPoint(), pt4.ToPoint());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public static Autodesk.DesignScript.Geometry.BoundingBox CropBox(View view)
        {
            if (!(view.InternalElement is Autodesk.Revit.DB.View v))
                throw new ArgumentNullException(nameof(view));

            var cb = v.CropBox;
            var min = cb.Min.ToPoint();
            var max = cb.Max.ToPoint();

            return Autodesk.DesignScript.Geometry.BoundingBox.ByCorners(min, max);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="boundingBox"></param>
        /// <returns></returns>
        public static View SetCropBox(View view, Autodesk.DesignScript.Geometry.BoundingBox boundingBox)
        {
            if (!(view.InternalElement is Autodesk.Revit.DB.View v))
                throw new ArgumentNullException(nameof(view));

            if (boundingBox == null)
                throw new ArgumentNullException(nameof(boundingBox));

            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);
            v.CropBoxActive = true;
            v.CropBoxVisible = true;
            v.CropBox = boundingBox.ToRevitType();
            TransactionManager.Instance.TransactionTaskDone();

            return view;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static View SetName(View view, string name)
        {
            if (view == null)
                throw new ArgumentException(nameof(view));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));

            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);
            view.InternalElement.Name = name;
            TransactionManager.Instance.TransactionTaskDone();

            return view;
        }

        #region Utilities

        /// <summary>
        /// Get Null
        /// </summary>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static object GetNull()
        {
            return null;
        }

        #endregion
    }
}
