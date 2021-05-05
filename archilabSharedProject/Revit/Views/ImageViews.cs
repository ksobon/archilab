using archilab.Revit.Elements;
using Dynamo.Graph.Nodes;
using DynamoServices;
using Revit.Elements.Views;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Element = Autodesk.Revit.DB.Element;

// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Views
{
    /// <summary>
    /// 
    /// </summary>
    [RegisterForTrace]
    public class ImageViews : View
    {
        internal Autodesk.Revit.DB.ImageView InternalImageView { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public override Element InternalElement
        {
            get { return InternalImageView; }
        }

        private ImageViews(Autodesk.Revit.DB.ImageExportOptions options)
        {
            SafeInit(() => InitImageView(options));
        }

        private void InitImageView(Autodesk.Revit.DB.ImageExportOptions options)
        {
            // Phase 1 - Check to see if the object exists and should be rebound
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var oldEle = ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.ImageView>(doc);

            // Rebind to Element
            if (oldEle != null)
            {
                InternalSetImageView(oldEle);
                return;
            }

            // Phase 2 - There was no existing Element, create new one
            TransactionManager.Instance.EnsureInTransaction(doc);

            var id = doc.SaveToProjectAsImage(options);
            var view = doc.GetElement(id) as Autodesk.Revit.DB.ImageView;

            InternalSetImageView(view);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(InternalElement);
        }

        private void InternalSetImageView(Autodesk.Revit.DB.ImageView view)
        {
            InternalImageView = view;
            InternalElementId = view.Id;
            InternalUniqueId = view.UniqueId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static View SaveToProject(ImageExportOptions options)
        {
            var o = options.InternalImageExportOptions;
            var view = new ImageViews(o);
            
            return view;
        }
    }
}
