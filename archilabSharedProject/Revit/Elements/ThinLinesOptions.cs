// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class ThinLinesOptions
    {
        internal ThinLinesOptions()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="show"></param>
        public static bool ShowThinLines(bool show = true)
        {
            Autodesk.Revit.UI.ThinLinesOptions.AreThinLinesEnabled = show;

            return show;
        }
    }
}
