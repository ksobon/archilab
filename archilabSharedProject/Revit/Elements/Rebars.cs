using System;
using Revit.Elements;
using RevitServices.Persistence;
// ReSharper disable UnusedMember.Global
// ReSharper disable EmptyConstructor

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class Rebars
    {
        internal Rebars()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rebar"></param>
        /// <returns></returns>
        public static Element Host(Element rebar)
        {
            if (rebar == null)
                throw new ArgumentNullException(nameof(rebar));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            switch (rebar.InternalElement)
            {
                case Autodesk.Revit.DB.Structure.Rebar r:
                    return doc.GetElement(r.GetHostId()).ToDSType(true);
                case Autodesk.Revit.DB.Structure.RebarInSystem rs:
                    return doc.GetElement(rs.GetHostId()).ToDSType(true);
                default:
                    return null;
            }
        }
    }
}
