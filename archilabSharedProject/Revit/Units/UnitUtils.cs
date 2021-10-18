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
#if !Revit2018 && !Revit2019 && !Revit2020 && !Revit2021

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
#else
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
#endif
    }
}
