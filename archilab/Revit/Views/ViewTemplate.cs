using System.Linq;
using Revit.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace archilab.Revit.Views
{
    /// <summary>
    /// View Template related utilities.
    /// </summary>
    public class ViewTemplate
    {
        internal ViewTemplate()
        {
        }

        /// <summary>
        /// Creates a copy of View Template.
        /// </summary>
        /// <param name="viewTemplate">View Template to be duplicated.</param>
        /// <param name="name">Name of the new View Template.</param>
        /// <returns name="view">View Template</returns>
        public static global::Revit.Elements.Views.View Duplicate(global::Revit.Elements.Views.View viewTemplate, string name)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var v = (Autodesk.Revit.DB.View)viewTemplate.InternalElement;

            TransactionManager.Instance.EnsureInTransaction(doc);
            var newId = Autodesk.Revit.DB.ElementTransformUtils.CopyElement(doc, v.Id, Autodesk.Revit.DB.XYZ.Zero);
            var newView = (Autodesk.Revit.DB.View)doc.GetElement(newId.FirstOrDefault());
            newView.Name = name;
            TransactionManager.Instance.TransactionTaskDone();

            return newView.ToDSType(true) as global::Revit.Elements.Views.View;
        }
    }
}
