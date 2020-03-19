// ReSharper disable UnusedMember.Global

namespace archilab.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public class Offset
    {
        internal Offset()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public double Top { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double Right { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double Bottom { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double Left { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="top"></param>
        /// <param name="right"></param>
        /// <param name="bottom"></param>
        /// <param name="left"></param>
        public static Offset Create(double top = 0, double right = 0, double bottom = 0, double left = 0)
        {
            var offset = new Offset
            {
                Top = top,
                Right = right,
                Bottom = bottom,
                Left = left
            };

            return offset;
        }
    }
}
