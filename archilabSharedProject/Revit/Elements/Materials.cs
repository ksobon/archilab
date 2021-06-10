using System;
using System.Collections.Generic;
using archilab.Utilities;
using Autodesk.DesignScript.Runtime;
using DSCore;
using Dynamo.Graph.Nodes;
using DynamoServices;
using Revit.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// Material wrapper class.
    /// </summary>
    [RegisterForTrace]
    public class Materials : Element
    {
        internal Autodesk.Revit.DB.Material InternalMaterial
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalMaterial; }
        }

        private Materials(string name)
        {
            SafeInit(() => InitMaterial(name));
        }

        private void InitMaterial(string name)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;

            //Phase 1 - Check to see if the object exists and should be rebound
            var oldEle = ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.Material>(doc);

            // Rebind to Element
            if (oldEle != null)
            {
                InternalSetMaterial(oldEle);
                return;
            }

            //Phase 2 - There was no existing Element, create new one
            TransactionManager.Instance.EnsureInTransaction(doc);

            var mId = Autodesk.Revit.DB.Material.Create(doc, name);
            var m = doc.GetElement(mId) as Autodesk.Revit.DB.Material;

            InternalSetMaterial(m);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(InternalElement);
        }

        private void InternalSetMaterial(Autodesk.Revit.DB.Material m)
        {
            InternalMaterial = m;
        }

        /// <summary>
        /// Creates new Material by name.
        /// </summary>
        /// <param name="name">Name of the material to be created.</param>
        /// <returns name="material"></returns>
        [NodeCategory("Action")]
        public static Material Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            var m = new Materials(name);

            return m.InternalMaterial.ToDSType(true) as Material;
        }

        /// <summary>
        /// Sets the Appearance Asset Element for the Material.
        /// </summary>
        /// <param name="material">Material to set the Appearance Asset for.</param>
        /// <param name="appearanceAsset">Appearance Asset object.</param>
        /// <returns name="material"></returns>
        [NodeCategory("Action")]
        public static Material SetAppearanceAsset(Material material, Element appearanceAsset)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            if (!(material.InternalElement is Autodesk.Revit.DB.Material m))
                throw new ArgumentNullException(nameof(material));

            if (appearanceAsset == null)
                throw new ArgumentNullException(nameof(appearanceAsset));

            if (!(appearanceAsset.InternalElement is Autodesk.Revit.DB.AppearanceAssetElement aae))
                throw new ArgumentNullException(nameof(appearanceAsset));

            var doc = DocumentManager.Instance.CurrentDBDocument;

            TransactionManager.Instance.EnsureInTransaction(doc);
            m.AppearanceAssetId = aae.Id;
            TransactionManager.Instance.TransactionTaskDone();

            return material;
        }

        /// <summary>
        /// Sets MaterialCategory property for the Material.
        /// </summary>
        /// <param name="material">Material to set the MaterialCategory for.</param>
        /// <param name="materialCategory">MaterialCategory value.</param>
        /// <returns name="material"></returns>
        [NodeCategory("Action")]
        public static Material SetMaterialCategory(Material material, string materialCategory)
        {
            if (string.IsNullOrWhiteSpace(materialCategory))
                throw new ArgumentNullException(nameof(materialCategory));

            if (material == null)
                throw new ArgumentNullException(nameof(material));

            if (!(material.InternalElement is Autodesk.Revit.DB.Material m))
                throw new ArgumentNullException(nameof(material));

            var doc = DocumentManager.Instance.CurrentDBDocument;

            TransactionManager.Instance.EnsureInTransaction(doc);
            m.MaterialCategory = materialCategory;
            TransactionManager.Instance.TransactionTaskDone();

            return material;
        }

        /// <summary>
        /// Sets MaterialCategory property for the Material.
        /// </summary>
        /// <param name="material">Material to set the MaterialCategory for.</param>
        /// <param name="materialClass">MaterialClass value.</param>
        /// <returns name="material"></returns>
        [NodeCategory("Action")]
        public static Material SetMaterialClass(Material material, string materialClass)
        {
            if (string.IsNullOrWhiteSpace(materialClass))
                throw new ArgumentNullException(nameof(materialClass));

            if (material == null)
                throw new ArgumentNullException(nameof(material));

            if (!(material.InternalElement is Autodesk.Revit.DB.Material m))
                throw new ArgumentNullException(nameof(material));

            var doc = DocumentManager.Instance.CurrentDBDocument;

            TransactionManager.Instance.EnsureInTransaction(doc);
            m.MaterialClass = materialClass;
            TransactionManager.Instance.TransactionTaskDone();

            return material;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="material"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static string GetMaterialClass(Material material)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            if (!(material.InternalElement is Autodesk.Revit.DB.Material m))
                throw new ArgumentNullException(nameof(material));

            return m.MaterialClass;
        }

