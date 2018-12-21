using System;
using System.Collections.Generic;
using System.Linq;
using Revit.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class Revisions
    {
        internal Revisions()
        {
        }

        /// <summary>
        /// Sets revisions in a sequence that they are supplied.
        /// </summary>
        /// <param name="revisions">Revisions in sequence to be set.</param>
        /// <returns name="revisions">Revisions in new sequence.</returns>
        public static List<Element> SetSequence(List<Element> revisions)
        {
            if(revisions == null) throw new ArgumentNullException(nameof(revisions));
            if(!revisions.Any()) throw new ArgumentException(nameof(revisions));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            var ids = revisions.Select(x => x.InternalElement.Id).ToList();

            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);
            Autodesk.Revit.DB.Revision.ReorderRevisionSequence(doc, ids);
            TransactionManager.Instance.TransactionTaskDone();

            return revisions;
        }
    }
}
