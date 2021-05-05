using Dynamo.Graph.Nodes;
using RevitServices.Persistence;
using RevitServices.Transactions;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class Documents
    {
        internal Documents()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="passThrough"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static object Regenerate(object passThrough)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            TransactionManager.Instance.EnsureInTransaction(doc);
            doc.Regenerate();
            TransactionManager.Instance.TransactionTaskDone();

            return passThrough;
        }
    }
}
