using Autodesk.DesignScript.Geometry;
using Dynamo.Graph.Nodes;
using Revit.Elements;
using RevitServices.Persistence;
using System;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Geometry
{
    /// <summary>
    /// 
    /// </summary>
    public class Surfaces
    {
        internal Surfaces()
        { 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="surface"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static Element GetWall(Surface surface)
        {
            if (surface == null)
                throw new ArgumentNullException(nameof(surface));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            var ref1 = surface.Tags.LookupTag("RevitFaceReference") as Autodesk.Revit.DB.Reference;
            
            return doc.GetElement(ref1?.ElementId).ToDSType(true);
        }
    }
}
