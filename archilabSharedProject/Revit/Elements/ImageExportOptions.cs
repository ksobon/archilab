using System;
using System.Collections.Generic;
using System.Linq;
using Revit.Elements;
using Revit.Elements.Views;
using RevitServices.Persistence;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class ImageExportOptions
    {
        internal ImageExportOptions()
        {
        }

        internal Autodesk.Revit.DB.ImageExportOptions InternalImageExportOptions { get; set; }

        internal ImageExportOptions(Autodesk.Revit.DB.ImageExportOptions options)
        {
            InternalImageExportOptions = options;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exportRange"></param>
        /// <param name="filePath"></param>
        /// <param name="fitDirection"></param>
        /// <param name="fileType"></param>
        /// <param name="resolution"></param>
        /// <param name="viewName"></param>
        /// <param name="zoomType"></param>
        /// <param name="size"></param>
        /// <param name="zoom"></param>
        /// <param name="createWebSite"></param>
        /// <returns></returns>
        public static ImageExportOptions Create(
            string exportRange,
            string fitDirection, 
            string fileType, 
            string resolution,
            string zoomType,
            string viewName,
            int size = 512,
            int zoom = 50,
            bool createWebSite = false,
            string filePath = "")
        {
            var er = (Autodesk.Revit.DB.ExportRange)Enum.Parse(typeof(Autodesk.Revit.DB.ExportRange), exportRange);
            var fd = (Autodesk.Revit.DB.FitDirectionType)Enum.Parse(typeof(Autodesk.Revit.DB.FitDirectionType), fitDirection);
            var ft = (Autodesk.Revit.DB.ImageFileType)Enum.Parse(typeof(Autodesk.Revit.DB.ImageFileType), fileType);
            var r = (Autodesk.Revit.DB.ImageResolution)Enum.Parse(typeof(Autodesk.Revit.DB.ImageResolution), resolution);
            var zt = (Autodesk.Revit.DB.ZoomFitType)Enum.Parse(typeof(Autodesk.Revit.DB.ZoomFitType), zoomType);
            var options = new Autodesk.Revit.DB.ImageExportOptions
            {
                ExportRange = er,
                FilePath = filePath,
                FitDirection = fd,
                HLRandWFViewsFileType = ft,
                ImageResolution = r,
                PixelSize = size,
                ShadowViewsFileType = ft,
                ShouldCreateWebSite = createWebSite,
                ViewName = viewName,
                Zoom = zoom,
                ZoomType = zt
            };

            return new ImageExportOptions(options);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="views"></param>
        /// <returns></returns>
        public static ImageExportOptions SetViewsAndSheets(ImageExportOptions options, List<View> views)
        {
            var o = options.InternalImageExportOptions;
            o.SetViewsAndSheets(views.Select(x => x.InternalElement.Id).ToList());

            return new ImageExportOptions(o);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static List<View> GetViews(ImageExportOptions options)
        {
            var o = options.InternalImageExportOptions;
            var doc = DocumentManager.Instance.CurrentDBDocument;

            return o.GetViewsAndSheets().Select(x => doc.GetElement(x).ToDSType(true) as View).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public static string GetFileName(View view)
        {
            var v = (Autodesk.Revit.DB.View)view.InternalElement;
            var doc = DocumentManager.Instance.CurrentDBDocument;

            return Autodesk.Revit.DB.ImageExportOptions.GetFileName(doc, v.Id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool IsValidFileName(string filePath)
        {
            return Autodesk.Revit.DB.ImageExportOptions.IsValidFileName(filePath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public bool IsValidForSaveToProjectAsImage(ImageExportOptions options)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            return Autodesk.Revit.DB.ImageExportOptions.IsValidForSaveToProjectAsImage(options.InternalImageExportOptions, doc);
        }
    }
}