#if Revit2018

        /// <summary>
        /// Sets Surface Pattern properties for a Material.
        /// </summary>
        /// <param name="material">Material to set Surface Pattern for.</param>
        /// <param name="surfacePattern">Fill Pattern to be used as Surface Pattern.</param>
        /// <param name="surfacePatternColor">Color of the Surface Pattern.</param>
        /// <returns name="material"></returns>
        [NodeCategory("Action")]
        public static Material SetSurfacePattern(
            Material material,
            [DefaultArgument("Selection.Select.GetNull()")] Element surfacePattern,
            [DefaultArgument("Utilities.ColorUtilities.GetBlack()")] Color surfacePatternColor)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            if (!(material.InternalElement is Autodesk.Revit.DB.Material m))
                throw new ArgumentNullException(nameof(material));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            TransactionManager.Instance.EnsureInTransaction(doc);

            if (surfacePattern != null)
            {
                if (!(surfacePattern.InternalElement is Autodesk.Revit.DB.FillPatternElement fp))
                    throw new ArgumentNullException(nameof(surfacePattern));

                m.SurfacePatternId = fp.Id;
            }

            m.SurfacePatternColor = ColorUtilities.RevitColorByColor(surfacePatternColor);

            TransactionManager.Instance.TransactionTaskDone();

            return material;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="material"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static Element GetSurfacePattern(Material material)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            if (!(material.InternalElement is Autodesk.Revit.DB.Material m))
                throw new ArgumentNullException(nameof(material));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            var sp = doc.GetElement(m.SurfacePatternId);

            return sp?.ToDSType(true);
        }

        /// <summary>
        /// Sets Cut Pattern properties for a Material.
        /// </summary>
        /// <param name="material">Material to set Cut Pattern for.</param>
        /// <param name="cutPattern">Fill Pattern to be used as Cut Pattern.</param>
        /// <param name="cutPatternColor">Color of the Cut Pattern.</param>
        /// <returns name="material"></returns>
        public static Material SetCutPattern(
            Material material,
            [DefaultArgument("Selection.Select.GetNull()")] Element cutPattern,
            [DefaultArgument("Utilities.ColorUtilities.GetBlack()")] Color cutPatternColor)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            if (!(material.InternalElement is Autodesk.Revit.DB.Material m))
                throw new ArgumentNullException(nameof(material));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            TransactionManager.Instance.EnsureInTransaction(doc);

            if (cutPattern != null)
            {
                if (!(cutPattern.InternalElement is Autodesk.Revit.DB.FillPatternElement fp))
                    throw new ArgumentNullException(nameof(cutPattern));

                m.CutPatternId = fp.Id;
            }

            m.CutPatternColor = ColorUtilities.RevitColorByColor(cutPatternColor);

            TransactionManager.Instance.TransactionTaskDone();

            return material;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="material"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static Element GetCutPattern(Material material)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            if (!(material.InternalElement is Autodesk.Revit.DB.Material m))
                throw new ArgumentNullException(nameof(material));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            var cp = doc.GetElement(m.CutPatternId);

            return cp?.ToDSType(true);
        }

#endif

