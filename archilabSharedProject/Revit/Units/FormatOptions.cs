using System;
using Autodesk.DesignScript.Runtime;
using RevitServices.Persistence;
using RevitServices.Transactions;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Units
{
    /// <summary>
    /// 
    /// </summary>
    public class FormatOptions
    {
        internal Autodesk.Revit.DB.FormatOptions InternalFormatOptions
        {
            get;
            private set;
        }


        internal FormatOptions()
        {
        }

        internal Autodesk.Revit.DB.ForgeTypeId InternalDisplayUnitType
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fu"></param>
        [SupressImportIntoVM]
        public FormatOptions(Autodesk.Revit.DB.ForgeTypeId fu)
        {
            InitFormatOptions(fu);
        }

        private void InitFormatOptions(Autodesk.Revit.DB.ForgeTypeId fu)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;

            TransactionManager.Instance.EnsureInTransaction(doc);
            var fo = new Autodesk.Revit.DB.FormatOptions(fu);
            InternalSetFormatOptions(fo);
            TransactionManager.Instance.TransactionTaskDone();
        }

        private void InternalSetFormatOptions(Autodesk.Revit.DB.FormatOptions options)
        {
            InternalFormatOptions = options;
            InternalDisplayUnitType = options.GetUnitTypeId();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="forgeUnit"></param>
        /// <returns></returns>
        public static FormatOptions Create(string forgeUnit)
        {
            if (string.IsNullOrWhiteSpace(forgeUnit))
                throw new ArgumentException(nameof(forgeUnit));

            var fu = new Autodesk.Revit.DB.ForgeTypeId(forgeUnit);
            return new FormatOptions(fu);
        }
    }
}
