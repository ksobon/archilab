using Autodesk.DesignScript.Geometry;
using Revit.Elements;
using Revit.Elements.Views;
using System;
using System.Collections.Generic;
using NUnit.Framework;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Grid = Autodesk.Revit.DB.Grid;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class Grids
    {
        internal Grids()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        public static Element SetToViewCropBox(Element grid, View view)
        {
            if (grid == null || !(grid.InternalElement is Grid g))
                throw new ArgumentNullException(nameof(grid));
            if (view == null || !(view.InternalElement is Autodesk.Revit.DB.View v))
                throw new ArgumentNullException(nameof(view));

            var boundingLines = GetOutline(v.CropBox.ToProtoType());
            var dsCurve = g.Curve.ToProtoType();
            var midPt = dsCurve.PointAtParameter(0.5);
            var dir = ((Autodesk.Revit.DB.Line) g.Curve).Direction.ToVector(true);

            var resultList = new List<Autodesk.Revit.DB.XYZ>();
            foreach (var bl in boundingLines)
            {
                midPt = Point.ByCoordinates(midPt.X, midPt.Y, bl.StartPoint.Z); // flatten 
                var iResult1 = midPt.Project(bl, dir);
                var iResult2 = midPt.Project(bl, dir.Reverse());

                if (iResult1.Length > 0)
                    resultList.Add(((Point)iResult1[0]).ToXyz());
                if (iResult2.Length > 0)
                    resultList.Add(((Point)iResult2[0]).ToXyz());
            }

            Autodesk.Revit.DB.Line updatedLine = null;
            if (resultList.Count == 2)
            {
                updatedLine = Autodesk.Revit.DB.Line.CreateBound(resultList[0], resultList[1]);
            }
            else
            {
                throw new ArgumentNullException(nameof(grid));
            }

            //// (Konrad) If Grid Curve is a line, Revit wants it to be a Bound Curve. 
            //// Otherwise it will throw an exception.
            //var startPt = curve.StartPoint.ToRevitType();
            //var endPt = curve.EndPoint.ToRevitType();
            //var line = Autodesk.Revit.DB.Line.CreateBound(startPt, endPt);
            //var distance = startPt.DistanceTo(endPt);
            //var length = line.Length;

            //var c = (length - distance) <= 0.001 
            //    ? line
            //    : curve.ToRevitType();
            
            //var et = (Autodesk.Revit.DB.DatumExtentType)Enum.Parse(typeof(Autodesk.Revit.DB.DatumExtentType), extentType);
            var doc = DocumentManager.Instance.CurrentDBDocument;

            TransactionManager.Instance.EnsureInTransaction(doc);
            g.SetCurveInView(Autodesk.Revit.DB.DatumExtentType.ViewSpecific, v, updatedLine);
            TransactionManager.Instance.TransactionTaskDone();

            return grid;
        }

        private static List<Line> GetOutline(BoundingBox bb)
        {
            var lines = new List<Line>
            {
                Line.ByStartPointEndPoint(Point.ByCoordinates(bb.MinPoint.X, bb.MinPoint.Y, 0), Point.ByCoordinates(bb.MaxPoint.X, bb.MinPoint.Y, 0)),
                Line.ByStartPointEndPoint(Point.ByCoordinates(bb.MaxPoint.X, bb.MinPoint.Y, 0), Point.ByCoordinates(bb.MaxPoint.X, bb.MaxPoint.Y, 0)),
                Line.ByStartPointEndPoint(Point.ByCoordinates(bb.MaxPoint.X, bb.MaxPoint.Y, 0), Point.ByCoordinates(bb.MinPoint.X, bb.MaxPoint.Y, 0)),
                Line.ByStartPointEndPoint(Point.ByCoordinates(bb.MinPoint.X, bb.MaxPoint.Y, 0), Point.ByCoordinates(bb.MinPoint.X, bb.MinPoint.Y, 0))
            };

            return lines;
        }
    }
}
