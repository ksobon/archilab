using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Graph.Nodes;
using Revit.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Views
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
        /// Retrieves all of the View Sets from the model.
        /// </summary>
        /// <returns name="viewSets">View Sets.</returns>
        [NodeCategory("Action")]
        public static List<Element> GetAll()
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var sets = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                .OfClass(typeof(Autodesk.Revit.DB.ViewSheetSet))
                .WhereElementIsNotElementType()
                .Select(x => x.ToDSType(true))
                .ToList();

            return sets;
        }

        /// <summary>
        /// Creates new View Set by name and List of Views.
        /// </summary>
        /// <param name="views">List of Views.</param>
        /// <param name="name">Name of the View Set.</param>
        /// <param name="replace">Override existing View Set with same name.</param>
        /// <returns name="viewSet">View Sheet Set.</returns>
        [NodeCategory("Action")]
        public static Element ByViewsName(List<Element> views, string name, bool replace = false)
        {
            if (views == null || !views.Any())
                throw new ArgumentException(nameof(views));
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException(nameof(name));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            var viewSet = new Autodesk.Revit.DB.ViewSet();

            foreach (var v in views)
            {
                viewSet.Insert((Autodesk.Revit.DB.View) v.InternalElement);
            }

            var printManager = doc.PrintManager;
            printManager.PrintRange = Autodesk.Revit.DB.PrintRange.Select;

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

            var set = settings.CurrentViewSheetSet as Autodesk.Revit.DB.ViewSheetSet;

            return set?.ToDSType(true);
        }

        /// <summary>
        /// Retrieves all of the Views from a View Set.
        /// </summary>
        /// <param name="viewSet">View Set to query for Views.</param>
        /// <returns name="views">Views that are part of the View Set.</returns>
        [NodeCategory("Query")]
        public static List<Element> Views(Element viewSet)
        {
            if (viewSet == null)
                throw new ArgumentException(nameof(viewSet));

            if (!(viewSet.InternalElement is Autodesk.Revit.DB.ViewSheetSet set))
                return new List<Element>();

            var views = new List<Element>();
            foreach (Autodesk.Revit.DB.View v in set.Views)
            {
                views.Add(v.ToDSType(true));
            }

            return views;
        }
    }
}
