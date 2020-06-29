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
    public class ViewDisplayBackgrounds
    {
        internal ViewDisplayBackgrounds()
        {
        }

        internal Autodesk.Revit.DB.ViewDisplayBackground InternalViewDisplayBackground { get; set; }

        internal ViewDisplayBackgrounds(Autodesk.Revit.DB.ViewDisplayBackground background)
        {
            InternalViewDisplayBackground = background;
        }

        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public string Type
        {
            get { return InternalViewDisplayBackground.Type.ToString(); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [NodeCategory("Query")]
        [MultiReturn("HorizonColor", "SkyColor", "GroundColor", "HorizontalImageOffset", "VerticalImageOffset", 
            "HorizontalImageScaling", "VerticalImageScaling", "ImageFlags", "ImagePath")]
        public Dictionary<string, object> Properties()
        {
            return new Dictionary<string, object>
            {
                {"HorizonColor", ColorUtil.GetColor(InternalViewDisplayBackground.BackgroundColor)},
                {"SkyColor", ColorUtil.GetColor(InternalViewDisplayBackground.SkyColor)},
                {"GroundColor", ColorUtil.GetColor(InternalViewDisplayBackground.GroundColor)},
                {"HorizontalImageOffset", InternalViewDisplayBackground.HorizontalImageOffset},
                {"VerticalImageOffset", InternalViewDisplayBackground.VerticalImageOffset},
                {"HorizontalImageScaling", InternalViewDisplayBackground.HorizontalImageScale},
                {"VerticalImageScaling", InternalViewDisplayBackground.VerticalImageScale},
                {"ImageFlags", InternalViewDisplayBackground.ImageFlags.ToString()},
                {"ImagePath", InternalViewDisplayBackground.ImagePath}
            };
        }
    }
}
