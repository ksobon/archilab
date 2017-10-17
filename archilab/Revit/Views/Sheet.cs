using System;
using System.Collections.Generic;
using System.Linq;
using DynamoServices;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Revit.Elements;

namespace archilab.Revit.Views
{
    /// <summary>
    /// Wrapper class for View Sheets.
    /// </summary>
    [RegisterForTrace]
    public class Sheet : Element
    {
        internal Autodesk.Revit.DB.ViewSheet InternalViewSheet { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalViewSheet; }
        }

        private Sheet(string nameOfSheet, string numberOfSheet)
        {
            SafeInit(() => InitSheet(nameOfSheet, numberOfSheet));
        }

        private void InitSheet(string nameOfSheet, string numberOfSheet)
        {

            //Phase 1 - Check to see if the object exists and should be rebound
            var oldEle = ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.ViewSheet>(DocumentManager.Instance.CurrentDBDocument);

            // Rebind to Element
            if (oldEle != null)
            {
                InternalSetViewSheet(oldEle);
                InternalSetSheetName(nameOfSheet);
                InternalSetSheetNumber(numberOfSheet);
                return;
            }

            //Phase 2 - There was no existing Element, create new one
            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);

            // create sheet without title block
            var sheet = Autodesk.Revit.DB.ViewSheet.CreatePlaceholder(DocumentManager.Instance.CurrentDBDocument);

            InternalSetViewSheet(sheet);
            InternalSetSheetName(nameOfSheet);
            InternalSetSheetNumber(numberOfSheet);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(InternalElement);
        }

        private void InternalSetViewSheet(Autodesk.Revit.DB.ViewSheet view)
        {
            InternalViewSheet = view;
            InternalElementId = view.Id;
            InternalUniqueId = view.UniqueId;
        }

        private void InternalSetSheetName(string name)
        {
            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);

            InternalViewSheet.Name = name;

            TransactionManager.Instance.TransactionTaskDone();
        }

        private void InternalSetSheetNumber(string number)
        {
            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);

            InternalViewSheet.SheetNumber = number;

            TransactionManager.Instance.TransactionTaskDone();
        }

        /// <summary>
        /// Revisions on Sheet.
        /// </summary>
        /// <param name="sheet">View Sheet.</param>
        /// <returns name="Element">Revisions on Sheet.</returns>
        public static List<Element> Revisions(global::Revit.Elements.Views.Sheet sheet)
        {
            var vs = sheet.InternalElement as Autodesk.Revit.DB.ViewSheet;
            var revIds = vs?.GetAllRevisionIds();

            if (revIds != null && revIds.Count > 0)
            {
                var doc = DocumentManager.Instance.CurrentDBDocument;
                return revIds.Select(x => doc.GetElement(x).ToDSType(true)).ToList();
            }
            return Enumerable.Empty<Element>().ToList();
        }

        /// <summary>
        /// Viewports on sheet.
        /// </summary>
        /// <param name="sheet">View Sheet.</param>
        /// <returns name="Element">Viewports on Sheet.</returns>
        public static List<Element> Viewports(global::Revit.Elements.Views.Sheet sheet)
        {
            var vs = sheet.InternalElement as Autodesk.Revit.DB.ViewSheet;
            var viewports = vs?.GetAllViewports();

            if (viewports?.Count > 0)
            {
                var doc = DocumentManager.Instance.CurrentDBDocument;
                return viewports.Select(x => doc.GetElement(x).ToDSType(true)).ToList();
            }
            return Enumerable.Empty<Element>().ToList();
        }

        /// <summary>
        /// Create Placeholder sheet.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="number"></param>
        /// <returns name="Sheet">View sheet</returns>
        public static global::Revit.Elements.Views.Sheet CreatePlaceholder(string number, string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (number == null)
            {
                throw new ArgumentNullException(nameof(number));
            }

            var newSheet = new Sheet(name, number);

            return (global::Revit.Elements.Views.Sheet)newSheet.InternalViewSheet.ToDSType(true);
        }
    }
}
