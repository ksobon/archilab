using System;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Units
{
    /// <summary>
    /// 
    /// </summary>
    public class UnitUtils
    {
        internal UnitUtils()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="forgeUnit"></param>
        /// <returns></returns>
        public static double ConvertFromInternalUnits(double value, string forgeUnit)
        {
            if (string.IsNullOrWhiteSpace(forgeUnit))
                throw new ArgumentNullException(nameof(forgeUnit));

            var dut = new Autodesk.Revit.DB.ForgeTypeId(forgeUnit);
            var result = Autodesk.Revit.DB.UnitUtils.ConvertFromInternalUnits(value, dut);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="units"></param>
        /// <param name="forgeSpec"></param>
        /// <param name="value"></param>
        /// <param name="forEditing"></param>
        /// <returns></returns>
        public static string Format(Units units, string forgeSpec, double value, bool forEditing = false)
        {
            if (units == null)
                throw new ArgumentException(nameof(units));
            if (string.IsNullOrWhiteSpace(forgeSpec))
                throw new ArgumentException(nameof(forgeSpec));
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException(nameof(value));

            var ut = new Autodesk.Revit.DB.ForgeTypeId(forgeSpec);
            return Autodesk.Revit.DB.UnitFormatUtils.Format(units.InternalUnits, ut, value, forEditing);
        }
    }
}
