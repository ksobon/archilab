using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;

namespace archilab.Revit.Utils
{
    /// <summary>
    /// Extension methods for ElementId to support both IntegerValue (pre-2026) and Value (2026+).
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public static class ElementIdExtensions
    {
        /// <summary>
        /// Returns the integer value of the ElementId. Uses Value in Revit 2026+, IntegerValue otherwise.
        /// </summary>
        public static int GetIdValue(this ElementId id)
        {
#if REVIT2026_OR_GREATER
            return (int)id.Value;
#else
            return id.IntegerValue;
#endif
        }
    }
}
