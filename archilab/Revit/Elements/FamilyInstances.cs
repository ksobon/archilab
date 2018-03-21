using System;
using DynamoServices;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Line = Autodesk.DesignScript.Geometry.Line;

namespace archilab.Revit.Elements
{
    /// <summary>
    /// Wrapper class for Family Instancess.
    /// </summary>
    [RegisterForTrace]
    public class FamilyInstances : AbstractFamilyInstance
    {
        internal Autodesk.Revit.DB.FamilyInstance InternalFamilyInstance { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        protected FamilyInstances(Autodesk.Revit.DB.FamilyInstance instance)
        {
            SafeInit(() => InitFamilyInstance(instance));
        }

        internal FamilyInstances(Autodesk.Revit.DB.FamilySymbol fs, Autodesk.Revit.DB.Line line, Autodesk.Revit.DB.Level level)
        {
            SafeInit(() => InitFamilyInstance(fs, line, level));
        }

        private void InitFamilyInstance(Autodesk.Revit.DB.FamilyInstance instance)
        {
            InternalSetFamilyInstance(instance);
        }

        private void InitFamilyInstance(Autodesk.Revit.DB.FamilySymbol fs, Autodesk.Revit.DB.Line line, Autodesk.Revit.DB.Level level)
        {
            //Phase 1 - Check to see if the object exists and should be rebound
            var oldFam = ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.FamilyInstance>(DocumentManager.Instance.CurrentDBDocument);

            //There was a point, rebind to that, and adjust its position
            if (oldFam != null)
            {
                InternalSetFamilyInstance(oldFam);
                InternalSetLevel(level);
                InternalSetFamilySymbol(fs);
                InternalSetPosition(line);
                return;
            }

            //Phase 2- There was no existing point, create one
            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);

            //If the symbol is not active, then activate it
            if (!fs.IsActive)
                fs.Activate();

            var fi = DocumentManager.Instance.CurrentDBDocument.Create.NewFamilyInstance(line, fs, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
            ((Autodesk.Revit.DB.LocationCurve)fi.Location).Curve = line;

            InternalSetFamilyInstance(fi);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(InternalElement);
        }

        private void InternalSetLevel(Autodesk.Revit.DB.Level level)
        {
            if (InternalFamilyInstance.LevelId.Compare(level.Id) == 0)
                return;

            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);

            InternalFamilyInstance.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.FAMILY_LEVEL_PARAM).Set(level.Id);

            TransactionManager.Instance.TransactionTaskDone();
        }

        private void InternalSetPosition(Autodesk.Revit.DB.Curve pos)
        {
            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);

            var lp = InternalFamilyInstance.Location as Autodesk.Revit.DB.LocationCurve;

            if (lp != null && lp.Curve != pos) lp.Curve = pos;

            TransactionManager.Instance.TransactionTaskDone();
        }

        /// <summary>
        /// New Family Instance by Curve
        /// </summary>
        /// <param name="familyType">Family Type to be applied to new Family Instance.</param>
        /// <param name="line">Line to place Family Instance at.</param>
        /// <param name="level">Level to associate Family Instance with.</param>
        /// <returns>New Family Instance.</returns>
        public static Element ByLine(FamilyType familyType, Line line, Level level)
        {
            if (familyType == null)
            {
                throw new ArgumentNullException(nameof(familyType));
            }

            var symbol = familyType.InternalElement as Autodesk.Revit.DB.FamilySymbol;
            var locationLine = line.ToRevitType() as Autodesk.Revit.DB.Line;
            var hostLevel = level.InternalElement as Autodesk.Revit.DB.Level;

            return new FamilyInstances(symbol, locationLine, hostLevel);
        }
    }
}
