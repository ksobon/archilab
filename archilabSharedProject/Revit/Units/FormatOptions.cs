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

#if !Revit2018 && !Revit2019 && !Revit2020 && !Revit2021
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
#else
        internal Autodesk.Revit.DB.DisplayUnitType InternalDisplayUnitType
        {
            get;
            private set;
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
#endif
    }
}
