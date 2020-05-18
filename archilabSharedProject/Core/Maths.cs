using System;

namespace archilab.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class Maths
    {
        internal Maths()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="number"></param>
        /// <param name="roundingType"></param>
        /// <returns></returns>
        public static double Round(double number, string roundingType = "ToEven")
        {
            var type = (MidpointRounding)Enum.Parse(typeof(MidpointRounding), roundingType);

            return Math.Round(number, type);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="number"></param>
        /// <param name="decimals"></param>
        /// <param name="roundingType"></param>
        /// <returns></returns>
        public static double Round(double number, int decimals, string roundingType = "ToEven")
        {
            var type = (MidpointRounding)Enum.Parse(typeof(MidpointRounding), roundingType);

            return Math.Round(number, decimals, type);
        }
    }
}
