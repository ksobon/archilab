using System;
using System.Linq;
using Dynamo.Graph.Nodes;
using Revit.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class FillPatterns
    {
        internal FillPatterns()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static Element GetByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            var fp = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                .OfClass(typeof(Autodesk.Revit.DB.FillPatternElement))
                .WhereElementIsNotElementType()
                .Where(x => x.Name == name)
                .ToList();

            if (fp.Any())
                return fp.First().ToDSType(true);

            throw new Exception("Could not find Fill Pattern with given name.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fillPattern"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static Element SetName(Element fillPattern, string name)
        {
            if (fillPattern == null)
                throw new ArgumentException(nameof(fillPattern));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            if (!(fillPattern.InternalElement is Autodesk.Revit.DB.FillPatternElement fpe))
                throw new ArgumentException(nameof(fillPattern));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            TransactionManager.Instance.EnsureInTransaction(doc);
            fpe.Name = name;
            TransactionManager.Instance.TransactionTaskDone();

            return fpe.ToDSType(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fillPattern"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static string Target(Element fillPattern)
        {
            if (fillPattern == null)
                throw new ArgumentException(nameof(fillPattern));

            if (!(fillPattern.InternalElement is Autodesk.Revit.DB.FillPatternElement fpe))
                throw new ArgumentException(nameof(fillPattern));

            return fpe.GetFillPattern().Target.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fillPattern"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static string Orientation(Element fillPattern)
        {
            if (fillPattern == null)
                throw new ArgumentException(nameof(fillPattern));

            if (!(fillPattern.InternalElement is Autodesk.Revit.DB.FillPatternElement fpe))
                throw new ArgumentException(nameof(fillPattern));

            var fp = fpe.GetFillPattern();
            switch (fp.HostOrientation)
            {
                case Autodesk.Revit.DB.FillPatternHostOrientation.ToView:
                    return "Orient To View";
                case Autodesk.Revit.DB.FillPatternHostOrientation.AsText:
                    return "Keep Readable";
                case Autodesk.Revit.DB.FillPatternHostOrientation.ToHost:
                    return "Align with element";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fillPattern"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static string Name(Element fillPattern)
        {
            if (fillPattern == null)
                throw new ArgumentException(nameof(fillPattern));

            if (!(fillPattern.InternalElement is Autodesk.Revit.DB.FillPatternElement fpe))
                throw new ArgumentException(nameof(fillPattern));

            return fpe.GetFillPattern().Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fillPattern"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static bool IsCrosshatch(Element fillPattern)
        {
            if (fillPattern == null)
                throw new ArgumentException(nameof(fillPattern));

            if (!(fillPattern.InternalElement is Autodesk.Revit.DB.FillPatternElement fpe))
                throw new ArgumentException(nameof(fillPattern));

            return fpe.GetFillPattern().GridCount == 2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fillPattern"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static double Angle(Element fillPattern)
        {
            if (fillPattern == null)
                throw new ArgumentException(nameof(fillPattern));

            if (!(fillPattern.InternalElement is Autodesk.Revit.DB.FillPatternElement fpe))
                throw new ArgumentException(nameof(fillPattern));

            var fp = fpe.GetFillPattern();
            var grids = fp.GetFillGrids();

            return RadiansToDegrees(grids[0].Angle);
        }
#if !Revit2018 && !Revit2019 && !Revit2020 && !Revit2021
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fillPattern"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static double LineSpacing1(Element fillPattern)
        {
            if (fillPattern == null)
                throw new ArgumentException(nameof(fillPattern));

            if (!(fillPattern.InternalElement is Autodesk.Revit.DB.FillPatternElement fpe))
                throw new ArgumentException(nameof(fillPattern));

            var fp = fpe.GetFillPattern();
            var grids = fp.GetFillGrids();
            var fu = new Autodesk.Revit.DB.ForgeTypeId("autodesk.unit.unit:millimeters-1.0.1");

            return Autodesk.Revit.DB.UnitUtils.ConvertFromInternalUnits(grids[0].Offset, fu);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fillPattern"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static double LineSpacing2(Element fillPattern)
        {
            if (fillPattern == null)
                throw new ArgumentException(nameof(fillPattern));

            if (!(fillPattern.InternalElement is Autodesk.Revit.DB.FillPatternElement fpe))
                throw new ArgumentException(nameof(fillPattern));

            var fp = fpe.GetFillPattern();
            var count = fp.GridCount;
            if (count < 2)
                throw new Exception("Fill Pattern doesn't have Line spacing 2 specified.");

            var grids = fp.GetFillGrids();
            var fu = new Autodesk.Revit.DB.ForgeTypeId("autodesk.unit.unit:millimeters-1.0.1");

            return Autodesk.Revit.DB.UnitUtils.ConvertFromInternalUnits(grids[1].Offset, fu);
        }
#else
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fillPattern"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static double LineSpacing1(Element fillPattern)
        {
            if (fillPattern == null)
                throw new ArgumentException(nameof(fillPattern));

            if (!(fillPattern.InternalElement is Autodesk.Revit.DB.FillPatternElement fpe))
                throw new ArgumentException(nameof(fillPattern));

            var fp = fpe.GetFillPattern();
            var grids = fp.GetFillGrids();

            return Autodesk.Revit.DB.UnitUtils.ConvertFromInternalUnits(grids[0].Offset, Autodesk.Revit.DB.DisplayUnitType.DUT_MILLIMETERS);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fillPattern"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static double LineSpacing2(Element fillPattern)
        {
            if (fillPattern == null)
                throw new ArgumentException(nameof(fillPattern));

            if (!(fillPattern.InternalElement is Autodesk.Revit.DB.FillPatternElement fpe))
                throw new ArgumentException(nameof(fillPattern));

            var fp = fpe.GetFillPattern();
            var count = fp.GridCount;
            if (count < 2)
                throw new Exception("Fill Pattern doesn't have Line spacing 2 specified.");

            var grids = fp.GetFillGrids();

            return Autodesk.Revit.DB.UnitUtils.ConvertFromInternalUnits(grids[1].Offset, Autodesk.Revit.DB.DisplayUnitType.DUT_MILLIMETERS);
        }
#endif
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fillPattern"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static bool IsSolidFill(Element fillPattern)
        {
            if (fillPattern == null)
                throw new ArgumentException(nameof(fillPattern));

            if (!(fillPattern.InternalElement is Autodesk.Revit.DB.FillPatternElement fpe))
                throw new ArgumentException(nameof(fillPattern));

            return fpe.GetFillPattern().IsSolidFill;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static bool Exists(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            var fp = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                .OfClass(typeof(Autodesk.Revit.DB.FillPatternElement))
                .WhereElementIsNotElementType()
                .Where(x => x.Name == name)
                .ToList();

            return fp.Any();
        }

#region Utilities

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radians"></param>
        /// <returns></returns>
        private static double RadiansToDegrees(double radians)
        {
            return (radians * 180) / Math.PI;
        }

#endregion
    }
}