#if !Revit2018

        /// <summary>
        /// Sets Surface Pattern properties for a Material.
        /// </summary>
        /// <param name="material">Material to set Surface Pattern for.</param>
        /// <param name="foregroundPattern">Fill Pattern to be used as Foreground Pattern.</param>
        /// <param name="backgroundPattern">Fill Pattern to be used as Background Pattern.</param>
        /// <param name="foregroundPatternColor">Color of the Foreground Pattern.</param>
        /// <param name="backgroundPatternColor">Color of the Background Pattern.</param>
        /// <returns name="material"></returns>
        [NodeCategory("Action")]
        public static Material SetSurfacePattern(
            Material material,
            [DefaultArgument("Selection.Select.GetNull()")] Element foregroundPattern,
            [DefaultArgument("Selection.Select.GetNull()")] Element backgroundPattern,
            [DefaultArgument("Utilities.ColorUtilities.GetBlack()")] Color foregroundPatternColor,
            [DefaultArgument("Utilities.ColorUtilities.GetBlack()")] Color backgroundPatternColor)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            if (!(material.InternalElement is Autodesk.Revit.DB.Material m))
                throw new ArgumentNullException(nameof(material));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            TransactionManager.Instance.EnsureInTransaction(doc);

            if (foregroundPattern != null)
            {
                if (!(foregroundPattern.InternalElement is Autodesk.Revit.DB.FillPatternElement fp))
                    throw new ArgumentNullException(nameof(foregroundPattern));

                m.SurfaceForegroundPatternId = fp.Id;
            }

            if (backgroundPattern != null)
            {
                if (!(backgroundPattern.InternalElement is Autodesk.Revit.DB.FillPatternElement bp))
                    throw new ArgumentNullException(nameof(backgroundPattern));

                m.SurfaceBackgroundPatternId = bp.Id;
            }

            m.SurfaceForegroundPatternColor = ColorUtilities.RevitColorByColor(foregroundPatternColor);
            m.SurfaceBackgroundPatternColor = ColorUtilities.RevitColorByColor(backgroundPatternColor);

            TransactionManager.Instance.TransactionTaskDone();

            return material;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="material"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        [MultiReturn("foregroundPattern", "backgroundPattern")]
        public static Dictionary<string, object> GetSurfacePatterns(Material material)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            if (!(material.InternalElement is Autodesk.Revit.DB.Material m))
                throw new ArgumentNullException(nameof(material));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            var fp = doc.GetElement(m.SurfaceForegroundPatternId);
            var bp = doc.GetElement(m.SurfaceBackgroundPatternId);

            return new Dictionary<string, object>
            {
                {"foregroundPattern", fp?.ToDSType(true)},
                {"backgroundPattern", bp?.ToDSType(true)}
            };
        }

        /// <summary>
        /// Sets Cut Pattern properties for a Material.
        /// </summary>
        /// <param name="material">Material to set Surface Pattern for.</param>
        /// <param name="foregroundPattern">Fill Pattern to be used as Foreground Pattern.</param>
        /// <param name="backgroundPattern">Fill Pattern to be used as Background Pattern.</param>
        /// <param name="foregroundPatternColor">Color of the Foreground Pattern.</param>
        /// <param name="backgroundPatternColor">Color of the Background Pattern.</param>
        /// <returns name="material"></returns>
        public static Material SetCutPattern(
            Material material,
            [DefaultArgument("Selection.Select.GetNull()")] Element foregroundPattern,
            [DefaultArgument("Selection.Select.GetNull()")] Element backgroundPattern,
            [DefaultArgument("Utilities.ColorUtilities.GetBlack()")] Color foregroundPatternColor,
            [DefaultArgument("Utilities.ColorUtilities.GetBlack()")] Color backgroundPatternColor)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            if (!(material.InternalElement is Autodesk.Revit.DB.Material m))
                throw new ArgumentNullException(nameof(material));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            TransactionManager.Instance.EnsureInTransaction(doc);

            if (foregroundPattern != null)
            {
                if (!(foregroundPattern.InternalElement is Autodesk.Revit.DB.FillPatternElement fp))
                    throw new ArgumentNullException(nameof(foregroundPattern));

                m.CutForegroundPatternId = fp.Id;
            }

            if (backgroundPattern != null)
            {
                if (!(backgroundPattern.InternalElement is Autodesk.Revit.DB.FillPatternElement bp))
                    throw new ArgumentNullException(nameof(backgroundPattern));

                m.CutBackgroundPatternId = bp.Id;
            }

            m.CutForegroundPatternColor = ColorUtilities.RevitColorByColor(foregroundPatternColor);
            m.CutBackgroundPatternColor = ColorUtilities.RevitColorByColor(backgroundPatternColor);

            TransactionManager.Instance.TransactionTaskDone();

            return material;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="material"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        [MultiReturn("foregroundPattern", "backgroundPattern")]
        public static Dictionary<string, object> GetCutPatterns(Material material)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            if (!(material.InternalElement is Autodesk.Revit.DB.Material m))
                throw new ArgumentNullException(nameof(material));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            var fp = doc.GetElement(m.CutForegroundPatternId);
            var bp = doc.GetElement(m.CutBackgroundPatternId);

            return new Dictionary<string, object>
            {
                {"foregroundPattern", fp?.ToDSType(true)},
                {"backgroundPattern", bp?.ToDSType(true)}
            };
        }

#endif

        /// <summary>
        /// Sets the checkbox property on the Material that would force the Appearance Color to be used for Shading display in Revit.
        /// </summary>
        /// <param name="material">Material to set the property for.</param>
        /// <param name="check">True if checkbox is to be checked, otherwise false.</param>
        /// <returns name="material">Material with the adjusted property.</returns>
        [NodeCategory("Action")]
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
        /// 
        /// </summary>
        /// <param name="material"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static bool UsesRenderAppearanceForShading(Material material)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            if (!(material.InternalElement is Autodesk.Revit.DB.Material m))
                throw new ArgumentNullException(nameof(material));

            return m.UseRenderAppearanceForShading;
        }

        /// <summary>
        /// Sets the color of the Material.
        /// </summary>
        /// <param name="material">Material to set the Color for.</param>
        /// <param name="color">Color to set the Material to.</param>
        /// <returns name="material">Material with the adjusted color.</returns>
        [NodeCategory("Action")]
        public static Material SetColor(Material material, Color color)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            if (color == null)
                throw new ArgumentNullException(nameof(color));

            if (!(material.InternalElement is Autodesk.Revit.DB.Material m))
                throw new ArgumentNullException(nameof(material));

            var doc = DocumentManager.Instance.CurrentDBDocument;

            TransactionManager.Instance.EnsureInTransaction(doc);
            m.Color = ColorUtilities.RevitColorByColor(color);
            TransactionManager.Instance.TransactionTaskDone();

            return material;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="material"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static Color GetColor(Material material)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            if (!(material.InternalElement is Autodesk.Revit.DB.Material m))
                throw new ArgumentNullException(nameof(material));

            return ColorUtilities.DsColorByColor(m.Color);
        }

        /// <summary>
        /// Retrieves default Assets that Material can have. 
        /// </summary>
        /// <param name="material">Material to retrieve the assets from.</param>
        /// <returns>Assets if set for the Material.</returns>
        [NodeCategory("Query")]
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
    }
}
