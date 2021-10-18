using System;
using System.Linq;
using Dynamo.Graph.Nodes;
using Revit.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class WallTypes
    {
        internal WallTypes()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wallType"></param>
        /// <param name="name"></param>
        /// <returns name="wallType">New Wall Type if one didn't exist, otherwise Wall Type that matched the name.</returns>
        [NodeCategory("Action")]
        public static Element Duplicate(Element wallType, string name)
        {
            if (wallType == null)
                throw new ArgumentNullException(nameof(wallType));
            if (!(wallType.InternalElement is Autodesk.Revit.DB.WallType wt))
                throw new Exception("Element is not a Wall.");
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            var existing = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                .OfClass(typeof(Autodesk.Revit.DB.Wall))
                .WhereElementIsElementType()
                .Cast<Autodesk.Revit.DB.WallType>()
                .FirstOrDefault(x => x.Name == name);
            if (existing != null)
                return existing.ToDSType(true);

            try
            {
                TransactionManager.Instance.EnsureInTransaction(doc);
                var newType = wt.Duplicate(name);
                TransactionManager.Instance.TransactionTaskDone();

                return newType.ToDSType(true);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wallType"></param>
        /// <param name="csLayer"></param>
        /// <param name="material"></param>
        [NodeCategory("Action")]
        public static bool SetMaterial(Element wallType, CompoundStructureLayer csLayer, Element material)
        {
            if (wallType == null)
                throw new ArgumentNullException(nameof(wallType));
            if (!(wallType.InternalElement is Autodesk.Revit.DB.WallType wt))
                throw new Exception("Element is not a Wall.");
            if (csLayer == null)
                throw new ArgumentNullException(nameof(csLayer));
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            var structure = wt.GetCompoundStructure();
            var doc = DocumentManager.Instance.CurrentDBDocument;

            try
            {
                TransactionManager.Instance.EnsureInTransaction(doc);

                structure.SetMaterialId(
                    csLayer.InternalCompoundStructureLayer.LayerId,
                    new Autodesk.Revit.DB.ElementId(material.Id));

                wt.SetCompoundStructure(structure);

                TransactionManager.Instance.TransactionTaskDone();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wallType"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static string Kind(Element wallType)
        {
            if (wallType == null)
                throw new ArgumentNullException(nameof(wallType));

            return (wallType.InternalElement as Autodesk.Revit.DB.WallType)?.Kind.ToString();
        }

        /// <summary>
        /// Get the Compound Structure of the Wall Type. This holds information about the layers and materials making up the Wall Type.
        /// </summary>
        /// <param name="wallType">Wall Type of the Wall.</param>
        /// <returns name="cs">Compound Structure that holds the Layer information for the Wall Type.</returns>
        [NodeCategory("Query")]
        public static CompoundStructure CompoundStructure(Element wallType)
        {
            if (wallType == null)
                throw new ArgumentException(nameof(wallType));
            if (!(wallType.InternalElement is Autodesk.Revit.DB.WallType wt))
                throw new Exception("Element is not a Wall.");

            return new CompoundStructure(wt.GetCompoundStructure());
        }
    }
}
