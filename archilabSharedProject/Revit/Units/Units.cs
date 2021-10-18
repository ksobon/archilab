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
    public class Units
    {
        internal Autodesk.Revit.DB.Units InternalUnits
        {
            get;
            private set;
        }

        internal Autodesk.Revit.DB.UnitSystem InternalUnitSystem
        {
            get;
            private set;
        }

        internal Units()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="units"></param>
        [SupressImportIntoVM]
        public Units(Autodesk.Revit.DB.Units units)
        {
            InitUnits(units);
        }

        private Units(Autodesk.Revit.DB.UnitSystem unitSystem)
        {
            InitUnits(unitSystem);
        }

        private void InitUnits(Autodesk.Revit.DB.Units units)
        {
            InternalSetUnits(units);
        }

        private void InitUnits(Autodesk.Revit.DB.UnitSystem unitSystem)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;

            TransactionManager.Instance.EnsureInTransaction(doc);
            var w = new Autodesk.Revit.DB.Units(unitSystem);
            InternalSetUnits(w);
            TransactionManager.Instance.TransactionTaskDone();
        }

        private void InternalSetUnits(Autodesk.Revit.DB.Units units)
        {
            InternalUnits = units;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitSystem"></param>
        /// <returns></returns>
        public static Units Create(string unitSystem)
        {
            if (string.IsNullOrWhiteSpace(unitSystem))
                throw new ArgumentException(nameof(unitSystem));

            var us = (Autodesk.Revit.DB.UnitSystem) Enum.Parse(typeof(Autodesk.Revit.DB.UnitSystem), unitSystem);

            return new Units(us);
        }
#if !Revit2018 && !Revit2019 && !Revit2020 && !Revit2021

        /// <summary>
        /// 
        /// </summary>
        /// <param name="units"></param>
        /// <param name="forgeSpec"></param>
        /// <param name="formatOptions"></param>
        /// <returns></returns>
        public static Units SetFormatOptions(Units units, string forgeSpec, FormatOptions formatOptions)
        {
            if (units == null)
                throw new ArgumentException(nameof(units));
            if (string.IsNullOrWhiteSpace(forgeSpec))
                throw new ArgumentException(nameof(forgeSpec));
            if (formatOptions == null)
                throw new ArgumentException(nameof(formatOptions));

            var ut = new Autodesk.Revit.DB.ForgeTypeId(forgeSpec);
            var fo = formatOptions.InternalFormatOptions;

            units.InternalUnits.SetFormatOptions(ut, fo);

            return units;
        }
#else
        /// <summary>
        /// 
        /// </summary>
        /// <param name="units"></param>
        /// <param name="unitType"></param>
        /// <param name="formatOptions"></param>
        /// <returns></returns>
        public static Units SetFormatOptions(Units units, string unitType, FormatOptions formatOptions)
        {
            if (units == null)
                throw new ArgumentException(nameof(units));
            if (string.IsNullOrWhiteSpace(unitType))
                throw new ArgumentException(nameof(unitType));
            if (formatOptions == null)
                throw new ArgumentException(nameof(formatOptions));

            var ut = (Autodesk.Revit.DB.UnitType)Enum.Parse(typeof(Autodesk.Revit.DB.UnitType), unitType);
            var fo = formatOptions.InternalFormatOptions;

            units.InternalUnits.SetFormatOptions(ut, fo);

            return units;
        }
#endif
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static Units GetInternal()
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;

            return new Units(doc.GetUnits());
        }
    }
}
