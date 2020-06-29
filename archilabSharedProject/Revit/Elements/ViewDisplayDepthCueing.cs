using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class ViewDisplayDepthCueing
    {
        internal ViewDisplayDepthCueing()
        {
        }

        internal Autodesk.Revit.DB.ViewDisplayDepthCueing InternalViewDisplayDepthCueing { get; set; }

        internal ViewDisplayDepthCueing(Autodesk.Revit.DB.ViewDisplayDepthCueing dc)
        {
            InternalViewDisplayDepthCueing = dc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [NodeCategory("Query")]
        [MultiReturn("Enabled", "StartPercentage", "EndPercentage", "FadeTo")] 
        public Dictionary<string, object> Properties()
        {
            return new Dictionary<string, object>
            {
                {"Enabled", InternalViewDisplayDepthCueing.EnableDepthCueing},
                {"StartPercentage", InternalViewDisplayDepthCueing.StartPercentage},
                {"EndPercentage", InternalViewDisplayDepthCueing.EndPercentage},
                {"FadeTo", InternalViewDisplayDepthCueing.FadeTo}
            };
        }
    }
}
