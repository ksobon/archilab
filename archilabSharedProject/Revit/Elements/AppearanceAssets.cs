#if !Revit2017

using System;
using DSCore;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Revit.Elements;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// Appearance Asset wrapper class.
    /// </summary>
    public class AppearanceAssets
    {
        internal AppearanceAssets()
        {
        }

        /// <summary>
        /// Sets the Diffuse Color property for the Appearance Asset inside of the Material.
        /// </summary>
        /// <param name="appearanceAsset">Appearance Asset to set the color for.</param>
        /// <param name="color">Color that property will be set to.</param>
        /// <returns>Appearance Asset with the adjusted color property.</returns>
        public static Element SetDiffuseColor(Element appearanceAsset, Color color)
        {
            if (appearanceAsset == null)
                throw new ArgumentNullException(nameof(appearanceAsset));
            if (color == null)
                throw new ArgumentNullException(nameof(color));

            var aa = appearanceAsset.InternalElement as Autodesk.Revit.DB.AppearanceAssetElement;
            var c = new Autodesk.Revit.DB.Color(color.Red, color.Green, color.Blue);
            var doc = DocumentManager.Instance.CurrentDBDocument;

            TransactionManager.Instance.EnsureInTransaction(doc);
            using (var editScope = new Autodesk.Revit.DB.Visual.AppearanceAssetEditScope(doc))
            {
                var editableAsset = editScope.Start(aa.Id);
                var diffuseProperty = editableAsset.FindByName("generic_diffuse") as Autodesk.Revit.DB.Visual.AssetPropertyDoubleArray4d;
                diffuseProperty.SetValueAsColor(c);
                editScope.Commit(true);
            }
            TransactionManager.Instance.TransactionTaskDone();

            return aa.ToDSType(true);
        }
    }
}

#endif
