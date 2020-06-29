using System.Collections.Generic;
using System.Globalization;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;
using Revit.Elements.Views;
using RevitServices.Persistence;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class SunAndShadowSettings
    {
        internal SunAndShadowSettings()
        {
        }

        internal Autodesk.Revit.DB.SunAndShadowSettings InternalSunAndShadowSettings { get; set; }

        internal SunAndShadowSettings(Autodesk.Revit.DB.SunAndShadowSettings settings)
        {
            InternalSunAndShadowSettings = settings;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        [MultiReturn("Latitude", "Longitude", "UsesDST", "StartDateTime", "EndDateTime", "UsesSharedSettings", 
            "UsesGroundPlane", "Level", "SunriseToSunset", "Frames", "TimeInterval", "Azimuth", "Altitude", "RelativeToView")]
        public static Dictionary<string, object> Settings(View view)
        {
            var v = (Autodesk.Revit.DB.View)view.InternalElement;
            var settings = v.SunAndShadowSettings;
            var doc = DocumentManager.Instance.CurrentDBDocument;

            return new Dictionary<string, object>
            {
                {"Latitude", settings.Latitude},
                {"Longitude", settings.Longitude},
                {"UsesDST", settings.UsesDST},
                {"StartDateTime", settings.StartDateAndTime.ToString(CultureInfo.InvariantCulture)},
                {"EndDateTime", settings.EndDateAndTime.ToString(CultureInfo.InvariantCulture)},
                {"UsesSharedSettings", settings.SharesSettings},
                {"UsesGroundPlane", settings.UsesGroundPlane},
                {"Level", settings.GroundPlaneLevelId != Autodesk.Revit.DB.ElementId.InvalidElementId ? "<none>" : doc.GetElement(settings.GroundPlaneLevelId)?.Name},
                {"SunriseToSunset", settings.SunriseToSunset},
                {"Frames", settings.NumberOfFrames},
                {"TimeInterval", settings.TimeInterval.ToString()},
                {"Azimuth", settings.Azimuth},
                {"Altitude", settings.Altitude},
                {"RelativeToView", settings.RelativeToView},
            };
        }

        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public string SolarStudy
        {
            get { return InternalSunAndShadowSettings.SunAndShadowType.ToString(); }
        }
    }
}
