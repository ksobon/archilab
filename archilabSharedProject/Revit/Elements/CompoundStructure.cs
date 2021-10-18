using System.Collections.Generic;
using System.Linq;
using Dynamo.Graph.Nodes;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class CompoundStructure
    {
        internal Autodesk.Revit.DB.CompoundStructure InternalCompoundStructure { get; set; }

        internal CompoundStructure()
        {
        }

        internal CompoundStructure(Autodesk.Revit.DB.CompoundStructure cs)
        {
            InternalCompoundStructure = cs;
        }

        /// <summary>
        /// Retrieves a list of Layers that make up the Wall Type.
        /// </summary>
        /// <param name="cs">Compound Structure of the Wall Type.</param>
        /// <returns name="csLayer">Returns a list of Compound Structure Layers.</returns>
        [NodeCategory("Query")]
        public static List<CompoundStructureLayer> Layers(CompoundStructure cs)
        {
            return cs.InternalCompoundStructure.GetLayers().Select(x => new CompoundStructureLayer(x)).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Compound Structure";
        }
    }
}
