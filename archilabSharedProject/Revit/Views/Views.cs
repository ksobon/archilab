using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using DynamoServices;
using Dynamo.Graph.Nodes;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Revit.Elements;
using Revit.Elements.Views;
using archilab.Revit.Elements;
using archilab.Utilities;
using Autodesk.DesignScript.Runtime;
using NUnit.Framework;
using Revit.Filter;
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

        #region Action

        /// <summary>
        /// Remove view filter from view.
        /// </summary>
        /// <param name="view">View to remove view filter from.</param>
        /// <param name="viewFilter">View filter to be removed.</param>
        /// <returns name="view">View that filter was removed from.</returns>
        /// <search>view, filter, remove, delete</search>
        [NodeCategory("Action")]
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
        /// 
        /// </summary>
        /// <param name="views"></param>
        /// <param name="viewFilter"></param>
        /// <param name="overrides"></param>
        /// <param name="show"></param>
        /// <param name="isEnabled"></param>
        /// <returns></returns>
        public static List<View> SetFilterOverrides(List<View> views, Element viewFilter,
            OverrideGraphicsSettings overrides, bool show = true, bool isEnabled = true)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var rvtViews = views.Select(x => (Autodesk.Revit.DB.View) x.InternalElement).ToList();
            var rvtFilter = (Autodesk.Revit.DB.ParameterFilterElement) viewFilter.InternalElement;

            TransactionManager.Instance.EnsureInTransaction(doc);
            foreach (var v in rvtViews)
            {
                v.SetFilterOverrides(rvtFilter.Id, overrides.InternalOverrideGraphicSettings);
                v.SetFilterVisibility(rvtFilter.Id, show);
                v.SetIsFilterEnabled(rvtFilter.Id, isEnabled);
            }
            TransactionManager.Instance.TransactionTaskDone();

            return views;
        }

        /// <summary>
        /// Set View Template for a View.
        /// </summary>
        /// <param name="view">View that template will be applied to.</param>
        /// <param name="viewTemplate">View Template that will be applied to View.</param>
        /// <returns name="view"></returns>
        /// <search>set, view, template</search>
        [NodeCategory("Action")]
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
        [NodeCategory("Action")]
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
        /// 
        /// </summary>
        /// <param name="except"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static List<string> GetAllViewTypes(List<string> except)
        {
            return except.Any() 
                ? new ViewTypes().Types.Values.Where(x => !except.Contains(x)).ToList() 
                : new ViewTypes().Types.Values.ToList();
        }

        /// <summary>
        /// Get all views by type.
        /// </summary>
        /// <param name="viewType">View type to retrieve all views for. If View Template selected, 
        /// 3D View Templates will be excluded from returned View Templates (currently a Dynamo limitation).</param>
        /// <returns name="view">Views that match view type.</returns>
        /// <search>view, get all views, view type</search>
        [NodeCategory("Action")]
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
        /// Sets Workset visibility for a View.
        /// </summary>
        /// <param name="view">View to set the visibility on.</param>
        /// <param name="worksets">Worksets to set the visibility for.</param>
        /// <param name="visibility">Visibility setting. Ex: Hide.</param>
        /// <returns name="view">View</returns>
        /// <search>workset, visibility, set</search>
        [NodeCategory("Action")]
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
        /// <search>view, duplicate</search>
        [NodeCategory("Action")]
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
        /// <param name="options"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static View Duplicate(View view, string options)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var v = (Autodesk.Revit.DB.View)view.InternalElement;
            var dupOptions = (Autodesk.Revit.DB.ViewDuplicateOption)Enum.Parse(typeof(Autodesk.Revit.DB.ViewDuplicateOption), options);

            TransactionManager.Instance.EnsureInTransaction(doc);
            var newView = doc.GetElement(v.Duplicate(dupOptions));
            TransactionManager.Instance.TransactionTaskDone();

            return newView.ToDSType(true) as View;
        }

        /// <summary>
        /// Creates a new View Callout.
        /// </summary>
        /// <param name="view">View to create the Callout in.</param>
        /// <param name="viewFamilyType">Type of Callout Family.</param>
        /// <param name="extents">Extents of the Callout as Rectangle.</param>
        /// <returns name="view">New Callout View.</returns>
        /// <search>view, create, callout</search>
        [NodeCategory("Action")]
        public static View CreateCallout(View view, Element viewFamilyType, Rectangle extents)
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
                case Autodesk.Revit.DB.ViewType.EngineeringPlan:
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
        /// Creates a new View Callout.
        /// </summary>
        /// <param name="view">View to create the Callout in.</param>
        /// <param name="referenceView">View to set as Reference.</param>
        /// <param name="extents">Extents of the Callout as Rectangle.</param>
        /// <returns name="view">New Callout View.</returns>
        /// <search>view, create, callout, reference</search>
        [NodeCategory("Action")]
        public static View CreateReferenceCallout(View view, View referenceView, Rectangle extents)
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
        /// Changes the Referenced View for a Callout.
        /// </summary>
        /// <param name="callout">Callout to change the Referenced View for.</param>
        /// <param name="reference">View to set the Reference to.</param>
        /// <returns name="callout">Callout.</returns>
        /// <search>view, reference, change, callout</search>
        [NodeCategory("Action")]
        public static Element ChangeReferencedView(Element callout, View reference)
        {
            if (callout == null)
                throw new ArgumentNullException(nameof(callout));
            if (reference == null)
                throw new ArgumentNullException(nameof(reference));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            TransactionManager.Instance.EnsureInTransaction(doc);
            Autodesk.Revit.DB.ReferenceableViewUtils.ChangeReferencedView(doc, callout.InternalElement.Id, reference.InternalElement.Id);
            TransactionManager.Instance.TransactionTaskDone();

            return callout;
        }

        /// <summary>
        /// Sets View's Crop Box to size matching supplied Bounding Box.
        /// </summary>
        /// <param name="view">View to set the Crop Box for.</param>
        /// <param name="boundingBox">Bounding Box representing new Crop Box extents.</param>
        /// <returns name="view">View.</returns>
        /// <search>view, set, crop box</search>
        [NodeCategory("Action")]
        public static View SetCropBox(View view, BoundingBox boundingBox)
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
        /// <param name="visible"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static View CropBoxVisible(View view, bool visible = true)
        {
            if (!(view.InternalElement is Autodesk.Revit.DB.View v))
                throw new ArgumentNullException(nameof(view));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            TransactionManager.Instance.EnsureInTransaction(doc);
            v.CropBoxVisible = visible;
            TransactionManager.Instance.TransactionTaskDone();

            return view;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="curves"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static View SetCropBoxByCurves(View view, List<Curve> curves)
        {
            if (!(view.InternalElement is Autodesk.Revit.DB.View v))
                throw new ArgumentNullException(nameof(view));
            if (curves == null || !curves.Any())
                throw new ArgumentNullException(nameof(curves));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            var shapeManager = v.GetCropRegionShapeManager();
            var cLoop = new Autodesk.Revit.DB.CurveLoop();
            foreach (var curve in curves)
            {
                cLoop.Append(curve.ToRevitType());
            }

            TransactionManager.Instance.EnsureInTransaction(doc);
            v.CropBoxActive = true;
            v.CropBoxVisible = true;
            shapeManager.SetCropShape(cLoop);
            TransactionManager.Instance.TransactionTaskDone();

            return view;
        }

        /// <summary>
        /// Changes View's Name to a new one.
        /// </summary>
        /// <param name="view">View to change the name for.</param>
        /// <param name="name">New name for the View.</param>
        /// <returns name="view">View with a new Name.</returns>
        /// <search>set, name</search>
        [NodeCategory("Action")]
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="detailLevel"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static View SetDetailLevel(View view, string detailLevel)
        {
            if (view == null)
                throw new ArgumentException(nameof(view));
            if (string.IsNullOrWhiteSpace(detailLevel))
                throw new ArgumentException(nameof(detailLevel));

            var dl = (Autodesk.Revit.DB.ViewDetailLevel)Enum.Parse(typeof(Autodesk.Revit.DB.ViewDetailLevel), detailLevel);

            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);
            view.InternalElement.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.VIEW_DETAIL_LEVEL)?.Set((int) dl);
            TransactionManager.Instance.TransactionTaskDone();

            return view;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="displayStyle"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static View SetDisplayStyle(View view, string displayStyle)
        {
            if (view == null)
                throw new ArgumentException(nameof(view));
            if (string.IsNullOrWhiteSpace(displayStyle))
                throw new ArgumentException(nameof(displayStyle));

            var ds = (Autodesk.Revit.DB.DisplayStyle)Enum.Parse(typeof(Autodesk.Revit.DB.DisplayStyle), displayStyle);

            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);
            ((Autodesk.Revit.DB.View)view.InternalElement).DisplayStyle = ds;
            TransactionManager.Instance.TransactionTaskDone();

            return view;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="crop"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static View CropView(View view, bool crop = false)
        {
            if (view == null)
                throw new ArgumentException(nameof(view));

            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);
            view.InternalElement.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.VIEWER_CROP_REGION)
                ?.Set(crop ? 1 : 0);
            TransactionManager.Instance.TransactionTaskDone();

            return view;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="crop"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static View AnnotationCrop(View view, bool crop = false)
        {
            if (view == null)
                throw new ArgumentException(nameof(view));

            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);
            view.InternalElement.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.VIEWER_ANNOTATION_CROP_ACTIVE)
                ?.Set(crop ? 1 : 0);
            TransactionManager.Instance.TransactionTaskDone();

            return view;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="visible"></param>
        [NodeCategory("Action")]
        public static View SetImportsInFamiliesVisibility(View view, bool visible = false)
        {
            if (view == null)
                throw new ArgumentException(nameof(view));

            var v = (Autodesk.Revit.DB.View)view.InternalElement;
            var doc = DocumentManager.Instance.CurrentDBDocument;

            // (Konrad) Imports in Families are under Imported Categories, but is actually a Graphics Style
            var allStyles = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                .OfClass(typeof(Autodesk.Revit.DB.GraphicsStyle))
                .Cast<Autodesk.Revit.DB.GraphicsStyle>()
                .ToList();

            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);
            foreach (var style in allStyles)
            {
                if (style.Name != "Imports in Families")
                    continue;

                var iif = style.GraphicsStyleCategory;
                if (iif.get_AllowsVisibilityControl(v))
                    iif.set_Visible(v, visible);

                break;
            }
            TransactionManager.Instance.TransactionTaskDone();

            return view;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="category"></param>
        /// <param name="visible"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static View SetImportedCategoriesVisibility(View view, Category category, bool visible = false)
        {
            if (view == null)
                throw new ArgumentException(nameof(view));
            if (category == null)
                throw new ArgumentException(nameof(category));

            var v = (Autodesk.Revit.DB.View)view.InternalElement;
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var c = Autodesk.Revit.DB.Category.GetCategory(doc, new Autodesk.Revit.DB.ElementId(category.Id));

            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);
            if (c.get_AllowsVisibilityControl(v))
                c.set_Visible(v, visible);
            TransactionManager.Instance.TransactionTaskDone();

            return view;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static List<bool> HideElements(View view, List<Element> elements)
        {
            if (view == null)
                throw new ArgumentException(nameof(view));

            var results = new List<bool>();
            var v = (Autodesk.Revit.DB.View)view.InternalElement;
            var ids = new List<Autodesk.Revit.DB.ElementId>();

            foreach (var element in elements)
            {
                var e = element.InternalElement;
                if (!e.IsHidden(v) && e.CanBeHidden(v))
                {
                    ids.Add(e.Id);
                    results.Add(true);
                }
                else
                {
                    results.Add(false);
                }
            }

            if (!ids.Any())
                return results;

            var doc = DocumentManager.Instance.CurrentDBDocument;
            TransactionManager.Instance.EnsureInTransaction(doc);
            v.HideElements(ids);
            TransactionManager.Instance.TransactionTaskDone();

            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static List<bool> UnHideElements(View view, List<Element> elements)
        {
            if (view == null)
                throw new ArgumentException(nameof(view));

            var results = new List<bool>();
            var v = (Autodesk.Revit.DB.View)view.InternalElement;
            var ids = new List<Autodesk.Revit.DB.ElementId>();

            foreach (var element in elements)
            {
                var e = element.InternalElement;
                if (e.IsHidden(v))
                {
                    ids.Add(e.Id);
                    results.Add(true);
                }
                else
                {
                    results.Add(false);
                }
            }

            if (!ids.Any())
                return results;

            var doc = DocumentManager.Instance.CurrentDBDocument;
            TransactionManager.Instance.EnsureInTransaction(doc);
            v.UnhideElements(ids);
            TransactionManager.Instance.TransactionTaskDone();

            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="category"></param>
        /// <param name="overrides"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static View SetCategoryOverrides(View view, Category category, OverrideGraphicsSettings overrides)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));
            if (category == null)
                throw new ArgumentNullException(nameof(category));
            if (overrides == null)
                throw new ArgumentNullException(nameof(overrides));

            var v = (Autodesk.Revit.DB.View)view.InternalElement;
            var o = overrides.InternalOverrideGraphicSettings;

            var doc = DocumentManager.Instance.CurrentDBDocument;
            TransactionManager.Instance.EnsureInTransaction(doc);
            v.SetCategoryOverrides(new Autodesk.Revit.DB.ElementId(category.Id), o);
            TransactionManager.Instance.TransactionTaskDone();

            return view;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="categoryId"></param>
        /// <param name="overrides"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static View SetCategoryOverrides(View view, int categoryId, OverrideGraphicsSettings overrides)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));
            if (overrides == null)
                throw new ArgumentNullException(nameof(overrides));

            var v = (Autodesk.Revit.DB.View)view.InternalElement;
            var o = overrides.InternalOverrideGraphicSettings;

            var doc = DocumentManager.Instance.CurrentDBDocument;
            TransactionManager.Instance.EnsureInTransaction(doc);
            v.SetCategoryOverrides(new Autodesk.Revit.DB.ElementId(categoryId), o);
            TransactionManager.Instance.TransactionTaskDone();

            return view;
        }

        #endregion

        #region Query

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static Point LowerLeftCorner(View view)
        {
            if (view == null)
                throw new ArgumentException(nameof(view));

            var v = (Autodesk.Revit.DB.View)view.InternalElement;
            var minOutl = v.Outline.Min * v.Scale;
            var o = v.Origin;
            o += v.RightDirection * minOutl.U;
            o += v.UpDirection * minOutl.V;

            return o.ToPoint();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        [MultiReturn("DWG", "RVT", "DWF", "DXF", "DWFX")]
        public static Dictionary<string, object> GetImportedCategories(View view)
        {
            if (view == null)
                throw new ArgumentException(nameof(view));

            var v = (Autodesk.Revit.DB.View)view.InternalElement;
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var categories = doc.Settings.Categories;
            var linkedRevitCat = categories.get_Item(Autodesk.Revit.DB.BuiltInCategory.OST_RvtLinks);

            var dwg = new List<Category>();
            var rvt = new List<Category>();
            var dwf = new List<Category>();
            var dxf = new List<Category>();
            var dwfx = new List<Category>();
            foreach (Autodesk.Revit.DB.Category c in categories)
            {
                // (Konrad) Skip out on imports that cannot be controlled from the view.
                if (!c.get_AllowsVisibilityControl(v))
                    continue;

                if (c.Name.ToLower().EndsWith(".dwg"))
                {
                    dwg.Add(Category.ById(c.Id.IntegerValue));
                    continue;
                }
                if (c.Name.ToLower().Contains(".rvt") || (linkedRevitCat != null && c.Id.Equals(linkedRevitCat.Id)))
                {
                    rvt.Add(Category.ById(c.Id.IntegerValue));
                    continue;
                }
                if (c.Name.ToLower().EndsWith(".dwf"))
                {
                    dwf.Add(Category.ById(c.Id.IntegerValue));
                    continue;
                }
                if (c.Name.ToLower().EndsWith(".dxf"))
                {
                    dxf.Add(Category.ById(c.Id.IntegerValue));
                    continue;
                }
                if (c.Name.ToLower().EndsWith(".dwfx"))
                {
                    dwfx.Add(Category.ById(c.Id.IntegerValue));
                }
            }

            return new Dictionary<string, object>()
            {
                {"DWG", dwg},
                {"RVT", rvt},
                {"DWF", dwf},
                {"DXF", dxf},
                {"DWFX", dwfx}
            };
        }

        /// <summary>
        /// Get View Template applied to view.
        /// </summary>
        /// <param name="view">View to retrieve View Template from.</param>
        /// <returns name="view">View Template applied to view.</returns>
        /// <search>view, template</search>
        [NodeCategory("Query")]
        public static object ViewTemplate(View view)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var v = (Autodesk.Revit.DB.View)view.InternalElement;

            var id = v.ViewTemplateId;
            return id != Autodesk.Revit.DB.ElementId.InvalidElementId ? doc.GetElement(id).ToDSType(true) : null;
        }

        /// <summary>
        /// Check if Schedule is Titleblock Schedule.
        /// </summary>
        /// <param name="view">Schedule View to test.</param>
        /// <returns></returns>
        /// <search>titleblock, schedule</search>
        [NodeCategory("Query")]
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
        [NodeCategory("Query")]
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
                    if (schedule == null) throw new ArgumentException("Invalid View");

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
        /// Retrieves Reference Callouts from a View.
        /// </summary>
        /// <param name="view">View to retrieve Reference Callouts from.</param>
        /// <returns name="callout[]">List of Reference Callouts.</returns>
        /// <search>get, reference, callout</search>
        [NodeCategory("Query")]
        public static List<Element> ReferenceCallouts(View view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));
            if (!(view.InternalElement is Autodesk.Revit.DB.View v))
                throw new ArgumentException("View is not a valid type.");

            var doc = DocumentManager.Instance.CurrentDBDocument;

            return v.GetReferenceCallouts().Select(x => doc.GetElement(x).ToDSType(true)).ToList();
        }

        /// <summary>
        /// Get View's Outline ie. Rectangle.
        /// </summary>
        /// <param name="view">View to retrieve Outline from.</param>
        /// <returns name="outline">View Outline.</returns>
        /// <search>view, outline</search>
        [NodeCategory("Query")]
        public static Rectangle Outline(View view)
        {
            var v = (Autodesk.Revit.DB.View)view.InternalElement;
            if (v == null)
                throw new ArgumentNullException(nameof(view));

            var o = v.Outline;
            var pt1 = new Autodesk.Revit.DB.XYZ(o.Min.U, o.Min.V, 0);
            var pt2 = new Autodesk.Revit.DB.XYZ(o.Max.U, o.Min.V, 0);
            var pt3 = new Autodesk.Revit.DB.XYZ(o.Max.U, o.Max.V, 0);
            var pt4 = new Autodesk.Revit.DB.XYZ(o.Min.U, o.Max.V, 0);

            return Rectangle.ByCornerPoints(pt1.ToPoint(), pt2.ToPoint(), pt3.ToPoint(), pt4.ToPoint());
        }

        /// <summary>
        /// Retrieves Crop Box of the View as Bounding Box object.
        /// </summary>
        /// <param name="view">View to extract the Crop Box from.</param>
        /// <returns name="boundingBox">Bounding Box.</returns>
        /// <search>view, crop box</search>
        [NodeCategory("Query")]
        public static BoundingBox CropBox(View view)
        {
            if (!(view.InternalElement is Autodesk.Revit.DB.View v))
                throw new ArgumentNullException(nameof(view));

            var cb = v.CropBox;
            var min = cb.Min.ToPoint();
            var max = cb.Max.ToPoint();

            return BoundingBox.ByCorners(min, max);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static string ViewType(View view)
        {
            var v = (Autodesk.Revit.DB.View)view.InternalElement;
            return v.ViewType.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static string DetailLevel(View view)
        {
            var v = (Autodesk.Revit.DB.View)view.InternalElement;
            return v.DetailLevel.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static string PartsVisibility(View view)
        {
            var v = (Autodesk.Revit.DB.View)view.InternalElement;
            return v.PartsVisibility.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static string Discipline(View view)
        {
            var v = (Autodesk.Revit.DB.View)view.InternalElement;
            return v.Discipline.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="parameterFilter"></param>
        /// <returns name="settings"></returns>
        [NodeCategory("Query")]
        public static OverrideGraphicsSettings FilterOverrides(View view, ParameterFilterElement parameterFilter)
        {
            var v = (Autodesk.Revit.DB.View)view.InternalElement;
            var overrides = v.GetFilterOverrides(parameterFilter.InternalElement.Id);

            return new OverrideGraphicsSettings(overrides);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="category"></param>
        /// <returns name="settings"></returns>
        [NodeCategory("Query")]
        public static OverrideGraphicsSettings CategoryOverrides(View view, Category category)
        {
            var v = (Autodesk.Revit.DB.View)view.InternalElement;
            var overrides = v.GetCategoryOverrides(new Autodesk.Revit.DB.ElementId(category.Id));

            return new OverrideGraphicsSettings(overrides);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="parameterFilter"></param>
        /// <returns name="settings"></returns>
        [NodeCategory("Query")]
        public static bool FilterVisibility(View view, ParameterFilterElement parameterFilter)
        {
            var v = (Autodesk.Revit.DB.View)view.InternalElement;
            var visible = v.GetFilterVisibility(parameterFilter.InternalElement.Id);

            return visible;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="parameterFilter"></param>
        /// <returns name="settings"></returns>
        [NodeCategory("Query")]
        public static bool IsFilterApplied(View view, ParameterFilterElement parameterFilter)
        {
            var v = (Autodesk.Revit.DB.View)view.InternalElement;
            var visible = v.IsFilterApplied(parameterFilter.InternalElement.Id);

            return visible;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        [MultiReturn("Silhouettes", "SilhouetteStyle", "Transparency", "SmoothLines", "ShowEdges")]
        public static Dictionary<string, object> ModelDisplay(View view)
        {
            var v = (Autodesk.Revit.DB.View) view.InternalElement;
            var model = v.GetViewDisplayModel();

            var doc = DocumentManager.Instance.CurrentDBDocument;
            return new Dictionary<string, object>
            {
                {"Silhouettes", model.EnableSilhouettes},
                {"SilhouetteStyle", !model.EnableSilhouettes ? "<none>" : doc.GetElement(model.SilhouetteEdgesGStyleId)?.Name},
                {"Transparency", model.Transparency},
                {"SmoothLines", model.SmoothEdges},
                {"ShowEdges", model.ShowHiddenLines.ToString()}
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        [MultiReturn("Enabled", "Extension", "Jitter")]
        public static Dictionary<string, object> SketchyLines(View view)
        {
            var v = (Autodesk.Revit.DB.View)view.InternalElement;
            var sl = v.GetSketchyLines();

            return new Dictionary<string, object>
            {
                {"Enabled", sl.EnableSketchyLines},
                {"Extension", sl.Extension},
                {"Jitter", sl.Jitter}
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        [MultiReturn("SunAndShadowSettings", "SunlightIntensity", "ShadowIntensity")]
        public static Dictionary<string, object> Lighting(View view)
        {
            var v = (Autodesk.Revit.DB.View)view.InternalElement;
            var settings = new SunAndShadowSettings(v.SunAndShadowSettings);


            return new Dictionary<string, object>
            {
                {"SunAndShadowSettings", settings},
                {"SunlightIntensity", v.SunlightIntensity},
                {"ShadowIntensity", v.ShadowIntensity}
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static string DisplayStyle(View view)
        {
            var v = (Autodesk.Revit.DB.View)view.InternalElement;
            var style = v.DisplayStyle.ToString();

            return style;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static ViewDisplayBackgrounds Background(View view)
        {
            var v = (Autodesk.Revit.DB.View)view.InternalElement;
            var bg = v.GetBackground();

            return bg == null ? null : new ViewDisplayBackgrounds(bg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static ViewDisplayDepthCueing DepthCueing(View view)
        {
            var v = (Autodesk.Revit.DB.View)view.InternalElement;

            return v.CanUseDepthCueing() 
                ? new ViewDisplayDepthCueing(v.GetDepthCueing()) 
                : null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static RenderingSettings RenderingSettings(View view)
        {
            var v = (Autodesk.Revit.DB.View)view.InternalElement;
            if (v is Autodesk.Revit.DB.View3D view3D)
            {
                return new RenderingSettings(view3D.GetRenderingSettings());
            }

            return null;
        }

        #endregion
    }
}
