using Dynamo.Graph.Nodes;
using RevitServices.Persistence;

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class Applications
    {
        internal Applications()
        { 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static int VersionNumber()
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            return int.Parse(doc.Application.VersionNumber);
        }
    }
}
