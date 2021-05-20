using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using archilab.Utilities;
using Autodesk.DesignScript.Geometry;
using Dynamo.Graph.Nodes;
using DynamoServices;
using Revit.Elements;
using Revit.Elements.Views;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Element = Revit.Elements.Element;
using FamilyType = Revit.Elements.FamilyType;
using Level = Revit.Elements.Level;
using Line = Autodesk.DesignScript.Geometry.Line;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// Wrapper class for Family Instances.
    /// </summary>
    [RegisterForTrace]
    public class FamilyInstances : AbstractFamilyInstance
    {
        internal Autodesk.Revit.DB.FamilyInstance InternalFamilyInstance { get; set; }

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

        internal FamilyInstances(Autodesk.Revit.DB.FamilySymbol fs, Autodesk.Revit.DB.XYZ point, Autodesk.Revit.DB.View view)
        {
            SafeInit(() => InitFamilyInstance(fs, point, view));
        } 
        
        internal FamilyInstances(Autodesk.Revit.DB.FamilySymbol fs, Autodesk.Revit.DB.XYZ point, Autodesk.Revit.DB.Element host, Autodesk.Revit.DB.Level level)
        {
            SafeInit(() => InitFamilyInstance(fs, point, host, level));
        }
        
        internal FamilyInstances(Autodesk.Revit.DB.FamilySymbol fs, Autodesk.Revit.DB.XYZ point, Autodesk.Revit.DB.Reference reference, Autodesk.Revit.DB.XYZ referenceDir)
        {
            SafeInit(() => InitFamilyInstance(fs, point, reference, referenceDir));
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
            if (!fs.IsActive) fs.Activate();

            var fi = DocumentManager.Instance.CurrentDBDocument.Create.NewFamilyInstance(line, fs, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
            ((Autodesk.Revit.DB.LocationCurve)fi.Location).Curve = line;

            InternalSetFamilyInstance(fi);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(InternalElement);
        }

        private void InitFamilyInstance(Autodesk.Revit.DB.FamilySymbol fs, Autodesk.Revit.DB.XYZ point, Autodesk.Revit.DB.View view)
        {
            //Phase 1 - Check to see if the object exists and should be rebound
            var oldFam = ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.FamilyInstance>(DocumentManager.Instance.CurrentDBDocument);

            //There was a point, rebind to that, and adjust its position
            if (oldFam != null)
            {
                InternalSetFamilyInstance(oldFam);
                InternalSetFamilySymbol(fs);
                return;
            }

            //Phase 2- There was no existing point, create one
            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);

            //If the symbol is not active, then activate it
            if (!fs.IsActive)
                fs.Activate();

            var fi = DocumentManager.Instance.CurrentDBDocument.Create.NewFamilyInstance(point, fs, view);

            InternalSetFamilyInstance(fi);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(InternalElement);
        }

        private void InitFamilyInstance(Autodesk.Revit.DB.FamilySymbol fs, Autodesk.Revit.DB.XYZ point, Autodesk.Revit.DB.Element host, Autodesk.Revit.DB.Level level) 
        {
            //Phase 1 - Check to see if the object exists and should be rebound
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var oldFam = ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.FamilyInstance>(doc);

            //There was a point, rebind to that, and adjust its position
            if (oldFam != null)
            {
                InternalSetFamilyInstance(oldFam);
                InternalSetFamilySymbol(fs);
                return;
            }

            //Phase 2- There was no existing point, create one
            TransactionManager.Instance.EnsureInTransaction(doc);

            //If the symbol is not active, then activate it
            if (!fs.IsActive)
                fs.Activate();

            var fi = doc.Create.NewFamilyInstance(point, fs, host, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

            InternalSetFamilyInstance(fi);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(InternalElement);
        }

        private void InitFamilyInstance(Autodesk.Revit.DB.FamilySymbol fs, Autodesk.Revit.DB.XYZ point, Autodesk.Revit.DB.Reference reference, Autodesk.Revit.DB.XYZ referenceDir)
        {
            //Phase 1 - Check to see if the object exists and should be rebound
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var oldFam = ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.FamilyInstance>(doc);

            //There was a point, rebind to that, and adjust its position
            if (oldFam != null)
            {
                InternalSetFamilyInstance(oldFam);
                InternalSetFamilySymbol(fs);
                return;
            }

            //Phase 2- There was no existing point, create one
            TransactionManager.Instance.EnsureInTransaction(doc);

            //If the symbol is not active, then activate it
            if (!fs.IsActive)
                fs.Activate();

            var fi = doc.Create.NewFamilyInstance(reference, point, referenceDir, fs);

            InternalSetFamilyInstance(fi);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(InternalElement);
        }

        private void InternalSetLevel(Autodesk.Revit.DB.Level level)
        {
            if (InternalFamilyInstance.LevelId.Compare(level.Id) == 0) return;

            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);

            InternalFamilyInstance.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.FAMILY_LEVEL_PARAM).Set(level.Id);

            TransactionManager.Instance.TransactionTaskDone();
        }

        private void InternalSetPosition(Autodesk.Revit.DB.Curve pos)
        {
            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);

            if (InternalFamilyInstance.Location is Autodesk.Revit.DB.LocationCurve lp && lp.Curve != pos) lp.Curve = pos;

            TransactionManager.Instance.TransactionTaskDone();
        }

        /// <summary>
        /// New Family Instance by Curve
        /// </summary>
        /// <param name="familyType">Family Type to be applied to new Family Instance.</param>
        /// <param name="line">Line to place Family Instance at.</param>
        /// <param name="level">Level to associate Family Instance with.</param>
        /// <returns>New Family Instance.</returns>
        [NodeCategory("Action")]
        public static Element ByLine(FamilyType familyType, Line line, Level level)
        {
            if (familyType == null)
            {
                throw new ArgumentNullException(nameof(familyType));
            }

            var symbol = familyType.InternalElement as Autodesk.Revit.DB.FamilySymbol;
            var locationLine = line.ToRevitType() as Autodesk.Revit.DB.Line;
            var hostLevel = level.InternalElement as Autodesk.Revit.DB.Level;

            return new FamilyInstances(symbol, locationLine, hostLevel).InternalElement.ToDSType(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="familyType"></param>
        /// <param name="point"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static Element ByView(FamilyType familyType, Point point, View view)
        {
            if (familyType == null)
                throw new ArgumentNullException(nameof(familyType));
            if (point == null)
                throw new ArgumentNullException(nameof(point));
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var symbol = familyType.InternalElement as Autodesk.Revit.DB.FamilySymbol;
            var pt = point.ToRevitType();
            var v = view.InternalElement as Autodesk.Revit.DB.View;

            return new FamilyInstances(symbol, pt, v).InternalElement.ToDSType(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="familyType"></param>
        /// <param name="point"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static Element ByHostAndPoint(FamilyType familyType, Point point, Element host)
        {
            if (familyType == null)
                throw new ArgumentNullException(nameof(familyType));
            if (point == null)
                throw new ArgumentNullException(nameof(point));
            if (host == null)
                throw new ArgumentNullException(nameof(host));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            var symbol = familyType.InternalElement as Autodesk.Revit.DB.FamilySymbol;
            var pt = point.ToRevitType();
            var h = host.InternalElement;
            var level = doc.GetElement(h.LevelId) as Autodesk.Revit.DB.Level;

            return new FamilyInstances(symbol, pt, h, level).InternalElement.ToDSType(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="familyType"></param>
        /// <param name="point"></param>
        /// <param name="surface"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static Element ByFaceAndPoint(FamilyType familyType, Point point, Surface surface)
        {
            if (familyType == null)
                throw new ArgumentNullException(nameof(familyType));
            if (point == null)
                throw new ArgumentNullException(nameof(point));
            if (surface == null)
                throw new ArgumentNullException(nameof(surface));

            var symbol = familyType.InternalElement as Autodesk.Revit.DB.FamilySymbol;
            var pt = point.ToRevitType();
            var reference = surface.Tags.LookupTag("RevitFaceReference") as Autodesk.Revit.DB.Reference;
            var faceNormal = surface.NormalAtPoint(point);
            var up = Vector.ZAxis();

            Autodesk.Revit.DB.XYZ referenceDir;
            if (Math.Abs(faceNormal.Dot(up)) > 0.9999) // horizontal
                referenceDir = Autodesk.Revit.DB.XYZ.BasisX;
            else
                referenceDir = faceNormal.Cross(up).ToXyz();

            return new FamilyInstances(symbol, pt, reference, referenceDir).InternalElement.ToDSType(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static Element FlipFacingOrientation(Element element)
        {
            if (!(element.InternalElement is Autodesk.Revit.DB.FamilyInstance e))
                throw new ArgumentNullException(nameof(element));

            if (!e.CanFlipFacing)
                return element;

            var doc = DocumentManager.Instance.CurrentDBDocument;
            TransactionManager.Instance.EnsureInTransaction(doc);
            e.flipFacing();
            TransactionManager.Instance.TransactionTaskDone();

            return element;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static Element FlipHandOrientation(Element element)
        {
            if (!(element.InternalElement is Autodesk.Revit.DB.FamilyInstance e))
                throw new ArgumentNullException(nameof(element));

            if (!e.CanFlipHand)
                return element;

            var doc = DocumentManager.Instance.CurrentDBDocument;
            TransactionManager.Instance.EnsureInTransaction(doc);
            e.flipHand();
            TransactionManager.Instance.TransactionTaskDone();

            return element;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static List<Connectors> Connectors(Element element)
        {
            if (!(element.InternalElement is Autodesk.Revit.DB.FamilyInstance e))
                throw new ArgumentNullException(nameof(element));

            return (from Autodesk.Revit.DB.Connector conn in e.MEPModel.ConnectorManager.Connectors
                select new Connectors(conn)).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static List<Element> SubComponents(Element element)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var e = element.InternalElement;
            switch (e)
            {
                case Autodesk.Revit.DB.FamilyInstance fi:
                    return fi.GetSubComponentIds().Select(x => doc.GetElement(x).ToDSType(true)).ToList();
                case Autodesk.Revit.DB.Architecture.Stairs s:
                    var stairComponents = s.GetStairsLandings().Select(x => doc.GetElement(x).ToDSType(true)).ToList();
                    stairComponents.AddRange(s.GetStairsRuns().Select(x => doc.GetElement(x).ToDSType(true)));
                    stairComponents.AddRange(s.GetStairsSupports().Select(x => doc.GetElement(x).ToDSType(true)));
                    return stairComponents;
                case Autodesk.Revit.DB.Architecture.Railing r:
                    var railComponents = r.GetHandRails().Select(x => doc.GetElement(x).ToDSType(true)).ToList();
                    railComponents.Add(doc.GetElement(r.TopRail).ToDSType(true));
                    return railComponents;
                case Autodesk.Revit.DB.BeamSystem b:
                    return b.GetBeamIds().Select(x => doc.GetElement(x).ToDSType(true)).ToList();
                default:
                    return new List<Element>();
            }
        }
    }
}
