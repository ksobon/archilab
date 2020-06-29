using System.Collections.Generic;
using DSCore;
using Dynamo.Graph.Nodes;
using RevitServices.Persistence;
using archilab.Utilities;
using Autodesk.DesignScript.Runtime;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class OverrideGraphicsSettings
    {
        internal OverrideGraphicsSettings()
        {
        }

        internal Autodesk.Revit.DB.OverrideGraphicSettings InternalOverrideGraphicSettings { get; set; }

        internal OverrideGraphicsSettings(Autodesk.Revit.DB.OverrideGraphicSettings settings)
        {
            InternalOverrideGraphicSettings = settings;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [NodeCategory("Query")]
        [MultiReturn("Halftone", "Transparency", "ProjectionLineColor", "ProjectionLineWeight", "ProjectionLinePattern",
            "CutLineColor", "CutLineWeight", "CutLinePattern"
#if !Revit2018 && !Revit2017
            ,"IsSurfaceForegroundPatternVisible", "SurfaceForegroundPattern", "SurfaceForegroundColor", "IsSurfaceBackgroundPatternVisible", 
            "SurfaceBackgroundPattern", "SurfaceBackgroundColor", "IsCutForegroundPatternVisible", "CutForegroundPattern", "CutForegroundColor", 
            "IsCutBackgroundPatternVisible", "CutBackgroundPattern", "CutBackgroundColor"
#endif
            )]
        public Dictionary<string, object> Properties()
        {
            return new Dictionary<string, object>
            {
                {"Halftone", Halftone},
                {"Transparency", Transparency},
                {"ProjectionLineColor", ProjectionLineColor},
                {"ProjectionLineWeight", ProjectionLineWeight},
                {"ProjectionLinePattern", ProjectionLinePattern},
                {"CutLineColor", CutLineColor},
                {"CutLineWeight", CutLineWeight},
                {"CutLinePattern", CutLinePattern},
#if !Revit2018 && !Revit2017
                {"IsSurfaceForegroundPatternVisible", IsSurfaceForegroundPatternVisible},
                {"SurfaceForegroundPattern", SurfaceForegroundPattern},
                {"SurfaceForegroundColor", SurfaceForegroundColor},
                {"IsSurfaceBackgroundPatternVisible", IsSurfaceBackgroundPatternVisible},
                {"SurfaceBackgroundPattern", SurfaceBackgroundPattern},
                {"SurfaceBackgroundColor", SurfaceBackgroundColor},
                {"IsCutForegroundPatternVisible", IsCutForegroundPatternVisible},
                {"CutForegroundPattern", CutForegroundPattern},
                {"CutForegroundColor", CutForegroundColor},
                {"IsCutBackgroundPatternVisible", IsCutBackgroundPatternVisible},
                {"CutBackgroundPattern", CutBackgroundPattern},
                {"CutBackgroundColor", CutBackgroundColor}
#endif
            };
        }

        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public bool Halftone
        {
            get { return InternalOverrideGraphicSettings.Halftone; }
        }

        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public int Transparency
        {
            get { return InternalOverrideGraphicSettings.Transparency; }
        }

        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public string ProjectionLinePattern
        {
            get
            {
                // (Konrad) Solid line pattern is hard coded and would not be allowed to be selected.
                var linePatternId = InternalOverrideGraphicSettings.ProjectionLinePatternId;
                if (linePatternId.IntegerValue == -3000010)
                    return "Solid";

                return DocumentManager.Instance.CurrentDBDocument
                    .GetElement(linePatternId)?.Name;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public Color ProjectionLineColor
        {
            get { return ColorUtilities.DsColorByColor(InternalOverrideGraphicSettings.ProjectionLineColor); }
        }

        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public int ProjectionLineWeight
        {
            get { return InternalOverrideGraphicSettings.ProjectionLineWeight; }
        }

#if !Revit2018 && !Revit2017

        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public bool IsSurfaceForegroundPatternVisible
        {
            get { return InternalOverrideGraphicSettings.IsSurfaceForegroundPatternVisible; }
        }

        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public string SurfaceForegroundPattern
        {
            get
            {
                return DocumentManager.Instance.CurrentDBDocument
                    .GetElement(InternalOverrideGraphicSettings.SurfaceForegroundPatternId)?.Name;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public Color SurfaceForegroundColor
        {
            get { return ColorUtilities.DsColorByColor(InternalOverrideGraphicSettings.SurfaceForegroundPatternColor); }
        }

        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public bool IsSurfaceBackgroundPatternVisible
        {
            get { return InternalOverrideGraphicSettings.IsSurfaceBackgroundPatternVisible; }
        }

        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public string SurfaceBackgroundPattern
        {
            get
            {
                return DocumentManager.Instance.CurrentDBDocument
                    .GetElement(InternalOverrideGraphicSettings.SurfaceBackgroundPatternId)?.Name;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public Color SurfaceBackgroundColor
        {
            get { return ColorUtilities.DsColorByColor(InternalOverrideGraphicSettings.SurfaceBackgroundPatternColor); }
        }

#endif

        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public string CutLinePattern
        {
            get
            {
                // (Konrad) Solid line pattern is hard coded and would not be allowed to be selected.
                var linePatternId = InternalOverrideGraphicSettings.CutLinePatternId;
                if (linePatternId.IntegerValue == -3000010)
                    return "Solid";

                return DocumentManager.Instance.CurrentDBDocument
                    .GetElement(linePatternId)?.Name;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public Color CutLineColor
        {
            get { return ColorUtilities.DsColorByColor(InternalOverrideGraphicSettings.CutLineColor); }
        }

        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public int CutLineWeight
        {
            get { return InternalOverrideGraphicSettings.CutLineWeight; }
        }

#if !Revit2018 && !Revit2017

        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public bool IsCutForegroundPatternVisible
        {
            get { return InternalOverrideGraphicSettings.IsCutForegroundPatternVisible; }
        }

        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public string CutForegroundPattern
        {
            get
            {
                return DocumentManager.Instance.CurrentDBDocument
                    .GetElement(InternalOverrideGraphicSettings.CutForegroundPatternId)?.Name;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public Color CutForegroundColor
        {
            get { return ColorUtilities.DsColorByColor(InternalOverrideGraphicSettings.CutForegroundPatternColor); }
        }

        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public bool IsCutBackgroundPatternVisible
        {
            get { return InternalOverrideGraphicSettings.IsCutBackgroundPatternVisible; }
        }

        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public string CutBackgroundPattern
        {
            get
            {
                return DocumentManager.Instance.CurrentDBDocument
                    .GetElement(InternalOverrideGraphicSettings.CutBackgroundPatternId)?.Name;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public Color CutBackgroundColor
        {
            get { return ColorUtilities.DsColorByColor(InternalOverrideGraphicSettings.CutBackgroundPatternColor); }
        }

#endif
    }
}