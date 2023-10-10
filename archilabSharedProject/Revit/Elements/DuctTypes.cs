using System;
using Dynamo.Graph.Nodes;
using Revit.Elements;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
#if !Revit2017 && !Revit2018
    /// <summary>
    /// 
    /// </summary>
    public class DuctTypes
    {
        internal DuctTypes()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ductType"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static string Shape(Element ductType)
        {
            if (ductType == null)
                throw new ArgumentNullException(nameof(ductType));

            if (!(ductType.InternalElement is Autodesk.Revit.DB.Mechanical.DuctType e))
                throw new ArgumentException("Provided element is not a DuctType.");

            return e.Shape.ToString();
        }
    }
#endif
}
