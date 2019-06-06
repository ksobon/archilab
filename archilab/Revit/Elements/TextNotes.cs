using System;
using Revit.Elements;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class TextNotes
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="textNote"></param>
        /// <returns></returns>
        public static double Width(Element textNote)
        {
            if (textNote == null)
                throw new ArgumentNullException(nameof(textNote));
            if (!(textNote.InternalElement is Autodesk.Revit.DB.TextNote tn))
                throw new Exception("Element is not a TextNote.");

            return tn.Width;
        }
    }
}
