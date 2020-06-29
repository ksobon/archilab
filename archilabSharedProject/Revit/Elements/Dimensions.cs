using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using DynamoServices;
using Dynamo.Graph.Nodes;
using Revit.Elements;
using Revit.Elements.Views;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    [RegisterForTrace]
    public class Dimensions : Element
    {
        internal Autodesk.Revit.DB.Dimension InternalDimension
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalDimension; }
        }

        ///
        protected Dimensions(Autodesk.Revit.DB.View view, Autodesk.Revit.DB.Line line, Autodesk.Revit.DB.ReferenceArray arr, Autodesk.Revit.DB.DimensionType type)
        {
            SafeInit(() => InitDimension(view, line, arr, type));
        }

        private void InternalSetDimension(Autodesk.Revit.DB.Dimension d)
        {
            InternalDimension = d;
        }

        private void InitDimension(Autodesk.Revit.DB.View view, Autodesk.Revit.DB.Line line, Autodesk.Revit.DB.ReferenceArray arr, Autodesk.Revit.DB.DimensionType type)
        {
            //Phase 1 - Check to see if the object exists and should be rebound
            var oldEle = ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.Dimension>(DocumentManager.Instance.CurrentDBDocument);

            // Rebind to Element
            if (oldEle != null)
            {
                InternalSetDimension(oldEle);
                return;
            }

            //Phase 2 - There was no existing Element, create new one
            TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);

            var ii = DocumentManager.Instance.CurrentDBDocument.Create.NewDimension(view, line, arr, type);

            InternalSetDimension(ii);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(InternalElement);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view"></param>
        /// <param name="line"></param>
        /// <param name="references"></param>
        /// <param name="dimType"></param>
        /// <returns></returns>
        [NodeCategory("Create")]
        public static Element Create(View view, Line line, List<References> references, Element dimType)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));
            if (line == null)
                throw new ArgumentNullException(nameof(line));
            if (references == null || !references.Any())
                throw new ArgumentNullException(nameof(references));
            if (dimType == null)
                throw new ArgumentNullException(nameof(dimType));

            var v = view.InternalElement as Autodesk.Revit.DB.View;
            var l = line.ToRevitType() as Autodesk.Revit.DB.Line;
            var dt = dimType.InternalElement as Autodesk.Revit.DB.DimensionType;
            var arr = new Autodesk.Revit.DB.ReferenceArray();
            foreach (var r in references)
            {
                arr.Append(r.InternalElement);
            }

            return new Dimensions(v, l, arr, dt).InternalElement.ToDSType(true);
        }
    }
}
