using System;
using Autodesk.DesignScript.Runtime;
using RevitServices.Persistence;
using RevitServices.Transactions;

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
            var us = (Autodesk.Revit.DB.UnitSystem) Enum.Parse(typeof(Autodesk.Revit.DB.UnitSystem), unitSystem);

            return new Units(us);
        }

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
