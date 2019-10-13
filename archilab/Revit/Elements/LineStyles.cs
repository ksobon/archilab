using System;
using DSCore;
using Revit.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class LineStyles
    {
        internal LineStyles()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lineStyle"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Element SetColor(string lineStyle, Color color)
        {
            if (string.IsNullOrWhiteSpace(lineStyle)) throw new ArgumentNullException(nameof(lineStyle));
            if (color == null) throw new ArgumentNullException(nameof(color));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            var cat = Autodesk.Revit.DB.Category.GetCategory(doc, Autodesk.Revit.DB.BuiltInCategory.OST_Lines);
            var gs = cat.GetGraphicsStyle(Autodesk.Revit.DB.GraphicsStyleType.Projection);
            var gsCat = gs.GraphicsStyleCategory.SubCategories;
            var style = gsCat.get_Item(lineStyle);
            if (style == null) throw new Exception("Could not find Line Style with matching name.");

            TransactionManager.Instance.EnsureInTransaction(doc);
            style.LineColor = new Autodesk.Revit.DB.Color(color.Red, color.Green, color.Blue);
            TransactionManager.Instance.TransactionTaskDone();

            return style.GetGraphicsStyle(Autodesk.Revit.DB.GraphicsStyleType.Projection).ToDSType(true);
        }
    }
}
