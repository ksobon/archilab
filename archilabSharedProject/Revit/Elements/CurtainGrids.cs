using System.Collections.Generic;
using System.Linq;
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
        public int NumULines
        {
            get { return InternalElement.NumULines; }
        }
        
        /// <summary>
        /// 
        /// </summary>
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
