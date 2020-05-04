#if !Revit2019 && !Revit2018 && !Revit2017

using System;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using RevitServices.Persistence;
using RevitServices.Transactions;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class ImageTypes
    {
        internal Autodesk.Revit.DB.ImageType InternalImageType
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ito"></param>
        [SupressImportIntoVM]
        public ImageTypes(Autodesk.Revit.DB.ImageType ito)
        {
            InitImageType(ito);
        }

        private void InitImageType(Autodesk.Revit.DB.ImageType ito)
        {
            InternalSetImageType(ito);
        }

        private void InternalSetImageType(Autodesk.Revit.DB.ImageType it)
        {
            InternalImageType = it;
        }

        /// <summary>
        /// Creates a new Image Type import object.
        /// </summary>
        /// <param name="imageTypeOptions">Image Type Options object.</param>
        /// <param name="reload">If true reload existing document, otherwise create new one.</param>
        /// <returns>Image Type object.</returns>
        public static ImageTypes Create(ImageTypeOptions imageTypeOptions, bool reload = true)
        {
            try
            {
                var doc = DocumentManager.Instance.CurrentDBDocument;
                TransactionManager.Instance.EnsureInTransaction(doc);

                Autodesk.Revit.DB.ImageType it;
                if (reload)
                {
                    var existing = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                        .OfClass(typeof(Autodesk.Revit.DB.ImageType))
                        .WhereElementIsElementType()
                        .Cast<Autodesk.Revit.DB.ImageType>()
                        .FirstOrDefault(x => x.Path == imageTypeOptions.FilePath && 
                                             x.PageNumber == imageTypeOptions.PageNumber);
                    if (existing == null)
                        it = Autodesk.Revit.DB.ImageType.Create(doc, imageTypeOptions.InternalImageTypeOptions);
                    else
                    {
                        existing.ReloadFrom(imageTypeOptions.InternalImageTypeOptions);
                        it = existing;
                    }
                }
                else
                {
                    it = Autodesk.Revit.DB.ImageType.Create(doc, imageTypeOptions.InternalImageTypeOptions);
                }
                TransactionManager.Instance.TransactionTaskDone();

                return new ImageTypes(it);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// Retrieves Page Number that import was created from.
        /// </summary>
        public int PageNumber
        {
            get { return InternalImageType.PageNumber; }
        }

        /// <summary>
        /// Retrieves File Path that import was created from.
        /// </summary>
        public string FilePath
        {
            get { return InternalImageType.Path; }
        }
    }
}

#endif
