// ReSharper disable UnusedMember.Global
namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class CompoundStructureLayer
    {
        internal Autodesk.Revit.DB.CompoundStructureLayer InternalCompoundStructureLayer { get; set; }

        internal CompoundStructureLayer()
        {
        }

        internal CompoundStructureLayer(Autodesk.Revit.DB.CompoundStructureLayer csl)
        {
            InternalCompoundStructureLayer = csl;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Compound Structure Layer";
        }
    }
}
