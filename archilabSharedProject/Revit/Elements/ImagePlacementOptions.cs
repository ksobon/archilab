#if !Revit2019 && !Revit2018 && !Revit2017

using System;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class ImagePlacementOptions
    {
        internal Autodesk.Revit.DB.ImagePlacementOptions InternalImagePlacementOptions
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipo"></param>
        [SupressImportIntoVM]
        public ImagePlacementOptions(Autodesk.Revit.DB.ImagePlacementOptions ipo)
        {
            InitImagePlacementOptions(ipo);
        }

        private void InitImagePlacementOptions(Autodesk.Revit.DB.ImagePlacementOptions ipo)
        {
            InternalSetImagePlacementOptions(ipo);
        }

        private void InternalSetImagePlacementOptions(Autodesk.Revit.DB.ImagePlacementOptions ipo)
        {
            InternalImagePlacementOptions = ipo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="location"></param>
        /// <param name="boxPlacementType"></param>
        /// <returns></returns>
        public static ImagePlacementOptions Create(Point location, string boxPlacementType)
        {
            if (location == null) throw new ArgumentNullException(nameof(location));
            if (string.IsNullOrWhiteSpace(boxPlacementType)) throw new ArgumentNullException(nameof(boxPlacementType));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            var pt = location.ToRevitType();
            var bpt = (Autodesk.Revit.DB.BoxPlacement)Enum.Parse(typeof(Autodesk.Revit.DB.BoxPlacement), boxPlacementType);

            TransactionManager.Instance.EnsureInTransaction(doc);
            var imageOptions = new Autodesk.Revit.DB.ImagePlacementOptions(pt, bpt);
            TransactionManager.Instance.TransactionTaskDone();

            return new ImagePlacementOptions(imageOptions);
        }
    }
}

#endif
