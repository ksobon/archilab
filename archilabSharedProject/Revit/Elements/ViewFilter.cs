using System.Collections.Generic;
using System.Linq;
using Dynamo.Graph.Nodes;
using Revit.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// Wrapper class for View Filters.
    /// </summary>
    public class ViewFilter
    {
        internal ViewFilter()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="category"></param>
        /// <param name="filterSet"></param>
        /// <returns name="viewFilter">Newly created Parameter Filter Element.</returns>
        [NodeCategory("Action")]
        public static Element CreateFilter(string name, List<Category> category, FilterSet filterSet)
        {
            var catIds = category.Select(x => new Autodesk.Revit.DB.ElementId(x.Id)).ToList();
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var pfe = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                .OfClass(typeof(Autodesk.Revit.DB.ParameterFilterElement))
                .Cast<Autodesk.Revit.DB.ParameterFilterElement>()
                .FirstOrDefault(x => x.Name == name);

            Element result;

            TransactionManager.Instance.EnsureInTransaction(doc);
            if (pfe == null)
            {
                pfe = Autodesk.Revit.DB.ParameterFilterElement.Create(doc, name, catIds);
                pfe.SetElementFilter(filterSet.InternalElementFilter);
                result = pfe.ToDSType(true);
            }
            else
            {
                pfe.ClearRules();
                pfe.SetElementFilter(filterSet.InternalElementFilter);
                pfe.SetCategories(catIds);
                result = pfe.ToDSType(true);
            }
            TransactionManager.Instance.TransactionTaskDone();

            return result;
        }

        /// <summary>
        /// Returns Views that View Filter is applied to.
        /// </summary>
        /// <param name="viewFilter">View Filter Element.</param>
        /// <returns name="view">Views.</returns>
        /// <search>view, filter, owner</search>
        [NodeCategory("Query")]
        public static List<Element> OwnerViews(Element viewFilter)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var vf = viewFilter.InternalElement as Autodesk.Revit.DB.ParameterFilterElement;

            var allViews = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                .OfClass(typeof(Autodesk.Revit.DB.View))
                .Cast<Autodesk.Revit.DB.View>()
                // exclude view templates and views that filters cannot be applied to
                .Where(x => x.AreGraphicsOverridesAllowed())
                .ToList();

            var matches = new List<Element>();
            foreach (var v in allViews)
            {
                var filters = v.GetFilters();
                if (filters.Count != 0)
                {
                    foreach (var id in filters)
                    {
                        var f = doc.GetElement(id) as Autodesk.Revit.DB.ParameterFilterElement;
                        if (vf != null && (f != null && f.Name == vf.Name))
                        {
                            matches.Add(v.ToDSType(true));
                        }
                    }
                }
            }
            return matches;
        }
    }
}
