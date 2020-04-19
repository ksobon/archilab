using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace archilab.Revit.Utils
{
    public static class DoubleExtensions
    {
        public static bool AlmostEqualTo(this double value1, double value2)
        {
            return Math.Abs(value1 - value2) < 0.001;
        }
    }
}
