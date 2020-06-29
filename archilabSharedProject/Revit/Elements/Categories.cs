using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Graph.Nodes;
using Autodesk.DesignScript.Runtime;
using RevitServices.Persistence;
using Revit.Elements;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class Categories
    {
        internal Categories()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [NodeCategory("Query")]
        [MultiReturn("Model", "Annotation", "Analytical")]
        public static Dictionary<string, object> GetAll()
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var modelCats = new List<Category>();
            var annotationCats = new List<Category>();
            var analyticalCats = new List<Category>();
            foreach (Autodesk.Revit.DB.Category c in doc.Settings.Categories)
            {
                if (c.Parent != null)
                    continue;

                switch (c.CategoryType)
                {
                    case Autodesk.Revit.DB.CategoryType.Invalid:
                        break;
                    case Autodesk.Revit.DB.CategoryType.Model:
                        modelCats.Add(Category.ById(c.Id.IntegerValue));
                        break;
                    case Autodesk.Revit.DB.CategoryType.Annotation:
                        annotationCats.Add(Category.ById(c.Id.IntegerValue));
                        break;
                    case Autodesk.Revit.DB.CategoryType.Internal:
                        break;
                    case Autodesk.Revit.DB.CategoryType.AnalyticalModel:
                        analyticalCats.Add(Category.ById(c.Id.IntegerValue));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return new Dictionary<string, object>
            {
                {"Model", modelCats.OrderBy(x => x.Name)},
                {"Annotation", annotationCats.OrderBy(x => x.Name)},
                {"Analytical", analyticalCats.OrderBy(x => x.Name)}
            };
        }
    }
}
