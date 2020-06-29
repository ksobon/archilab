using System.Collections.Generic;
using archilab.Revit.Utils;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class RenderingBackgroundSettings
    {
        internal RenderingBackgroundSettings()
        {
        }

        internal Autodesk.Revit.DB.BackgroundSettings InternalBackgroundSettings { get; set; }

        internal RenderingBackgroundSettings(Autodesk.Revit.DB.BackgroundSettings settings)
        {
            InternalBackgroundSettings = settings;
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
                {"BackgroundStyle", InternalBackgroundSettings is Autodesk.Revit.DB.ColorBackgroundSettings cbs ? ColorUtil.GetColor(cbs.Color) : null},
                {"GroundColor", InternalBackgroundSettings is Autodesk.Revit.DB.GradientBackgroundSettings gbs ? ColorUtil.GetColor(gbs.GroundColor) : null},
                {"HorizonColor", InternalBackgroundSettings is Autodesk.Revit.DB.GradientBackgroundSettings gbs1 ? ColorUtil.GetColor(gbs1.HorizonColor) : null},
                {"SkyColor", InternalBackgroundSettings is Autodesk.Revit.DB.GradientBackgroundSettings gbs2 ? ColorUtil.GetColor(gbs2.SkyColor) : null},
                {"BackgroundImageFit", InternalBackgroundSettings is Autodesk.Revit.DB.ImageBackgroundSettings ibs ? ibs.BackgroundImageFit.ToString() : null},
                {"FilePath", InternalBackgroundSettings is Autodesk.Revit.DB.ImageBackgroundSettings ibs1 ? ibs1.FilePath : null},
                {"OffsetHeight", InternalBackgroundSettings is Autodesk.Revit.DB.ImageBackgroundSettings ibs2 ? ibs2.OffsetHeight : 0d},
                {"OffsetWidth", InternalBackgroundSettings is Autodesk.Revit.DB.ImageBackgroundSettings ibs3 ? ibs3.OffsetWidth : 0d},
                {"UsesSkyBackground", InternalBackgroundSettings is Autodesk.Revit.DB.SkyBackgroundSettings sbs && sbs.IsValidObject},
            };
        }
    }
}
