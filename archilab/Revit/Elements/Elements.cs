using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using Revit.Elements;
using Revit.Elements.Views;
using RevitServices.Persistence;
using RevitServices.Transactions;

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
        /// Returns worksharing information about element.
        /// </summary>
        /// <param name="element">Element to analyze.</param>
        /// <returns>Information about the Elements Owner, Creator etc.</returns>
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
        public static int PhaseDemolished(Element element)
        {
            return element.InternalElement.DemolishedPhaseId.IntegerValue;
        }

        /// <summary>
        /// Get Element Type.
        /// </summary>
        /// <param name="element"></param>
        /// <returns name="Type"></returns>
        /// <search>element, type</search>
        public static Element Type(Element element)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var e = element.InternalElement;

            return doc.GetElement(e.GetTypeId()).ToDSType(true);
        }

        /// <summary>
        /// Delete element from Revit DB.
        /// </summary>
        /// <param name="element">Element to delete.</param>
        /// <returns></returns>
        /// <search>delete, remove, element</search>
        public static bool Delete(Element element)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var e = element.InternalElement;

            try
            {
                TransactionManager.Instance.EnsureInTransaction(doc);
                doc.Delete(e.Id);
                TransactionManager.Instance.TransactionTaskDone();
                return true;
            }
            catch (Exception) { return false; }
        }

        /// <summary>
        /// Checks whether an Element is visible in given View. 
        /// </summary>
        /// <param name="element">Element to check.</param>
        /// <param name="view">View to check visibility in.</param>
        /// <returns>True if Element is visible in View, otherwise false.</returns>
        public static bool IsVisible(Element element, View view)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            var e = element.InternalElement;
            var v = view.InternalElement as Autodesk.Revit.DB.View;

            var found = new Autodesk.Revit.DB.FilteredElementCollector(doc, v.Id)
                .OfCategoryId(e.Category.Id)
                .WhereElementIsNotElementType()
                .Where(x => x.Id == e.Id);

            return found.Any();
        }
    }
}
