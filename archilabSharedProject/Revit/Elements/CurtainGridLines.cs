using System;
using Revit.Elements;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class CurtainGridLines
    {
        internal CurtainGridLines()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="curtainGridLine"></param>
        /// <returns></returns>
        public static References GetReference(Element curtainGridLine)
        {
            if (curtainGridLine == null)
                throw new Exception("Curtain Grid Line cannot be null.");
            if (!(curtainGridLine.InternalElement is Autodesk.Revit.DB.CurtainGridLine cgl))
                throw new Exception("Input curtainGridLine must be of type CurtainGridLine.");

            var opt = new Autodesk.Revit.DB.Options
            {
                ComputeReferences = true,
                IncludeNonVisibleObjects = true
            };

            var geomElement = cgl.get_Geometry(opt);
            foreach (var go in geomElement)
            {
                if (!(go is Autodesk.Revit.DB.Line l)) continue;

                return new References(l.Reference);
            }

            throw new Exception("Could not find a Line Reference.");
        }
    }
}
