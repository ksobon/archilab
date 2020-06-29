using System.Collections.Generic;
using System.Linq;
using Dynamo.Graph.Nodes;
using Revit.Elements;
using RevitServices.Persistence;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class CurtainGrids
    {
        private Autodesk.Revit.DB.CurtainGrid InternalElement { get; }

        internal CurtainGrids(Autodesk.Revit.DB.CurtainGrid cg)
        {
            InternalElement = cg;
        }

        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public List<Element> UGrids {
            get
            {
                return InternalElement.GetUGridLineIds()
                    .Select(x => DocumentManager.Instance.CurrentDBDocument.GetElement(x).ToDSType(true))
                    .ToList();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public List<Element> VGrids
        {
            get
            {
                return InternalElement.GetVGridLineIds()
                    .Select(x => DocumentManager.Instance.CurrentDBDocument.GetElement(x).ToDSType(true))
                    .ToList();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public int NumULines
        {
            get { return InternalElement.NumULines; }
        }

        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public int NumVLines
        {
            get { return InternalElement.NumVLines; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Curtain Grid";
        }
    }
}
