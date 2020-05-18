using System.Linq;
using DynamoServices;
using Revit.Elements.Views;
using RevitServices.Persistence;
using RevitServices.Transactions;
using System;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Views
{
    /// <summary>
    /// 
    /// </summary>
    [RegisterForTrace]
    public class ThreeD : View3D
    {
        internal Autodesk.Revit.DB.View3D InternalView3D { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        protected ThreeD(Autodesk.Revit.DB.View3D view)
        {
            SafeInit(() => InitView3D(view));
        }

        internal ThreeD(string name)
        {
            SafeInit(() => InitView3D(name));
        }

        private void InitView3D(Autodesk.Revit.DB.View3D view)
        {
            InternalSetView3D(view);
        }

        private void InitView3D(string name)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var oldView = ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.View3D>(doc);
            if (oldView != null)
            {
                InternalSetView3D(oldView);
                InternalSetName(name);
                return;
            }

            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);

            var vft = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                .OfClass(typeof(Autodesk.Revit.DB.ViewFamilyType))
                .Cast<Autodesk.Revit.DB.ViewFamilyType>()
                .FirstOrDefault(x => x.ViewFamily == Autodesk.Revit.DB.ViewFamily.ThreeDimensional);
            var view = Autodesk.Revit.DB.View3D.CreateIsometric(doc, vft?.Id);
            view.Name = name;

            InternalSetView3D(view);
            InternalSetName(name);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(InternalElement);
        }

        /// <summary>
        /// Creates a Three Dimensional Isometric View by a name.
        /// </summary>
        /// <param name="name">Name of the View.</param>
        /// <returns>View that was created.</returns>
        public static View ByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            return new ThreeD(name);
        }
    }
}
