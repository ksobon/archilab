using System;
using System.Collections.Generic;
using System.Linq;
using archilab.Revit.Elements;
using Autodesk.DesignScript.Runtime;
using DynamoServices;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Revit.Elements;
using Revit.Elements.Views;

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
        ///     Remove view filter from view.
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
        ///     Get View Template applied to view.
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
        ///     Set View Template for a View.
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
        ///     Removes View Template from given view.
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
        ///     Get all views by type.
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
        ///     Check if Schedule is Titleblock Schedule.
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
        /// <param name="options">Duplicate options. Ex: Duplicate as Dependant.</param>
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
        /// Retrieves Template Parameter Ids.
        /// </summary>
        /// <param name="view">View to get the Parameter Ids from.</param>
        /// <returns>List of Parameter Ids.</returns>
        public static List<int> GetTemplateParameterIds(View view)
        {
            var v = (Autodesk.Revit.DB.View) view.InternalElement;
            return v.GetTemplateParameterIds().Select(x => x.IntegerValue).ToList();
        }

        #region Utilities

        /// <summary>
        ///     Get Null
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
