using System;
using System.Collections.Generic;
using System.Linq;
using DynamoServices;
using Revit.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    [RegisterForTrace]
    public class Revisions : Element
    {
        internal Autodesk.Revit.DB.Revision InternalRevision
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalRevision; }
        }

        private Revisions()
        {
            SafeInit(InitRevision);
        }

        private void InternalSetRevision(Autodesk.Revit.DB.Revision r)
        {
            InternalRevision = r;
        }

        private void InitRevision()
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            
            //Phase 1 - Check to see if the object exists and should be rebound
            var oldEle = ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.Revision>(doc);

            // Rebind to Element
            if (oldEle != null)
            {
                InternalSetRevision(oldEle);
                return;
            }

            //Phase 2 - There was no existing Element, create new one
            TransactionManager.Instance.EnsureInTransaction(doc);

            var r = Autodesk.Revit.DB.Revision.Create(doc);

            InternalSetRevision(r);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(InternalElement);
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

        /// <summary>
        /// Create a new Revision.
        /// </summary>
        /// <param name="revisionDate">Revision Date.</param>
        /// <param name="description">Revision Description.</param>
        /// <param name="issued">Revision Issued.</param>
        /// <param name="issuedBy">Revision Issued By.</param>
        /// <param name="issuedTo">Revision Issued To.</param>
        /// <param name="numberType">Revision Number Type.</param>
        /// <param name="visibility">Revision Visibility.</param>
        /// <returns name="revision">Newly created Revision.</returns>
        public static Element Create(
            string revisionDate = "",
            string description = "",
            bool issued = false,
            string issuedBy = "", 
            string issuedTo = "", 
            string numberType = "Alphanumeric",
            string visibility = "CloudAndTagVisible")
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;

            var nType = (Autodesk.Revit.DB.RevisionNumberType)Enum.Parse(typeof(Autodesk.Revit.DB.RevisionNumberType), numberType);
            var vis = (Autodesk.Revit.DB.RevisionVisibility)Enum.Parse(typeof(Autodesk.Revit.DB.RevisionVisibility), visibility);

            TransactionManager.Instance.EnsureInTransaction(doc);

            var rev = new Revisions().InternalRevision;
            rev.Issued = issued;

            if (!string.IsNullOrWhiteSpace(revisionDate))
                rev.RevisionDate = revisionDate;
            if (!string.IsNullOrWhiteSpace(description))
                rev.Description = description;
            if (!string.IsNullOrWhiteSpace(issuedBy))
                rev.IssuedBy = issuedBy;
            if (!string.IsNullOrWhiteSpace(issuedTo))
                rev.IssuedTo = issuedTo;
            if (!string.IsNullOrWhiteSpace(numberType))
                rev.NumberType = nType;
            if (!string.IsNullOrWhiteSpace(visibility))
                rev.Visibility = vis;

            TransactionManager.Instance.TransactionTaskDone();

            return rev.ToDSType(true);
        }
    }
}
