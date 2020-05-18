using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;
using DSCore;
using Revit.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// Material wrapper class.
    /// </summary>
    public class Materials
    {
        internal Materials()
        {
        }

        /// <summary>
        /// Retrieves default Assets that Material can have. 
        /// </summary>
        /// <param name="material">Material to retrieve the assets from.</param>
        /// <returns>Assets if set for the Material.</returns>
        [MultiReturn("appearanceAsset", "thermalAsset", "structuralAsset")]
        public static Dictionary<string, object> Assets(Material material)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            var m = material.InternalElement as Autodesk.Revit.DB.Material;
            var doc = DocumentManager.Instance.CurrentDBDocument;

            var aa = doc.GetElement(m?.AppearanceAssetId);
            var ta = doc.GetElement(m?.ThermalAssetId);
            var sa = doc.GetElement(m?.StructuralAssetId);

            return new Dictionary<string, object>
            {
                { "appearanceAsset", aa?.ToDSType(true)},
                { "thermalAsset", ta?.ToDSType(true)},
                { "structuralAsset", sa?.ToDSType(true)}
            };
        }

        /// <summary>
        /// Sets the checkbox property on the Material that would force the Appearance Color to be used for Shading display in Revit.
        /// </summary>
        /// <param name="material">Material to set the property for.</param>
        /// <param name="check">True if checkbox is to be checked, otherwise false.</param>
        /// <returns>Material with the adjusted property.</returns>
        public static Material UseRenderAppearanceForShading(Material material, bool check = true)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            if (!(material.InternalElement is Autodesk.Revit.DB.Material m))
                throw new ArgumentNullException(nameof(material));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            TransactionManager.Instance.EnsureInTransaction(doc);
            m.UseRenderAppearanceForShading = check;
            TransactionManager.Instance.TransactionTaskDone();

            return material;
        }

        /// <summary>
        /// Sets the color of the Material.
        /// </summary>
        /// <param name="material">Material to set the Color for.</param>
        /// <param name="color">Color to set the Material to.</param>
        /// <returns>Material with the adjusted color.</returns>
        public static Material SetColor(Material material, Color color)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));
            if (color == null)
                throw new ArgumentNullException(nameof(color));

            if (!(material.InternalElement is Autodesk.Revit.DB.Material m))
                throw new ArgumentNullException(nameof(material));

            var c = new Autodesk.Revit.DB.Color(color.Red, color.Green, color.Blue);
            var doc = DocumentManager.Instance.CurrentDBDocument;

            TransactionManager.Instance.EnsureInTransaction(doc);
            m.Color = c;
            TransactionManager.Instance.TransactionTaskDone();

            return material;
        }
    }
}
