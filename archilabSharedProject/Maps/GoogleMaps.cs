using System;
using System.Net;
// ReSharper disable UnusedMember.Global

namespace archilab.Maps
{
    /// <summary>
    /// 
    /// </summary>
    public class GoogleMaps
    {
        internal string ApiKey { get; set; }
        internal string BaseUrl { get; set; } = "https://maps.googleapis.com/maps/api/staticmap";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="apiKey"></param>
        public GoogleMaps(string apiKey)
        {
            ApiKey = apiKey;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public string DownloadMap(MapSettings settings, string filePath)
        {
            var url =
                $"{BaseUrl}?center={settings.Latitude},{settings.Longitude}&zoom={settings.Zoom}&size={settings.Width}x{settings.Height}&scale={settings.Scale}&format={settings.ImageFormat}&maptype={settings.MapType}&key={ApiKey}";

            using (var wc = new WebClient())
            {
                wc.DownloadFile(url, filePath);
            }

            return filePath;
        }
    }
}
