using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class RenderingImageExposureSettings
    {
        internal RenderingImageExposureSettings()
        {
        }

        internal Autodesk.Revit.DB.RenderingImageExposureSettings InternalRenderingImageExposureSettings { get; set; }

        internal RenderingImageExposureSettings(Autodesk.Revit.DB.RenderingImageExposureSettings settings)
        {
            InternalRenderingImageExposureSettings = settings;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [NodeCategory("Query")]
        [MultiReturn("BackgroundStyle", "LightingSource", "PrinterResolution", "ResolutionTarget", "ResolutionValue")]
        public Dictionary<string, object> Settings()
        {
            return new Dictionary<string, object>
            {
                {"ExposureValue", InternalRenderingImageExposureSettings.ExposureValue},
                {"Highlights", InternalRenderingImageExposureSettings.Highlights},
                {"Saturation", InternalRenderingImageExposureSettings.Saturation},
                {"Shadows", InternalRenderingImageExposureSettings.Shadows},
                {"WhitePoint", InternalRenderingImageExposureSettings.WhitePoint}
            };
        }
    }
}
