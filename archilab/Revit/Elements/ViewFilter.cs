using System.Collections.Generic;
using System.Linq;
using Revit.Elements;
using RevitServices.Persistence;

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
        ///     Returns Views that View Filter is applied to.
        /// </summary>
        /// <param name="viewFilter">View Filter Element.</param>
        /// <returns name="view">Views.</returns>
        /// <search>view, filter, owner</search>
        public static List<global::Revit.Elements.Element> OwnerViews(global::Revit.Elements.Element viewFilter)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var vf = viewFilter.InternalElement as Autodesk.Revit.DB.ParameterFilterElement;

            var allViews = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                .OfClass(typeof(Autodesk.Revit.DB.View))
                .Cast<Autodesk.Revit.DB.View>()
                // exclude view templates and views that filters cannot be applied to
                .Where(x => x.AreGraphicsOverridesAllowed())
                .ToList();

            var matches = new List<global::Revit.Elements.Element>();
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
