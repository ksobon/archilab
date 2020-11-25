using System;
// ReSharper disable UnusedMember.Global

namespace archilab.Maps
{
    /// <summary>
    /// 
    /// </summary>
    public class MapSettings
    {
        /// <summary>
        /// 
        /// </summary>
        public MapSettings()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="zoom"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="scale"></param>
        /// <param name="imageFormat"></param>
        /// <param name="mapType"></param>
        public MapSettings(
            double lat = 40.714728, 
            double lon = -73.998672, 
            int zoom = 12, 
            int width = 400, 
            int height = 400, 
            int scale = 1,
            string imageFormat = "png",
            string mapType = "roadmap")
        {
            Latitude = lat;
            Longitude = lon;

            if (width > 640 || width < 1)
                throw new ArgumentException("Image Width can be max of 640. Please use scale value of 2 to get 1280px size.");
            Width = width;

            if (height > 640 || height < 1)
                throw new ArgumentException("Image Height can be max of 640. Please use scale value of 2 to get 1280px size.");
            Height = height;

            if (zoom > 20 || zoom < 1)
                throw new ArgumentException("Zoom value can only be set between 1-20.");

            Zoom = zoom;

            if (scale > 2 || scale < 1)
                throw new ArgumentException("Scale can only be set to 1 or 2.");

            MapType = mapType;
            ImageFormat = imageFormat;
        }

        /// <summary>
        /// 
        /// </summary>
        public string MapType { get; set; } = "roadmap";

        /// <summary>
        /// 
        /// </summary>
        public string ImageFormat { get; set; } = "png";

        /// <summary>
        /// 
        /// </summary>
        public int Scale { get; set; } = 1;

        /// <summary>
        /// 
        /// </summary>
        public double Latitude { get; set; } = 40.714728;

        /// <summary>
        /// 
        /// </summary>
        public double Longitude { get; set; } = -73.998672;

        /// <summary>
        /// 
        /// </summary>
        public int Zoom { get; set; } = 12;

        /// <summary>
        /// 
        /// </summary>
        public int Width { get; set; } = 400;

        /// <summary>
        /// 
        /// </summary>
        public int Height { get; set; } = 400;
    }
}
