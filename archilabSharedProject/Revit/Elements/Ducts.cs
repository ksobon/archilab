using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Revit.Elements;
using Revit.Elements.InternalUtilities;
using Revit.GeometryConversion;
using RevitServices.Transactions;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class Ducts
    {
        internal Ducts()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="duct"></param>
        /// <param name="point"></param>
        /// <param name="damperType"></param>
        /// <param name="damperWidth"></param>
        /// <param name="damperHeight"></param>
        public static void InsertDamper(
            Element duct, 
            Point point, 
            FamilyType damperType, 
            string damperWidth = "Duct Width", 
            string damperHeight = "Duct Height")
        {
            if (!(duct.InternalElement is Autodesk.Revit.DB.Mechanical.Duct duct1))
                throw new ArgumentNullException(nameof(duct));
            if (point == null)
                throw new ArgumentNullException(nameof(point));
            if (damperType == null)
                throw new ArgumentNullException(nameof(damperType));

            if (!(duct1.Location is Autodesk.Revit.DB.LocationCurve location))
                throw new ArgumentException("Duct is not curve based.");

            var doc = duct1.Document;
            var xyz = point.ToRevitType();
            var curve = location.Curve;
            var result = curve.Project(xyz);
            var pt = result.XYZPoint;
            var symbol = damperType.InternalElement as Autodesk.Revit.DB.FamilySymbol;

            TransactionManager.Instance.EnsureInTransaction(doc);

            // (Konrad) Create new Damper instance and split duct into two.
            var id = Autodesk.Revit.DB.Mechanical.MechanicalUtils.BreakCurve(doc, duct1.Id, pt);
            if (!(doc.GetElement(id) is Autodesk.Revit.DB.Mechanical.Duct duct2))
                throw new ArgumentException("Failed to split Duct.");

            var fi = doc.Create.NewFamilyInstance(pt, symbol, duct1, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
            doc.Regenerate();

            // (Konrad) Rotate Damper.
            var start = curve.GetEndPoint(0).ToPoint();
            var end = curve.GetEndPoint(1).ToPoint();
            var direction = Vector.ByTwoPoints(start, end);
            var up = Vector.ZAxis();
            var perpendicular = direction.Cross(up);
            var cs = CoordinateSystem.ByOriginVectors(pt.ToPoint(), direction, perpendicular);
            var transform = cs.ToTransform();
            TransformUtils.ExtractEularAnglesFromTransform(transform, out var newRotationAngles);
            var rotation = ConvertEularToAngleDegrees(newRotationAngles.FirstOrDefault());
            var oldTransform = fi.GetTransform();
            TransformUtils.ExtractEularAnglesFromTransform(oldTransform, out var oldRotationAngles);
            var newRotationAngle = rotation * Math.PI / 180;
            var rotateAngle = newRotationAngle - oldRotationAngles.FirstOrDefault();
            var axis = Autodesk.Revit.DB.Line.CreateUnbound(oldTransform.Origin, oldTransform.BasisZ);
            Autodesk.Revit.DB.ElementTransformUtils.RotateElement(doc, fi.Id, axis, -rotateAngle);

            // (Konrad) Set Damper Width/Height to match Duct.
            var sourceWidth = duct1.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.RBS_CURVE_WIDTH_PARAM).AsDouble();
            var sourceHeight = duct1.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.RBS_CURVE_HEIGHT_PARAM).AsDouble();
            fi.GetParameters(damperWidth).FirstOrDefault()?.Set(sourceWidth);
            fi.GetParameters(damperHeight).FirstOrDefault()?.Set(sourceHeight);

            // (Konrad) Connect Damper to Ducts.
            var c1 = FindClosest(duct1.ConnectorManager.Connectors, xyz); // duct1 endpoint
            var c1Other = FindOther(duct1.ConnectorManager.Connectors, c1);
            var c1A = FindClosest(fi.MEPModel.ConnectorManager.Connectors, c1Other);
            c1.ConnectTo(c1A);

            var c2 = FindClosest(duct2.ConnectorManager.Connectors, xyz); // duct2 endpoint
            var c2Other = FindOther(duct2.ConnectorManager.Connectors, c2);
            var c2A = FindClosest(fi.MEPModel.ConnectorManager.Connectors, c2Other);
            c2.ConnectTo(c2A);

            TransactionManager.Instance.TransactionTaskDone();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="duct"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Element Split(Element duct, Point point)
        {
            if (!(duct.InternalElement is Autodesk.Revit.DB.Mechanical.Duct duct1))
                throw new ArgumentNullException(nameof(duct));

            if (!(duct1.Location is Autodesk.Revit.DB.LocationCurve location))
                throw new ArgumentException("Duct is not curve based.");

            var doc = duct1.Document;
            var xyz = point.ToRevitType();
            var curve = location.Curve;
            var result = curve.Project(xyz);
            var pt = result.XYZPoint;

            TransactionManager.Instance.EnsureInTransaction(doc);
            var id = Autodesk.Revit.DB.Mechanical.MechanicalUtils.BreakCurve(doc, duct1.Id, pt);
            TransactionManager.Instance.TransactionTaskDone();

            return doc.GetElement(id).ToDSType(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="duct"></param>
        /// <returns></returns>
        public static Curve Location(Element duct)
        {
            if (!(duct.InternalElement is Autodesk.Revit.DB.Mechanical.Duct duct1))
                throw new ArgumentNullException(nameof(duct));

            if (!(duct1.Location is Autodesk.Revit.DB.LocationCurve location))
                throw new ArgumentException("Duct is not curve based.");

            return location.Curve.ToProtoType();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static List<Connectors> Connectors(Element element)
        {
            if (!(element.InternalElement is Autodesk.Revit.DB.Mechanical.Duct duct))
                throw new ArgumentNullException(nameof(element));

            return (from Autodesk.Revit.DB.Connector conn in duct.ConnectorManager.Connectors
                select new Connectors(conn)).ToList();
        }

        #region Utilities

        private static double ConvertEularToAngleDegrees(double newRotationAngles)
        {
            return (newRotationAngles / (2 * Math.PI)) * 360;
        }

        private static Autodesk.Revit.DB.XYZ FindOther(IEnumerable connectors, Autodesk.Revit.DB.Connector closest)
        {
            foreach (Autodesk.Revit.DB.Connector conn in connectors)
            {
                if (closest.Id == conn.Id) continue;

                return conn.Origin;
            }

            return null;
        }

        private static Autodesk.Revit.DB.Connector FindClosest(IEnumerable connectors, Autodesk.Revit.DB.XYZ pt)
        {
            Autodesk.Revit.DB.Connector c = null;
            var closest = 999999.0;
            foreach (Autodesk.Revit.DB.Connector conn in connectors)
            {
                var distance = conn.Origin.DistanceTo(pt);
                if (!(distance <= closest))
                    continue;

                c = conn;
                closest = distance;
            }

            return c;
        }

        #endregion
    }
}
