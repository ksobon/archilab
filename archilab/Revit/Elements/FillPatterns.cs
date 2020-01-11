using System;
using Autodesk.Revit.DB;
using Revit.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Element = Revit.Elements.Element;

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
        /// <param name="fillPattern"></param>
        /// <returns></returns>
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
        public static string Orientation(Element fillPattern)
        {
            if (fillPattern == null)
                throw new ArgumentException(nameof(fillPattern));

            if (!(fillPattern.InternalElement is Autodesk.Revit.DB.FillPatternElement fpe))
                throw new ArgumentException(nameof(fillPattern));

            var fp = fpe.GetFillPattern();
            switch (fp.HostOrientation)
            {
                case FillPatternHostOrientation.ToView:
                    return "Orient To View";
                case FillPatternHostOrientation.AsText:
                    return "Keep Readable";
                case FillPatternHostOrientation.ToHost:
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fillPattern"></param>
        /// <returns></returns>
        public static double LineSpacing1(Element fillPattern)
        {
            if (fillPattern == null)
                throw new ArgumentException(nameof(fillPattern));

            if (!(fillPattern.InternalElement is Autodesk.Revit.DB.FillPatternElement fpe))
                throw new ArgumentException(nameof(fillPattern));

            var fp = fpe.GetFillPattern();
            var grids = fp.GetFillGrids();

            return UnitUtils.ConvertFromInternalUnits(grids[0].Offset, DisplayUnitType.DUT_MILLIMETERS);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fillPattern"></param>
        /// <returns></returns>
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

            return UnitUtils.ConvertFromInternalUnits(grids[1].Offset, DisplayUnitType.DUT_MILLIMETERS);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fillPattern"></param>
        /// <returns></returns>
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
        /// <param name="fillPattern"></param>
        /// <param name="name"></param>
        /// <returns></returns>
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
