using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Element = Revit.Elements.Element;

namespace archilab.Revit.ViewSets
{
    /// <summary>
    /// Methods and properties typically associated with View Sets in Revit
    /// </summary>
    public class ViewSets
    {
        internal ViewSets()
        {
        }

        /// <summary>
        /// Creates new View Set by name and List of Views.
        /// </summary>
        /// <param name="views">List of Views.</param>
        /// <param name="name">Name of the View Set.</param>
        /// <param name="replace">Override existing View Set with same name.</param>
        /// <returns>True if succeeded, otherwise false.</returns>
        public static bool ByViewsName(List<Element> views, string name, bool replace = false)
        {
            if (!views.Any() || views == null) throw new Exception("Views list is empty or null.");
            if (string.IsNullOrEmpty(name)) throw new Exception("Name is empty or null.");

            var doc = DocumentManager.Instance.CurrentDBDocument;

            var viewSet = new ViewSet();
            foreach (var v in views)
            {
                viewSet.Insert((View) v.InternalElement);
            }

            var printManager = doc.PrintManager;
            printManager.PrintRange = PrintRange.Select;

            var settings = printManager.ViewSheetSetting;
            settings.CurrentViewSheetSet.Views = viewSet;

            TransactionManager.Instance.EnsureInTransaction(doc);

            try
            {
                settings.SaveAs(name);
            }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException)
            {
                if (replace)
                {
                    try
                    {
                        settings.Delete();
                        doc.Regenerate();

                        settings.CurrentViewSheetSet.Views = viewSet;
                        settings.SaveAs(name);
                    }
                    catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                    {
                        throw new Exception("Could not replace existing View Set.");
                    }
                }
            }

            TransactionManager.Instance.TransactionTaskDone();

            return true;
        }
    }
}
