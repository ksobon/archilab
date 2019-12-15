using System;
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
    }
}
