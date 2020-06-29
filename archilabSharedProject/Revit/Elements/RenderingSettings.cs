using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class RenderingSettings
    {
        internal RenderingSettings()
        {
        }

        internal Autodesk.Revit.DB.RenderingSettings InternalRenderingSettings { get; set; }

        internal RenderingSettings(Autodesk.Revit.DB.RenderingSettings settings)
        {
            InternalRenderingSettings = settings;
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
                {"BackgroundStyle", InternalRenderingSettings.BackgroundStyle.ToString()},
                {"LightingSource", InternalRenderingSettings.LightingSource.ToString()},
                {"PrinterResolution", InternalRenderingSettings.PrinterResolution.ToString()},
                {"ResolutionTarget", InternalRenderingSettings.ResolutionTarget.ToString()},
                {"ResolutionValue", InternalRenderingSettings.ResolutionValue}
            };
        }

        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public bool UsesRegionRendering
        {
            get { return InternalRenderingSettings.UsesRegionRendering; }
        }

        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public Outline RegionOutline
        {
            get { return new Outline(InternalRenderingSettings.GetRenderingRegionOutline()); }
        }

        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public RenderingQualitySettings QualitySettings
        {
            get { return new RenderingQualitySettings(InternalRenderingSettings.GetRenderingQualitySettings()); }
        }
        
        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public RenderingImageExposureSettings ImageExposureSettings
        {
            get { return new RenderingImageExposureSettings(InternalRenderingSettings.GetRenderingImageExposureSettings()); }
        }
        
        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public RenderingBackgroundSettings BackgroundSettings
        {
            get { return new RenderingBackgroundSettings(InternalRenderingSettings.GetBackgroundSettings()); }
        }
    }
}
