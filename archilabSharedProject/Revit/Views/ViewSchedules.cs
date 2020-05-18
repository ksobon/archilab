using Revit.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Views
{
    /// <summary>
    /// 
    /// </summary>
    public class ScheduleViews
    {
        internal ScheduleViews()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="isItemized"></param>
        /// <returns></returns>
        public static global::Revit.Elements.Views.ScheduleView IsItemized(global::Revit.Elements.Views.ScheduleView view, bool isItemized = true)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var v = (Autodesk.Revit.DB.ViewSchedule)view.InternalElement;

            TransactionManager.Instance.EnsureInTransaction(doc);
            v.Definition.IsItemized = isItemized;
            TransactionManager.Instance.TransactionTaskDone();

            return v.ToDSType(true) as global::Revit.Elements.Views.ScheduleView;
        }
    }
}
