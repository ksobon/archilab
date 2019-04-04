using System;
using Autodesk.DesignScript.Runtime;
using RevitServices.Persistence;
using RevitServices.Transactions;

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

        internal Autodesk.Revit.DB.DisplayUnitType InternalDisplayUnitType
        {
            get;
            private set;
        }

        internal FormatOptions()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dut"></param>
        [SupressImportIntoVM]
        public FormatOptions(Autodesk.Revit.DB.DisplayUnitType dut)
        {
            InitFormatOptions(dut);
        }

        private void InitFormatOptions(Autodesk.Revit.DB.DisplayUnitType dut)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;

            TransactionManager.Instance.EnsureInTransaction(doc);
            var fo = new Autodesk.Revit.DB.FormatOptions(dut);
            InternalSetFormatOptions(fo);
            TransactionManager.Instance.TransactionTaskDone();
        }

        private void InternalSetFormatOptions(Autodesk.Revit.DB.FormatOptions options)
        {
            InternalFormatOptions = options;
            InternalDisplayUnitType = options.DisplayUnits;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static FormatOptions Create(string displayUnitType)
        {
            if (string.IsNullOrWhiteSpace(displayUnitType))
                throw new ArgumentException(nameof(displayUnitType));

            var dut = (Autodesk.Revit.DB.DisplayUnitType)Enum.Parse(typeof(Autodesk.Revit.DB.DisplayUnitType), displayUnitType);

            return new FormatOptions(dut);
        }
    }
}
