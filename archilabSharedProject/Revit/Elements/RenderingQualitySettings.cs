using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class RenderingQualitySettings
    {
        internal RenderingQualitySettings()
        {
        }

        internal Autodesk.Revit.DB.RenderingQualitySettings InternalRenderingQualitySettings { get; set; }

        internal RenderingQualitySettings(Autodesk.Revit.DB.RenderingQualitySettings settings)
        {
            InternalRenderingQualitySettings = settings;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [NodeCategory("Query")]
        [MultiReturn("LightAndMaterialAccuracyMode", "RenderDuration", "RenderingQuality", "RenderLevel", "RenderTime")]
        public Dictionary<string, object> Settings()
        {
            return new Dictionary<string, object>
            {
                {"LightAndMaterialAccuracyMode", InternalRenderingQualitySettings.LightAndMaterialAccuracyMode.ToString()},
                {"RenderDuration", InternalRenderingQualitySettings.RenderDuration.ToString()},
                {"RenderingQuality", InternalRenderingQualitySettings.RenderingQuality.ToString()},
                {"RenderLevel", InternalRenderingQualitySettings.RenderLevel.ToString()},
                {"RenderTime", InternalRenderingQualitySettings.RenderTime},
            };
        }
    }
}
