using System.Collections.Generic;
using DSCore;
using Dynamo.Graph.Nodes;
using RevitServices.Persistence;
using archilab.Utilities;
using Autodesk.DesignScript.Runtime;
using Revit.Elements;
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
        /// <param name="projectionLineColor"></param>
        /// <param name="projectionLineWeight"></param>
        /// <param name="projectionLinePattern"></param>
        /// <param name="projectionForegroundFillPattern"></param>
        /// <param name="projectionForegroundFillColor"></param>
        /// <param name="projectionBackgroundFillPattern"></param>
        /// <param name="projectionBackgroundFillColor"></param>
        /// <param name="cutLineColor"></param>
        /// <param name="cutLineWeight"></param>
        /// <param name="cutLinePattern"></param>
        /// <param name="cutForegroundFillPattern"></param>
        /// <param name="cutForegroundFillColor"></param>
        /// <param name="cutBackgroundFillPattern"></param>
        /// <param name="cutBackgroundFillColor"></param>
        /// <param name="transparency"></param>
        /// <param name="halftone"></param>
        /// <returns name="overrides">Override Graphics Settings.</returns>
        public static OverrideGraphicsSettings Create(
            [DefaultArgument("Selection.Select.GetNull()")] Color projectionLineColor,
            [DefaultArgument("Selection.Select.GetNull()")] int? projectionLineWeight,
            [DefaultArgument("Selection.Select.GetNull()")] Element projectionLinePattern,
            [DefaultArgument("Selection.Select.GetNull()")] Element projectionForegroundFillPattern,
            [DefaultArgument("Selection.Select.GetNull()")] Color projectionForegroundFillColor, 
            [DefaultArgument("Selection.Select.GetNull()")] Element projectionBackgroundFillPattern,
            [DefaultArgument("Selection.Select.GetNull()")] Color projectionBackgroundFillColor,
            [DefaultArgument("Selection.Select.GetNull()")] Color cutLineColor,
            [DefaultArgument("Selection.Select.GetNull()")] int? cutLineWeight,
            [DefaultArgument("Selection.Select.GetNull()")] Element cutLinePattern,
            [DefaultArgument("Selection.Select.GetNull()")] Element cutForegroundFillPattern,
            [DefaultArgument("Selection.Select.GetNull()")] Color cutForegroundFillColor,
            [DefaultArgument("Selection.Select.GetNull()")] Element cutBackgroundFillPattern,
            [DefaultArgument("Selection.Select.GetNull()")] Color cutBackgroundFillColor,
            [DefaultArgument("Selection.Select.GetNull()")] int? transparency = null,
            bool halftone = false)
        {
            var ogs = new Autodesk.Revit.DB.OverrideGraphicSettings();

            if (projectionLineColor != null)
                ogs.SetProjectionLineColor(new Autodesk.Revit.DB.Color(projectionLineColor.Red,
                    projectionLineColor.Green, projectionLineColor.Blue));
            if (projectionLineWeight.HasValue)
                ogs.SetProjectionLineWeight(projectionLineWeight.Value);
            if (projectionLinePattern != null)
                ogs.SetProjectionLinePatternId(projectionLinePattern.InternalElement.Id);

            if (projectionForegroundFillPattern != null)
            {
                ogs.SetSurfaceForegroundPatternId(projectionForegroundFillPattern.InternalElement.Id);
                ogs.SetSurfaceForegroundPatternVisible(true);
            }
            if (projectionForegroundFillColor != null)
                ogs.SetSurfaceForegroundPatternColor(new Autodesk.Revit.DB.Color(projectionForegroundFillColor.Red,
                    projectionForegroundFillColor.Green, projectionForegroundFillColor.Blue));
            if (projectionBackgroundFillPattern != null)
            {
                ogs.SetSurfaceBackgroundPatternId(projectionBackgroundFillPattern.InternalElement.Id);
                ogs.SetSurfaceBackgroundPatternVisible(true);
            }
            if (projectionBackgroundFillColor != null)
            {
                ogs.SetSurfaceBackgroundPatternColor(new Autodesk.Revit.DB.Color(projectionBackgroundFillColor.Red,
                    projectionBackgroundFillColor.Green, projectionBackgroundFillColor.Blue));
            }

            if (transparency.HasValue)
                ogs.SetSurfaceTransparency(transparency.Value);

            if (cutLineColor != null)
                ogs.SetCutLineColor(new Autodesk.Revit.DB.Color(cutLineColor.Red, cutLineColor.Green, cutLineColor.Blue));
            if (cutLineWeight.HasValue)
                ogs.SetCutLineWeight(cutLineWeight.Value);
            if (cutLinePattern != null)
                ogs.SetCutLinePatternId(cutLinePattern.InternalElement.Id);

            if (cutForegroundFillPattern != null)
            {
                ogs.SetCutForegroundPatternId(cutForegroundFillPattern.InternalElement.Id);
                ogs.SetCutForegroundPatternVisible(true);
            }
            if (cutForegroundFillColor != null)
                ogs.SetCutForegroundPatternColor(new Autodesk.Revit.DB.Color(cutForegroundFillColor.Red,
                    cutForegroundFillColor.Green, cutForegroundFillColor.Blue));
            if (cutBackgroundFillPattern != null)
            {
                ogs.SetCutBackgroundPatternId(cutBackgroundFillPattern.InternalElement.Id);
                ogs.SetCutBackgroundPatternVisible(true);
            }
            if (cutBackgroundFillColor != null)
            {
                ogs.SetCutBackgroundPatternColor(new Autodesk.Revit.DB.Color(cutBackgroundFillColor.Red,
                    cutBackgroundFillColor.Green, cutBackgroundFillColor.Blue));
            }

            if (halftone)
                ogs.SetHalftone(halftone);

            return new OverrideGraphicsSettings(ogs);
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