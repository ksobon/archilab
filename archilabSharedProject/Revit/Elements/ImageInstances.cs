#if !Revit2019 && !Revit2018 && !Revit2017

using DynamoServices;
using Revit.Elements;
using Revit.Elements.Views;
using RevitServices.Persistence;
using RevitServices.Transactions;
using System;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    [RegisterForTrace]
    public class ImageInstances : Element
    {
        internal Autodesk.Revit.DB.ImageInstance InternalImageInstance
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalImageInstance; }
        }

        private ImageInstances(Autodesk.Revit.DB.View view, Autodesk.Revit.DB.ElementId typeId, Autodesk.Revit.DB.ImagePlacementOptions options)
        {
            SafeInit(() => InitImageInstance(view, typeId, options));
        }

        private void InitImageInstance(Autodesk.Revit.DB.View view, Autodesk.Revit.DB.ElementId typeId, Autodesk.Revit.DB.ImagePlacementOptions options)
        {
            //Phase 1 - Check to see if the object exists and should be rebound
            var oldEle = ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.ImageInstance>(DocumentManager.Instance.CurrentDBDocument);

            // Rebind to Element
            if (oldEle != null)
            {
                InternalSetImageInstance(oldEle);
                return;
            }

            //Phase 2 - There was no existing Element, create new one
            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);

            var ii = Autodesk.Revit.DB.ImageInstance.Create(DocumentManager.Instance.CurrentDBDocument, view, typeId, options);

            InternalSetImageInstance(ii);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(InternalElement);
        }

        private void InternalSetImageInstance(Autodesk.Revit.DB.ImageInstance ii)
        {
            InternalImageInstance = ii;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="imageType"></param>
        /// <param name="imagePlacementOptions"></param>
        /// <returns></returns>
        public static Element Create(View view, ImageTypes imageType, ImagePlacementOptions imagePlacementOptions)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));
            if (imageType == null) throw new ArgumentNullException(nameof(imageType));
            if (imagePlacementOptions == null) throw new ArgumentNullException(nameof(imagePlacementOptions));

            var ipo = new ImageInstances(view.InternalElement as Autodesk.Revit.DB.View, imageType.InternalImageType.Id,
                imagePlacementOptions.InternalImagePlacementOptions);

            return ipo.InternalImageInstance.ToDSType(true);
        }
    }
}

#endif
