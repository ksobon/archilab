using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;
using Revit.Elements;
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
        public static Dictionary<string, string> GetWorksharingTooltipInfo(global::Revit.Elements.Element element)
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
        public static int PhaseDemolished(global::Revit.Elements.Element element)
        {
            return element.InternalElement.DemolishedPhaseId.IntegerValue;
        }

        /// <summary>
        /// Get Element Type.
        /// </summary>
        /// <param name="element"></param>
        /// <returns name="Type"></returns>
        /// <search>element, type</search>
        public static global::Revit.Elements.Element Type(global::Revit.Elements.Element element)
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
        public static bool Delete(global::Revit.Elements.Element element)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var e = element.InternalElement;

            TransactionManager.Instance.EnsureInTransaction(doc);
            try
            {
                doc.Delete(e.Id);
                return true;
            }
            catch (Exception) { return false; }
        }
    }
}
