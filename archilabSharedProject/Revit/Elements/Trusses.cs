using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Revit.GeometryConversion;
using Curve = Autodesk.DesignScript.Geometry.Curve;
using Element = Revit.Elements.Element;
using Plane = Autodesk.DesignScript.Geometry.Plane;

// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class Trusses
    {
        internal Trusses()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="truss"></param>
        /// <returns></returns>
        public static List<Curve> Curves(Element truss)
        {
            if (truss == null)
                throw new ArgumentException(nameof(truss));
            if (!(truss.InternalElement is Autodesk.Revit.DB.Structure.Truss rTruss))
                throw new ArgumentException("Provided element is not a Truss.");

            return rTruss.Curves
                .Cast<Autodesk.Revit.DB.Curve>()
                .Select(x => x.ToProtoType())
                .ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="truss"></param>
        /// <returns></returns>
        public static BoundingBox BoundingBox(Element truss)
        {
            if (truss == null)
                throw new ArgumentException(nameof(truss));
            if (!(truss.InternalElement is Autodesk.Revit.DB.Structure.Truss rTruss))
                throw new ArgumentException("Provided element is not a Truss.");

            return rTruss.get_BoundingBox(null).ToProtoType();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="truss"></param>
        /// <returns></returns>
        public static Plane Plane(Element truss)
        {
            if (truss == null)
                throw new ArgumentException(nameof(truss));
            if (!(truss.InternalElement is Autodesk.Revit.DB.Structure.Truss rTruss))
                throw new ArgumentException("Provided element is not a Truss.");

            var curve = rTruss.Curves.Cast<Autodesk.Revit.DB.Curve>().OrderByDescending(x => x.Length).First();
            var startPoint = curve.Evaluate(0, true);
            var dir = (curve.GetEndPoint(1) - curve.GetEndPoint(0)).Normalize();
            var normal = new Autodesk.Revit.DB.XYZ(0, 0, 1);
            var cross = dir.CrossProduct(normal);
            
            return Autodesk.Revit.DB.Plane.CreateByNormalAndOrigin(cross, startPoint).ToPlane();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="truss"></param>
        /// <returns></returns>
        public static List<Curve> FlatCurves(Element truss)
        {
            if (truss == null)
                throw new ArgumentException(nameof(truss));
            if (!(truss.InternalElement is Autodesk.Revit.DB.Structure.Truss rTruss))
                throw new ArgumentException("Provided element is not a Truss.");

            var finalCurves = new List<Curve>();

            // (Konrad) Get a plane that the Truss is in.
            var curves = rTruss.Curves;
            var longest = curves.Cast<Autodesk.Revit.DB.Curve>().OrderByDescending(x => x.Length).First();
            var startPoint = longest.Evaluate(0, true);
            var dir = (longest.GetEndPoint(1) - longest.GetEndPoint(0)).Normalize();
            var basisZ = Autodesk.Revit.DB.XYZ.BasisZ;
            var normal = dir.CrossProduct(basisZ);
            var plane = Autodesk.Revit.DB.Plane.CreateByNormalAndOrigin(normal, startPoint);

            double minU = 999999;
            double maxU = -999999;

            // (Konrad) Project points onto the plane, and flatten it via UV coordinates.
            var flatLines = new List<Autodesk.Revit.DB.Line>();
            foreach (Autodesk.Revit.DB.Curve curve in curves)
            {
                var start = curve.GetEndPoint(0);
                var end = curve.GetEndPoint(1);
                plane.Project(start, out var startUv, out var unused);
                plane.Project(end, out var endUv, out var unused1);

                var potentialMinU = Math.Min(Math.Abs(startUv.U), Math.Abs(endUv.U));
                if (potentialMinU < minU)
                    minU = potentialMinU;

                var potentialMaxU = Math.Max(Math.Abs(startUv.U), Math.Abs(endUv.U));
                if (potentialMaxU > maxU)
                    maxU = potentialMaxU;

                var newStart = new Autodesk.Revit.DB.XYZ(Math.Abs(startUv.U), Math.Abs(startUv.V), 0);
                var newEnd = new Autodesk.Revit.DB.XYZ(Math.Abs(endUv.U), Math.Abs(endUv.V), 0);
                var line = Autodesk.Revit.DB.Line.CreateBound(newStart, newEnd);
                flatLines.Add(line);
            }

            // (Konrad) If truss is in YX plane, flip it to XY. All coordinates should be in positive
            // XY direction with long axis of the truss along the X axis.
            var flatLongest = flatLines.OrderByDescending(x => x.Length).First();
            var flatDirection = (flatLongest.GetEndPoint(1) - flatLongest.GetEndPoint(0)).Normalize();
            var height = maxU - minU;
            if (Math.Abs(flatDirection.Y - 1) < 0.001)
            {
                var rotation = Autodesk.Revit.DB.Transform.CreateRotationAtPoint(Autodesk.Revit.DB.XYZ.BasisZ,
                    90.0.ToRadians(), Autodesk.Revit.DB.XYZ.Zero);
                var rotation2 = Autodesk.Revit.DB.Transform.CreateRotationAtPoint(Autodesk.Revit.DB.XYZ.BasisX,
                    180.0.ToRadians(), Autodesk.Revit.DB.XYZ.Zero);
                var move = Autodesk.Revit.DB.Transform.CreateTranslation(new Autodesk.Revit.DB.XYZ(0, height, 0));
                foreach (var flatLine in flatLines)
                {
                    var nL = flatLine.CreateTransformed(rotation);
                    var nL2 = nL.CreateTransformed(rotation2);
                    var nl3 = nL2.CreateTransformed(move);
                    //var s = nL2.GetEndPoint(0);
                    //var e = nL2.GetEndPoint(1);
                    //var nS = new Autodesk.Revit.DB.XYZ(s.X, Math.Abs(s.Y), s.Z);
                    //var nE = new Autodesk.Revit.DB.XYZ(e.X, Math.Abs(e.Y), e.Z);
                    //var finalLine = Autodesk.Revit.DB.Line.CreateBound(nS, nE);
                    finalCurves.Add(nl3.ToProtoType());

                }
            }
            else
            {
                foreach (var flatLine in flatLines)
                {
                    finalCurves.Add(flatLine.ToProtoType());
                }
            }

            return finalCurves;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="truss"></param>
        /// <param name="maxY"></param>
        /// <returns></returns>
        public static List<Curve> FlatCurves(Element truss, double maxY = 200)
        {
            if (truss == null)
                throw new ArgumentException(nameof(truss));
            if (!(truss.InternalElement is Autodesk.Revit.DB.Structure.Truss rTruss))
                throw new ArgumentException("Provided element is not a Truss.");

            var curves = rTruss.Curves;
            var longest = curves.Cast<Autodesk.Revit.DB.Curve>().OrderByDescending(x => x.Length).First();
            var midPoint = longest.Evaluate(0, true);
            var dir = (longest.GetEndPoint(1) - longest.GetEndPoint(0)).Normalize();
            var normal = new Autodesk.Revit.DB.XYZ(0, 0, 1);
            var cross = dir.CrossProduct(normal);
            var plane = Autodesk.Revit.DB.Plane.CreateByNormalAndOrigin(cross, midPoint);

            double minU = 999999;
            double maxU = -999999;
            double minV = 999999;
            double maxV = -999999;
            var flatLines = new List<Autodesk.Revit.DB.Line>();
            foreach (Autodesk.Revit.DB.Curve curve in curves)
            {
                var start = curve.GetEndPoint(0);
                var end = curve.GetEndPoint(1);
                plane.Project(start, out var startUv, out var unused);
                plane.Project(end, out var endUv, out var unused1);

                var potentialMinU = Math.Min(startUv.U, endUv.U);
                if (potentialMinU < minU)
                    minU = potentialMinU;

                var potentialMaxU = Math.Max(startUv.U, endUv.U);
                if (potentialMaxU > maxU)
                    maxU = potentialMaxU;

                var potentialMinV = Math.Min(startUv.V, endUv.V);
                if (potentialMinV < minV)
                    minV = potentialMinV;

                var potentialMaxV = Math.Max(startUv.V, endUv.V);
                if (potentialMaxV > maxV)
                    maxV = potentialMaxV;

                var newStart = new Autodesk.Revit.DB.XYZ(startUv.U, startUv.V, 0);
                var newEnd = new Autodesk.Revit.DB.XYZ(endUv.U, endUv.V, 0);
                var line = Autodesk.Revit.DB.Line.CreateBound(newStart, newEnd);

                flatLines.Add(line);
            }

            var scaledLines = new List<Curve>();
            foreach (var line in flatLines)
            {
                var scaledLine = Scale(line, minU, maxU, minV, maxV, maxY).ToProtoType();
                scaledLines.Add(scaledLine);
            }

            return scaledLines;
        }

        private static Autodesk.Revit.DB.Line Scale(Autodesk.Revit.DB.Curve line, double minU, double maxU, double minV, double maxV, double maxHeight)
        {
            var start = line.GetEndPoint(0);
            var end = line.GetEndPoint(1);

            var width = maxU - minU;
            var height = maxV - minV;
            var scaleFactor = maxHeight / height;
            var maxWidth = width * scaleFactor;

            var oldRangeU = maxU - minU;
            var oldRangeV = maxV - minV;
            var startU = (start.X - minU) * maxWidth / oldRangeU + 0;
            var startV = (start.Y - minV) * maxHeight / oldRangeV + 0;
            var endU = (end.X - minU) * maxWidth / oldRangeU + 0;
            var endV = (end.Y - minV) * maxHeight / oldRangeV + 0;
            var newStart = new Autodesk.Revit.DB.XYZ(startU, startV, 0);
            var newEnd = new Autodesk.Revit.DB.XYZ(endU, endV, 0);
            return Autodesk.Revit.DB.Line.CreateBound(newStart, newEnd);
        }
    }
}
