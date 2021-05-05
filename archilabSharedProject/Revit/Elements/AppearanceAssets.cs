#if !Revit2017

using System;
using System.Collections.Generic;
using System.Linq;
using archilab.Utilities;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;
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
        /// Creates a new Appearance Asset Element via Duplicating of Default material's Appearance Asset Element.
        /// </summary>
        /// <param name="name">Name of the Appearance Asset to be created.</param>
        /// <returns name="appearanceAsset"></returns>
        [NodeCategory("Action")]
        public static Element CreateViaDuplicate(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            var defaultMat = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                .OfClass(typeof(Autodesk.Revit.DB.Material))
                .Cast<Autodesk.Revit.DB.Material>()
                .FirstOrDefault(x =>
                    x.AppearanceAssetId != Autodesk.Revit.DB.ElementId.InvalidElementId && x.Name.Contains("Default"));

            if (defaultMat == null)
                throw new Exception("Could not find Default Material to duplicate its Asset;");

            var aaeId = defaultMat.AppearanceAssetId;
            if (!(doc.GetElement(aaeId) is Autodesk.Revit.DB.AppearanceAssetElement aae))
                throw new Exception("Default Material doesn't have Appearance Asset.");

            TransactionManager.Instance.EnsureInTransaction(doc);

            Autodesk.Revit.DB.AppearanceAssetElement newAae;
            var count = 1;

            Restart:

            try
            {
                newAae = aae.Duplicate($"{name}_{count}");
            }
            catch (Exception)
            {
                count++;
                goto Restart;
            }

            TransactionManager.Instance.TransactionTaskDone();

            return newAae.ToDSType(true);
        }

        /// <summary>
        /// Sets the Generic Properties of an Appearance Asset Element.
        /// </summary>
        /// <param name="appearanceAsset">Appearance Asset to set the color for.</param>
        /// <param name="color">Color that property will be set to.</param>
        /// <param name="glossiness">Glossiness value from 1-100</param>
        /// <param name="metallic">In Revit this is called Highlights. Set to True for "Metallic" otherwise false.</param>
        /// <returns name="appearanceAsset">Appearance Asset with adjusted properties.</returns>
        [NodeCategory("Action")]
        public static Element SetGenericProperties(
            Element appearanceAsset,
            Color color, 
            int glossiness = 50, 
            bool metallic = false)
        {
            if (appearanceAsset == null)
                throw new ArgumentNullException(nameof(appearanceAsset));

            if (color == null)
                throw new ArgumentNullException(nameof(color));

            if (!(appearanceAsset.InternalElement is Autodesk.Revit.DB.AppearanceAssetElement aa))
                throw new Exception("Could not retrieve Appearance Asset from Material.");

            var doc = DocumentManager.Instance.CurrentDBDocument;

            TransactionManager.Instance.EnsureInTransaction(doc);
            using (var editScope = new Autodesk.Revit.DB.Visual.AppearanceAssetEditScope(doc))
            {
                var editableAsset = editScope.Start(aa.Id);
                if (!(editableAsset.FindByName("generic_diffuse") is Autodesk.Revit.DB.Visual.AssetPropertyDoubleArray4d diffuseProperty))
                    throw new Exception("Could not find Color property.");

                diffuseProperty.SetValueAsColor(ColorUtilities.RevitColorByColor(color));

                if (!(editableAsset.FindByName("generic_glossiness") is Autodesk.Revit.DB.Visual.AssetPropertyDouble glossinessProperty))
                    throw new Exception("Could not find Glossiness property.");

                glossinessProperty.Value = glossiness / 100.0;

                if (!(editableAsset.FindByName("generic_is_metal") is Autodesk.Revit.DB.Visual.AssetPropertyBoolean isMetalProperty))
                    throw new Exception("Could not find Metallic property.");

                isMetalProperty.Value = metallic;

                editScope.Commit(true);
            }
            TransactionManager.Instance.TransactionTaskDone();

            return aa.ToDSType(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appearanceAsset"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        [MultiReturn("color", "glossiness", "metallic")]
        public static Dictionary<string, object> GetGenericProperties(Element appearanceAsset)
        {
            if (appearanceAsset == null)
                throw new ArgumentNullException(nameof(appearanceAsset));

            if (!(appearanceAsset.InternalElement is Autodesk.Revit.DB.AppearanceAssetElement aa))
                return new Dictionary<string, object>
                {
                    {"color", null},
                    {"glossiness", null},
                    {"metallic", null}
                };

            var doc = DocumentManager.Instance.CurrentDBDocument;
            Color color = null;
            double? glossiness = null;
            bool? metallic = null;

            TransactionManager.Instance.EnsureInTransaction(doc);
            using (var editScope = new Autodesk.Revit.DB.Visual.AppearanceAssetEditScope(doc))
            {
                var editableAsset = editScope.Start(aa.Id);
                if (editableAsset.FindByName("generic_diffuse") is Autodesk.Revit.DB.Visual.AssetPropertyDoubleArray4d diffuseProperty)
                    color = ColorUtilities.DsColorByColor(diffuseProperty.GetValueAsColor());

                if (editableAsset.FindByName("generic_glossiness") is Autodesk.Revit.DB.Visual.AssetPropertyDouble glossinessProperty)
                    glossiness = glossinessProperty.Value * 100.0;

                if (editableAsset.FindByName("generic_is_metal") is Autodesk.Revit.DB.Visual.AssetPropertyBoolean isMetalProperty)
                    metallic = isMetalProperty.Value;

                editScope.Commit(true);
            }
            TransactionManager.Instance.TransactionTaskDone();

            return new Dictionary<string, object>
            {
                {"color", color},
                {"glossiness", glossiness},
                {"metallic", metallic}
            };
        }

        /// <summary>
        /// Sets the Transparency Properties of an Appearance Asset Element.
        /// </summary>
        /// <param name="appearanceAsset">Appearance Asset to set the color for.</param>
        /// <param name="amount">Value from 1-100</param>
        /// <returns name="appearanceAsset">Appearance Asset with adjusted properties.</returns>
        [NodeCategory("Action")]
        public static Element SetTransparencyProperties(
            Element appearanceAsset,
            int amount = 0)
        {
            if (appearanceAsset == null)
                throw new ArgumentNullException(nameof(appearanceAsset));

            if (!(appearanceAsset.InternalElement is Autodesk.Revit.DB.AppearanceAssetElement aa))
                throw new Exception("Could not retrieve Appearance Asset from Material.");

            var doc = DocumentManager.Instance.CurrentDBDocument;

            TransactionManager.Instance.EnsureInTransaction(doc);
            using (var editScope = new Autodesk.Revit.DB.Visual.AppearanceAssetEditScope(doc))
            {
                var editableAsset = editScope.Start(aa.Id);

                if (!(editableAsset.FindByName("generic_transparency") is Autodesk.Revit.DB.Visual.AssetPropertyDouble genericTransparencyProperty))
                    throw new Exception("Could not find Oblique Reflectivity property.");

                genericTransparencyProperty.Value = amount / 100.0;

                editScope.Commit(true);
            }
            TransactionManager.Instance.TransactionTaskDone();

            return aa.ToDSType(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appearanceAsset"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        [MultiReturn("transparency")]
        public static Dictionary<string, object> GetTransparencyProperties(Element appearanceAsset)
        {
            if (appearanceAsset == null)
                throw new ArgumentNullException(nameof(appearanceAsset));

            if (!(appearanceAsset.InternalElement is Autodesk.Revit.DB.AppearanceAssetElement aa))
                return new Dictionary<string, object>
                {
                    {"transparency", null}
                };

            var doc = DocumentManager.Instance.CurrentDBDocument;
            double? transparency = null;

            TransactionManager.Instance.EnsureInTransaction(doc);
            using (var editScope = new Autodesk.Revit.DB.Visual.AppearanceAssetEditScope(doc))
            {
                var editableAsset = editScope.Start(aa.Id);

                if (editableAsset.FindByName("generic_transparency") is Autodesk.Revit.DB.Visual.AssetPropertyDouble genericTransparencyProperty)
                    transparency = genericTransparencyProperty.Value * 100;

                editScope.Commit(true);
            }
            TransactionManager.Instance.TransactionTaskDone();

            return new Dictionary<string, object>
            {
                {"transparency", transparency}
            };
        }

        /// <summary>
        /// Sets the Reflectivity Properties of an Appearance Asset Element.
        /// </summary>
        /// <param name="appearanceAsset">Appearance Asset to set the color for.</param>
        /// <param name="directReflectivity">Value from 1-100</param>
        /// <param name="obliqueReflectivity">Value from 1-100</param>
        /// <returns name="appearanceAsset">Appearance Asset with adjusted properties.</returns>
        [NodeCategory("Action")]
        public static Element SetReflectivityProperties(
            Element appearanceAsset,
            int directReflectivity = 0,
            int obliqueReflectivity = 0)
        {
            if (appearanceAsset == null)
                throw new ArgumentNullException(nameof(appearanceAsset));

            if (!(appearanceAsset.InternalElement is Autodesk.Revit.DB.AppearanceAssetElement aa))
                throw new Exception("Could not retrieve Appearance Asset from Material.");

            var doc = DocumentManager.Instance.CurrentDBDocument;

            TransactionManager.Instance.EnsureInTransaction(doc);
            using (var editScope = new Autodesk.Revit.DB.Visual.AppearanceAssetEditScope(doc))
            {
                var editableAsset = editScope.Start(aa.Id);

                if (!(editableAsset.FindByName("generic_reflectivity_at_0deg") is Autodesk.Revit.DB.Visual.AssetPropertyDouble directReflectivityProperty))
                    throw new Exception("Could not find Direct Reflectivity property.");

                directReflectivityProperty.Value = directReflectivity / 100.0;

                if (!(editableAsset.FindByName("generic_reflectivity_at_90deg") is Autodesk.Revit.DB.Visual.AssetPropertyDouble obliqueReflectivityProperty))
                    throw new Exception("Could not find Oblique Reflectivity property.");

                obliqueReflectivityProperty.Value = obliqueReflectivity / 100.0;

                editScope.Commit(true);
            }
            TransactionManager.Instance.TransactionTaskDone();

            return aa.ToDSType(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appearanceAsset"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        [MultiReturn("directReflectivity", "obliqueReflectivity")]
        public static Dictionary<string, object> GetReflectivityProperties(Element appearanceAsset)
        {
            if (appearanceAsset == null)
                throw new ArgumentNullException(nameof(appearanceAsset));

            if (!(appearanceAsset.InternalElement is Autodesk.Revit.DB.AppearanceAssetElement aa))
                return new Dictionary<string, object>
                {
                    {"directReflectivity", null},
                    {"obliqueReflectivity", null}
                };

            var doc = DocumentManager.Instance.CurrentDBDocument;
            double? direct = null;
            double? oblique = null;

            TransactionManager.Instance.EnsureInTransaction(doc);
            using (var editScope = new Autodesk.Revit.DB.Visual.AppearanceAssetEditScope(doc))
            {
                var editableAsset = editScope.Start(aa.Id);

                if (editableAsset.FindByName("generic_reflectivity_at_0deg") is Autodesk.Revit.DB.Visual.AssetPropertyDouble directReflectivityProperty)
                    direct = directReflectivityProperty.Value * 100.0;

                if (editableAsset.FindByName("generic_reflectivity_at_90deg") is Autodesk.Revit.DB.Visual.AssetPropertyDouble obliqueReflectivityProperty)
                    oblique = obliqueReflectivityProperty.Value * 100.0;

                editScope.Commit(true);
            }
            TransactionManager.Instance.TransactionTaskDone();

            return new Dictionary<string, object>
            {
                {"directReflectivity", direct},
                {"obliqueReflectivity", oblique}
            };
        }
    }
}

#endif
