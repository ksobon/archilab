using System;
using Revit.Elements;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class WallTypes
    {
        internal WallTypes()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wallType"></param>
        /// <returns></returns>
        public static string Kind(Element wallType)
        {
            if (wallType == null)
                throw new ArgumentNullException(nameof(wallType));

            return (wallType.InternalElement as Autodesk.Revit.DB.WallType)?.Kind.ToString();
        }
    }
}
