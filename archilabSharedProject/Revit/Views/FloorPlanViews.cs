using System;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using DynamoServices;
using Revit.Elements;
using Revit.Elements.Views;
using RevitServices.Persistence;
using RevitServices.Transactions;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Views
{
    /// <summary>
    /// Wrapper around Floor Plan View.
    /// </summary>
    [RegisterForTrace]
    public class FloorPlanViews : View
    {
        internal Autodesk.Revit.DB.ViewPlan InternalViewPlan { get; private set; }

        /// <summary>
        /// Internal Revit API object.
        /// </summary>
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalViewPlan; }
        }

        private FloorPlanViews(Autodesk.Revit.DB.Level level, Autodesk.Revit.DB.ViewFamilyType viewFamilyType)
        {
            SafeInit(() => InitViewPlan(level, viewFamilyType));
        }

        private void InitViewPlan(Autodesk.Revit.DB.Level level, Autodesk.Revit.DB.ViewFamilyType viewFamilyType)
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

            var view = Autodesk.Revit.DB.ViewPlan.Create(doc, viewFamilyType.Id, level.Id);

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
        /// Create a Floor Plan view by Level and View Family Type.
        /// </summary>
        /// <param name="level">Level to associate the Plan to.</param>
        /// <param name="viewFamilyType">View Family Type object.</param>
        /// <returns name="floorPlan">Floor Plan.</returns>
        public static FloorPlanView ByLevelAndType(Level level, [DefaultArgument("Selection.Select.GetNull()")] Element viewFamilyType)
        {
            if (level == null || !(level.InternalElement is Autodesk.Revit.DB.Level lvl))
                throw new ArgumentException(nameof(level));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            Autodesk.Revit.DB.ViewFamilyType vft;
            if (viewFamilyType == null)
            {
                vft = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                    .OfClass(typeof(Autodesk.Revit.DB.ViewFamilyType))
                    .Cast<Autodesk.Revit.DB.ViewFamilyType>()
                    .FirstOrDefault(x => x.ViewFamily == Autodesk.Revit.DB.ViewFamily.FloorPlan);
            }
            else
            {
                vft = viewFamilyType.InternalElement as Autodesk.Revit.DB.ViewFamilyType;
            }

            if (vft?.ViewFamily != Autodesk.Revit.DB.ViewFamily.FloorPlan)
                throw new ArgumentException(nameof(viewFamilyType));

            var plan = new FloorPlanViews(lvl, vft);

            return (FloorPlanView)plan.InternalViewPlan.ToDSType(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="room"></param>
        /// <param name="viewFamilyType"></param>
        /// <returns></returns>
        public static FloorPlanView ByRoom(Room room, [DefaultArgument("Selection.Select.GetNull()")] Element viewFamilyType)
        {
            if (room == null)
                throw new ArgumentException(nameof(room));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            Autodesk.Revit.DB.ViewFamilyType vft;
            if (viewFamilyType == null)
            {
                vft = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                    .OfClass(typeof(Autodesk.Revit.DB.ViewFamilyType))
                    .Cast<Autodesk.Revit.DB.ViewFamilyType>()
                    .FirstOrDefault(x => x.ViewFamily == Autodesk.Revit.DB.ViewFamily.FloorPlan);
            }
            else
            {
                vft = viewFamilyType.InternalElement as Autodesk.Revit.DB.ViewFamilyType;
            }

            if (!(room.InternalElement is Autodesk.Revit.DB.Architecture.Room rm))
                throw new ArgumentException(nameof(room));

            var plan = new FloorPlanViews(rm.Level, vft);

            return (FloorPlanView)plan.InternalViewPlan.ToDSType(true);
        }
    }
}
