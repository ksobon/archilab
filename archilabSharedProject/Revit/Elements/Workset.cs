using Autodesk.DesignScript.Runtime;
using DynamoServices;
using RevitServices.Persistence;
using RevitServices.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// Workset wrapper class.
    /// </summary>
    [RegisterForTrace]
    public class Workset
    {
        #region Constructors

        private Autodesk.Revit.DB.WorksetId _internalId;

        /// <summary>
        /// 
        /// </summary>
        protected Autodesk.Revit.DB.WorksetId InternalWorksetId
        {
            get
            {
                return _internalId ?? InternalWorkset?.Id;
            }
            set
            {
                _internalId = value;
            }
        }

        internal Autodesk.Revit.DB.Workset InternalWorkset
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workset"></param>
        [SupressImportIntoVM]
        public Workset(Autodesk.Revit.DB.Workset workset)
        {
            InitWorkset(workset);
        }

        private Workset(string name)
        {
            InitWorkset(name);
        }

        private void InitWorkset(Autodesk.Revit.DB.Workset workset)
        {
            InternalSetWorkset(workset);
        }

        private void InternalSetWorkset(Autodesk.Revit.DB.Workset workset)
        {
            InternalWorkset = workset;
            InternalWorksetId = workset.Id;
        }

        private void InitWorkset(string name)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;

            TransactionManager.Instance.EnsureInTransaction(doc);
            var w = Autodesk.Revit.DB.Workset.Create(doc, name);
            InternalSetWorkset(w);
            TransactionManager.Instance.TransactionTaskDone();
        }

        #endregion

        /// <summary>
        /// Create Workset by Name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Workset ByName(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            var w = new Autodesk.Revit.DB.FilteredWorksetCollector(doc)
                .OfKind(Autodesk.Revit.DB.WorksetKind.UserWorkset)
                .FirstOrDefault(x => x.Name == name);

            var output = w == null
                ? new Workset(name)
                : new Workset(w);

            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workset"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Workset Rename(Workset workset, string name)
        {
            if (workset == null) throw new ArgumentNullException(nameof(workset));
            if (name == null) throw new ArgumentNullException(nameof(name));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            var w = workset.InternalWorkset;

            TransactionManager.Instance.EnsureInTransaction(doc);
            Autodesk.Revit.DB.WorksetTable.RenameWorkset(doc, w.Id, name);
            TransactionManager.Instance.TransactionTaskDone();

            return workset;
        }

        /// <summary>
        /// Retrieves all user created Worksets.
        /// </summary>
        /// <returns name="Workset">Worksets</returns>
        public static IList<Workset> GetAll()
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var w = new Autodesk.Revit.DB.FilteredWorksetCollector(doc)
                .OfKind(Autodesk.Revit.DB.WorksetKind.UserWorkset)
                .Select(x => new Workset(x))
                .ToList();

            return w.Any() ? w : Enumerable.Empty<Workset>().ToList();
        }

        #region Properties

        /// <summary>
        /// Workset Name
        /// </summary>
        public string Name
        {
            get { return InternalWorkset.Name; }
        }

        /// <summary>
        /// Workset Id
        /// </summary>
        public int Id
        {
            get { return InternalWorksetId.IntegerValue; }
        }

        /// <summary>
        /// Workset Guid
        /// </summary>
        public string Guid
        {
            get { return InternalWorkset.UniqueId.ToString(); }
        }

        #endregion

        /// <summary>
        /// String override for Workset.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Workset(Name: {Name})";
        }
    }
}
