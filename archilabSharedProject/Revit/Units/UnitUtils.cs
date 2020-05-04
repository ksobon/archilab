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
        /// <param name="displayUnitType"></param>
        /// <returns></returns>
        public static double ConvertFromInternalUnits(double value, string displayUnitType)
        {
            if (string.IsNullOrWhiteSpace(displayUnitType))
                throw new ArgumentNullException(nameof(displayUnitType));

            var dut = (Autodesk.Revit.DB.DisplayUnitType)Enum.Parse(typeof(Autodesk.Revit.DB.DisplayUnitType), displayUnitType);
            var result = Autodesk.Revit.DB.UnitUtils.ConvertFromInternalUnits(value, dut);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="units"></param>
        /// <param name="unitType"></param>
        /// <param name="value"></param>
        /// <param name="maxAccuracy"></param>
        /// <param name="forEditing"></param>
        /// <returns></returns>
        public static string Format(Units units, string unitType, double value, bool maxAccuracy = false, bool forEditing = false)
        {
            if (units == null)
                throw new ArgumentException(nameof(units));
            if (string.IsNullOrWhiteSpace(unitType))
                throw new ArgumentException(nameof(unitType));
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException(nameof(value));

            var ut = (Autodesk.Revit.DB.UnitType)Enum.Parse(typeof(Autodesk.Revit.DB.UnitType), unitType);
            return Autodesk.Revit.DB.UnitFormatUtils.Format(units.InternalUnits, ut, value, maxAccuracy, forEditing);
        }
    }
}
