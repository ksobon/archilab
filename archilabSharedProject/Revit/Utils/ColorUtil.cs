using Autodesk.DesignScript.Runtime;

namespace archilab.Revit.Utils
{
    /// <summary>
    /// 
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public static class ColorUtil
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string GetColor(Autodesk.Revit.DB.Color color)
        {
            return $"Color(Red = {color.Red}, Green = {color.Green}, Blue = {color.Blue}, Alpha = 255)";
        }
    }
}
