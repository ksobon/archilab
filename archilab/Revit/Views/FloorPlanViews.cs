using System;
using DynamoServices;
using Revit.Elements;
using Revit.Elements.Views;
using RevitServices.Persistence;
using RevitServices.Transactions;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Views
{
    /// <summary>
    /// 
    /// </summary>
    [RegisterForTrace]
    public class FloorPlanViews : View
    {
        internal Autodesk.Revit.DB.ViewPlan InternalViewPlan { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalViewPlan; }
        }

        private FloorPlanViews(Level level, Element viewFamilyType)
        {
            SafeInit(() => InitViewPlan(level, viewFamilyType));
        }

        private void InitViewPlan(Level level, Element viewFamilyType)
        {
            // Phase 1 - Check to see if the object exists and should be rebound
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var oldEle = ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.ViewPlan>(doc);

            // Rebind to Element
            if (oldEle != null)
            {
                InternalSetViewPlan(oldEle);
                return;
            }

            // Phase 2 - There was no existing Element, create new one
            TransactionManager.Instance.EnsureInTransaction(doc);

            var view = Autodesk.Revit.DB.ViewPlan.Create(doc, viewFamilyType.InternalElement.Id, level.InternalElement.Id);

            InternalSetViewPlan(view);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(InternalElement);
        }

        private void InternalSetViewPlan(Autodesk.Revit.DB.ViewPlan view)
        {
            InternalViewPlan = view;
            InternalElementId = view.Id;
            InternalUniqueId = view.UniqueId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <param name="viewFamilyType"></param>
        /// <returns></returns>
        public static FloorPlanView ByLevelAndType(Level level, Element viewFamilyType)
        {
            if (level == null)
                throw new ArgumentException(nameof(level));

            if (viewFamilyType == null)
                throw new ArgumentException(nameof(viewFamilyType));

            if (!(viewFamilyType.InternalElement is Autodesk.Revit.DB.ViewFamilyType vft) || vft.ViewFamily != Autodesk.Revit.DB.ViewFamily.FloorPlan)
                throw new ArgumentException(nameof(viewFamilyType));

            var plan = new FloorPlanViews(level, viewFamilyType);

            return (FloorPlanView)plan.InternalViewPlan.ToDSType(true);
        }
    }
}
