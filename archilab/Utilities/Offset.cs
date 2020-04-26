// ReSharper disable UnusedMember.Global

namespace archilab.Utilities
{
    /// <summary>
    /// Utility class for creating offsets.
    /// </summary>
    public class Offset
    {
        internal Offset()
        {
        }

        /// <summary>
        /// Top offset.
        /// </summary>
        public double Top { get; set; }

        /// <summary>
        /// Right offset.
        /// </summary>
        public double Right { get; set; }

        /// <summary>
        /// Bottom offset.
        /// </summary>
        public double Bottom { get; set; }

        /// <summary>
        /// Left offset.
        /// </summary>
        public double Left { get; set; }

        /// <summary>
        /// Creates offset object that has default values set to 0 for all four sides.
        /// </summary>
        /// <param name="top">Top offset.</param>
        /// <param name="right">Right offset.</param>
        /// <param name="bottom">Bottom offset.</param>
        /// <param name="left">Left offset.</param>
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
