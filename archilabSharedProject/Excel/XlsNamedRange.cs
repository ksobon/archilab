using Autodesk.DesignScript.Runtime;
using ClosedXML.Excel;
// ReSharper disable UnusedMember.Global

namespace archilab.Excel
{
    /// <summary>
    /// Named Range Wrapper
    /// </summary>
    public class NamedRange
    {
        #region Internal Methods

        internal NamedRange()
        {
        }

        internal IXLNamedRange InternalNamedRange { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="range"></param>
        [SupressImportIntoVM]
        public NamedRange(IXLNamedRange range)
        {
            InternalNamedRange = range;
        }

        #endregion

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public string Name
        {
            get { return InternalNamedRange.Name; }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"NamedRange: Name={(InternalNamedRange != null ? Name : string.Empty)}";
        }

        #endregion
    }
}
