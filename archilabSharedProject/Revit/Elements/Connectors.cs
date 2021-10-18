using System;
using Dynamo.Graph.Nodes;
using Autodesk.DesignScript.Geometry;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class Connectors
    {
        internal Autodesk.Revit.DB.Connector InternalConnector { get; set; }

        internal Connectors()
        {
        }   

        internal Connectors(Autodesk.Revit.DB.Connector conn)
        {
            InternalConnector = conn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connector1"></param>
        /// <param name="connector2"></param>
        public static void Connect(Connectors connector1, Connectors connector2)
        {
            if (!(connector1.InternalConnector is Autodesk.Revit.DB.Connector c1))
                throw new ArgumentNullException(nameof(connector1));
            if (!(connector2.InternalConnector is Autodesk.Revit.DB.Connector c2))
                throw new ArgumentNullException(nameof(connector2));

            var doc = DocumentManager.Instance.CurrentDBDocument;
            TransactionManager.Instance.EnsureInTransaction(doc);
            c1.ConnectTo(c2);
            TransactionManager.Instance.TransactionTaskDone();
        }

        /// <summary>
        /// 
        /// </summary>
        [NodeCategory("Query")]
        public Point Point
        {
            get { return InternalConnector.Origin.ToPoint(); }
        }
    }
}
