using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class FilterSet
    {
        internal Autodesk.Revit.DB.ElementFilter InternalElementFilter { get; set; }

        internal FilterSet()
        {
        }

        internal FilterSet(Autodesk.Revit.DB.ElementFilter ef)
        {
            InternalElementFilter = ef;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filterRules"></param>
        /// <param name="filterSets"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static FilterSet CreateAndSet(
            [DefaultArgument("Selection.Select.GetNull()")] List<FilterRule> filterRules, 
            [DefaultArgument("Selection.Select.GetNull()")] List<FilterSet> filterSets)
        {
            var finalRules = new List<Autodesk.Revit.DB.ElementFilter>();
            if (filterRules != null && filterRules.Any())
            {
                var epfRules = filterRules.Select(x => new Autodesk.Revit.DB.ElementParameterFilter(x.InternalFilterRule))
                    .Cast<Autodesk.Revit.DB.ElementFilter>().ToList();
                finalRules.AddRange(epfRules);
            }

            if (filterSets != null && filterSets.Any())
            {
                var epfSets = filterSets.Select(x => x.InternalElementFilter).ToList();
                finalRules.AddRange(epfSets);
            }

            var andFilter = new Autodesk.Revit.DB.LogicalAndFilter(finalRules);
            return new FilterSet(andFilter);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filterRules"></param>
        /// <param name="filterSets"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static FilterSet CreateOrSet(
            [DefaultArgument("Selection.Select.GetNull()")] List<FilterRule> filterRules,
            [DefaultArgument("Selection.Select.GetNull()")] List<FilterSet> filterSets)
        {
            var finalRules = new List<Autodesk.Revit.DB.ElementFilter>();
            if (filterRules != null && filterRules.Any())
            {
                var epfRules = filterRules.Select(x => new Autodesk.Revit.DB.ElementParameterFilter(x.InternalFilterRule))
                    .Cast<Autodesk.Revit.DB.ElementFilter>().ToList();
                finalRules.AddRange(epfRules);
            }

            if (filterSets != null && filterSets.Any())
            {
                var epfSets = filterSets.Select(x => x.InternalElementFilter).ToList();
                finalRules.AddRange(epfSets);
            }

            var orFilter = new Autodesk.Revit.DB.LogicalOrFilter(finalRules);
            return new FilterSet(orFilter);
        }
    }
}
