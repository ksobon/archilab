using System;
using System.Linq;
using Revit.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace archilab.Revit.Elements
{
    /// <summary>
    /// Methods and properties typically associated with Element Types in Revit
    /// </summary>
    public class ElementTypes
    {
        internal ElementTypes()
        {
        }

        /// <summary>
        /// Duplicates Element Type given a name doesn't exist. If it does it will return Element Type with that name.
        /// </summary>
        /// <param name="element">Element Type to duplicate.</param>
        /// <param name="name">Name of Duplicated Type. Must be unique.</param>
        /// <returns>New Element Type or Type with the given name if already exists.</returns>
        public static Element Duplicate(Element element, string name)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var e = element.InternalElement as Autodesk.Revit.DB.ElementType;

            if (e == null)
            {
                throw new Exception("Only Element Types can be duplicated.");
            }

            try
            {
                TransactionManager.Instance.EnsureInTransaction(doc);
                var newType = e.Duplicate(name);
                TransactionManager.Instance.TransactionTaskDone();

                return newType.ToDSType(true);
            }
            catch (Autodesk.Revit.Exceptions.ArgumentException)
            {
                var viewFam = DocumentManager.Instance.ElementsOfType<Autodesk.Revit.DB.ViewFamilyType>()
                    .FirstOrDefault(x => x.Name == name);
                return viewFam.ToDSType(true);
            }
            catch (Exception ex)
            {
                throw  new Exception("Failed duplicating: " + ex.Message);
            }
        }
    }
}
