#if !Revit2019 && !Revit2018 && !Revit2017

using System;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using RevitServices.Persistence;
using RevitServices.Transactions;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class ImageTypeOptions
    {
        internal Autodesk.Revit.DB.ImageTypeOptions InternalImageTypeOptions
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ito"></param>
        [SupressImportIntoVM]
        public ImageTypeOptions(Autodesk.Revit.DB.ImageTypeOptions ito)
        {
            InitImageTypeOptions(ito);
        }

        private void InitImageTypeOptions(Autodesk.Revit.DB.ImageTypeOptions ito)
        {
            InternalSetImageTypeOptions(ito);
        }

        private void InternalSetImageTypeOptions(Autodesk.Revit.DB.ImageTypeOptions ito)
        {
            InternalImageTypeOptions = ito;
        }

        /// <summary>
        /// Creates Image Type Options for image import.
        /// </summary>
        /// <param name="filePath">File path to Image.</param>
        /// <returns>Image Type Options object.</returns>
        public static ImageTypeOptions Create(string filePath)
        {
            try
            {
                var doc = DocumentManager.Instance.CurrentDBDocument;
                TransactionManager.Instance.EnsureInTransaction(doc);
#if !Revit2018 && !Revit2019 && !Revit2020 && !Revit2021
                var options = new Autodesk.Revit.DB.ImageTypeOptions(filePath, false, ImageTypeSource.Link);
#else
                var options = new Autodesk.Revit.DB.ImageTypeOptions(filePath);
#endif
                TransactionManager.Instance.TransactionTaskDone();

                return new ImageTypeOptions(options);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// Creates Image Type Options for image/pdf import.
        /// </summary>
        /// <param name="filePath">File path to image/PDF.</param>
        /// <param name="resolution">Optional resolution.</param>
        /// <param name="pageNumber">Optional page number if PDF has more than one page.</param>
        /// <returns>Image Type Option object.</returns>
        // ReSharper disable once MethodOverloadWithOptionalParameter
        public static ImageTypeOptions Create(string filePath, double resolution = 72, int pageNumber = 1)
        {
            try
            {
                var doc = DocumentManager.Instance.CurrentDBDocument;
                TransactionManager.Instance.EnsureInTransaction(doc);
#if !Revit2018 && !Revit2020 && !Revit2021
                var options = new Autodesk.Revit.DB.ImageTypeOptions(filePath, false, ImageTypeSource.Link)
                {
                    PageNumber = pageNumber,
                    Resolution = resolution
                };
#else
                var options = new Autodesk.Revit.DB.ImageTypeOptions(filePath)
                {
                    PageNumber = pageNumber,
                    Resolution = resolution
                };
#endif
                TransactionManager.Instance.TransactionTaskDone();

                return new ImageTypeOptions(options);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// Retrieves Page Number that options are set to.
        /// </summary>
        public int PageNumber
        {
            get
            {
                return InternalImageTypeOptions.PageNumber;
            }
        }

        /// <summary>
        /// Retrieves Resolution that options are set to.
        /// </summary>
        public double Resolution
        {
            get { return InternalImageTypeOptions.Resolution; }
        }

        /// <summary>
        /// Retrieves File Path that options are set to.
        /// </summary>
        public string FilePath
        {
            get { return InternalImageTypeOptions.Path; }
        }
    }
}

#endif