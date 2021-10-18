using System;
using System.Linq;
using Dynamo.Graph.Nodes;
using Revit.Elements;
using RevitServices.Persistence;
using RevitServices.Transactions;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// Parameter related utilities.
    /// </summary>
    public class Parameter
    {
        internal Parameter()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="bipName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static Element SetBuiltInParameter(Element element, string bipName, object value)
        {
            if (element == null)
                throw new ArgumentException(nameof(element));
            if (string.IsNullOrWhiteSpace(bipName))
                throw new ArgumentException(nameof(bipName));

            var bip = (Autodesk.Revit.DB.BuiltInParameter)Enum.Parse(typeof(Autodesk.Revit.DB.BuiltInParameter), bipName);
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var p = element.InternalElement.get_Parameter(bip);
            
            TransactionManager.Instance.EnsureInTransaction(doc);
            switch (p.StorageType)
            {
                case Autodesk.Revit.DB.StorageType.None:
                    break;
                case Autodesk.Revit.DB.StorageType.Integer:
                    p.Set((int) value);
                    break;
                case Autodesk.Revit.DB.StorageType.Double:
                    p.Set((double) value);
                    break;
                case Autodesk.Revit.DB.StorageType.String:
                    p.Set((string) value);
                    break;
                case Autodesk.Revit.DB.StorageType.ElementId:
                    var id = GetElementId(value);
                    p.Set(id);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            TransactionManager.Instance.TransactionTaskDone();

            return element;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="bipName"></param>
        /// <returns></returns>
        [NodeCategory("Query")]
        public static object GetBuiltInParameter(Element element, string bipName)
        {
            if (element == null)
                throw new ArgumentException(nameof(element));
            if (string.IsNullOrWhiteSpace(bipName))
                throw new ArgumentException(nameof(bipName));

            var bip = (Autodesk.Revit.DB.BuiltInParameter)Enum.Parse(typeof(Autodesk.Revit.DB.BuiltInParameter), bipName);
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var p = element.InternalElement.get_Parameter(bip);

            switch (p.StorageType)
            {
                case Autodesk.Revit.DB.StorageType.None:
                    return null;
                case Autodesk.Revit.DB.StorageType.Integer:
                    return p.AsInteger();
                case Autodesk.Revit.DB.StorageType.Double:
                    return p.AsDouble();
                case Autodesk.Revit.DB.StorageType.String:
                    return p.AsString();
                case Autodesk.Revit.DB.StorageType.ElementId:
                    var id = p.AsElementId();
                    return doc.GetElement(id).ToDSType(true);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Returns name of the BuiltInParameter if such exists. 
        /// </summary>
        /// <param name="element">Element to query.</param>
        /// <param name="name">Name of the parameter. In case that multiple parameters have the same name, first encountered name will be returned.</param>
        /// <returns name="bipName">Name of the Bip.</returns>
        [NodeCategory("Query")]
        public static string GetBuiltInParameterName(Element element, string name)
        {
            var e = element.InternalElement;
            var param = e.Parameters.Cast<Autodesk.Revit.DB.Parameter>().FirstOrDefault(x => x.Definition.Name == name);

            return ((Autodesk.Revit.DB.InternalDefinition)param?.Definition)?.BuiltInParameter.ToString();
        }

        #region Utilities

        private static Autodesk.Revit.DB.ElementId GetElementId(object id)
        {
            switch (Type.GetTypeCode(id.GetType()))
            {
                case TypeCode.String:
                    return new Autodesk.Revit.DB.ElementId(int.Parse((string)id));
                case TypeCode.Double:
                    var integerValue = Convert.ToInt32(id);
                    return new Autodesk.Revit.DB.ElementId(integerValue);
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Int16:
                    return new Autodesk.Revit.DB.ElementId(int.Parse(id.ToString()));
                default:
                    return (Autodesk.Revit.DB.ElementId)id;
            }
        }

        #endregion
    }
}
