using System;
using System.Collections.Generic;
using System.Linq;
using archilab.Utilities;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using Category = Revit.Elements.Category;
using Element = Revit.Elements.Element;
using View = Revit.Elements.Views.View;
using Workset = archilab.Revit.Elements.Workset;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Selection
{
    /// <summary>
    /// Collection of various selection nodes. 
    /// </summary>
    public class Select
    {
        internal Select()
        {
        }

        /// <summary>
        /// Retrieves all Link Documents and their Names/Paths.
        /// </summary>
        /// <returns>Link Documents, Link Names, Link Instances and Link Paths.</returns>
        [MultiReturn("Link Doc", "Link Name", "Link Instance", "Link Path")]
        public static Dictionary<string, List<object>> GetDocuments()
        {
            var result = new Dictionary<string, List<object>>()
            {
                {"Link Doc", new List<object>()},
                {"Link Name", new List<object>()},
                {"Link Instance", new List<object>()},
                {"Link Path", new List<object>()}
            };

            var doc = DocumentManager.Instance.CurrentDBDocument;
            var instances = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                .OfClass(typeof(Autodesk.Revit.DB.RevitLinkInstance))
                .Cast<Autodesk.Revit.DB.RevitLinkInstance>()
                .ToList();
            if (!instances.Any()) return result;

            var linkDocs = new List<object>();
            var linkNames = new List<object>();
            var linkPaths = new List<object>();
            var linkInstances = new List<object>();
            foreach (var inst in instances)
            {
                linkInstances.Add(inst.ToDSType(true));
                linkDocs.Add(inst.GetLinkDocument());
                linkNames.Add(inst.Name);
                linkPaths.Add(inst.GetLinkDocument()?.PathName);
            }

            result["Link Doc"] = linkDocs;
            result["Link Name"] = linkNames;
            result["Link Instance"] = linkInstances;
            result["Link Path"] = linkPaths;

            return result;
        }

        /// <summary>
        /// Retrieves a Room at given point.
        /// </summary>
        /// <param name="point">Point.</param>
        /// <returns>Room.</returns>
        public static Element GetRoomAtPoint(Point point)
        {
            if (point == null) return null;

            var doc = DocumentManager.Instance.CurrentDBDocument;
            return doc.GetRoomAtPoint(point.ToXyz()).ToDSType(true);
        }

        /// <summary>
        /// Retrieves Survey Point from the model.
        /// </summary>
        /// <returns>Survey Point.</returns>
        public static Point GetSurveyPoint()
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var pts = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                .OfCategory(Autodesk.Revit.DB.BuiltInCategory.OST_SharedBasePoint)
                .FirstOrDefault();

            if (pts == null) return null;

            var ew = pts.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.BASEPOINT_EASTWEST_PARAM).AsDouble();
            var ns = pts.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM).AsDouble();
            var elev = pts.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.BASEPOINT_ELEVATION_PARAM).AsDouble();

            return Point.ByCoordinates(ew, ns, elev);
        }

        /// <summary>
        /// Retrieve Level by its name.
        /// </summary>
        /// <param name="name">Name of the Level.</param>
        /// <returns>Level.</returns>
        public static Level GetLevelByName(string name)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var lvl = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                .OfClass(typeof(Autodesk.Revit.DB.Level))
                .FirstOrDefault(x => x.Name == name);

            return lvl?.ToDSType(true) as Level;
        }

        /// <summary>
        /// Retrieve a TextNoteType by its name.
        /// </summary>
        /// <param name="name">Name of the Text Note Type.</param>
        /// <returns>Text note type.</returns>
        public static Element GetTextNoteTypeByName(string name)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var tnt = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                .OfClass(typeof(Autodesk.Revit.DB.TextNoteType))
                .FirstOrDefault(x => x.get_Parameter(Autodesk.Revit.DB.BuiltInParameter.ALL_MODEL_TYPE_NAME)?.AsString() == name);

            return tnt?.ToDSType(true);
        }

        /// <summary>
        /// Select all Worksets in the model.
        /// </summary>
        /// <param name="kind">Kind of a workset. By default "User Worksets" will be collected.</param>
        /// <returns name="Workset">List of worksets that passed the filter.</returns>
        public static IList<Workset> GetAllWorksetsByKind(string kind = "UserWorkset")
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var worksetKind = (Autodesk.Revit.DB.WorksetKind)Enum.Parse(typeof(Autodesk.Revit.DB.WorksetKind), kind);
            var worksets = new Autodesk.Revit.DB.FilteredWorksetCollector(doc)
                .OfKind(worksetKind)
                .Select(x => new Workset(x))
                .ToList();

            return worksets.Any() ? worksets : Enumerable.Empty<Workset>().ToList();
        }

        /// <summary>
        /// Returns all elements at a given workset.
        /// </summary>
        /// <param name="workset">Workset that we are filtering for.</param>
        /// <returns name="Element">Elements that passed the workset filter.</returns>
        public static IList<Element> ByWorkset(Workset workset)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var filter = new Autodesk.Revit.DB.ElementWorksetFilter(workset.InternalWorkset.Id, false);
            var elements = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                .WherePasses(filter)
                .ToElements()
                .Select(x => x.ToDSType(true))
                .ToList();

            return elements.Any() ? elements : Enumerable.Empty<Element>().ToList();
        }

        /// <summary>
        /// Select Element by Id
        /// </summary>
        /// <param name="id">ElementId, String, Guid or Integer id of the element.</param>
        /// <returns name="Element">Found element or null.</returns>
        public static object ByElementId(object id)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            Autodesk.Revit.DB.Element e;

            switch (Type.GetTypeCode(id.GetType()))
            {
                case TypeCode.String:
                    e = ((string)id).Length > 8 ? doc.GetElement((string)id) : doc.GetElement(new Autodesk.Revit.DB.ElementId(int.Parse((string)id)));
                    break;
                case TypeCode.Double:
                    var integerValue = Convert.ToInt32(id);
                    e = doc.GetElement(new Autodesk.Revit.DB.ElementId(integerValue));
                    break;
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Int16:
                    e = doc.GetElement(new Autodesk.Revit.DB.ElementId(int.Parse(id.ToString())));
                    break;
                default:
                    e = doc.GetElement((Autodesk.Revit.DB.ElementId)id);
                    break;
            }
            return e?.ToDSType(true);
        }

        /// <summary>
        /// Select Elements by Level and Category.
        /// </summary>
        /// <param name="category">Category to filter for.</param>
        /// <param name="level">Level to filter for.</param>
        /// <returns name="Element">Elements.</returns>
        public static IList<Element> ByCategoryAndLevel(Category category, Element level)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var bic = (Autodesk.Revit.DB.BuiltInCategory)Enum.ToObject(typeof(Autodesk.Revit.DB.BuiltInCategory), category.Id);

            // create level filter
            var filter = new Autodesk.Revit.DB.ElementLevelFilter(new Autodesk.Revit.DB.ElementId(level.Id));

            // select elements
            var elements = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                .OfCategory(bic)
                .WherePasses(filter)
                .WhereElementIsNotElementType()
                .ToElements()
                .Select(x => x.ToDSType(true))
                .ToList();

            return elements.Any() ? elements : Enumerable.Empty<Element>().ToList();
        }

        /// <summary>
        ///     Select all elements by design option and category.
        /// </summary>
        /// <param name="category">Category to filter for.</param>
        /// <param name="designOption">Design Option to filter for.</param>
        /// <returns name="Element">Elements.</returns>
        public static IList<Element> ByCategoryAndDesignOption(Category category, Element designOption)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var bic = (Autodesk.Revit.DB.BuiltInCategory)Enum.ToObject(typeof(Autodesk.Revit.DB.BuiltInCategory), category.Id);

            // create design option filter
            var filter = new Autodesk.Revit.DB.ElementDesignOptionFilter(new Autodesk.Revit.DB.ElementId(designOption.Id));

            // select elements
            var elements = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                .OfCategory(bic)
                .WherePasses(filter)
                .WhereElementIsNotElementType()
                .ToElements()
                .Select(x => x.ToDSType(true))
                .ToList();

            return elements.Any() ? elements : Enumerable.Empty<Element>().ToList();
        }

        /// <summary>
        /// Selects all Elements of given Type from given Document (Linked Models).
        /// </summary>
        /// <param name="type">Element Type to retrieve.</param>
        /// <param name="document">Optional Document object. If null, current active Document will be used.</param>
        /// <returns name="element">List of Elements.</returns>
        public static IList<Element> ByTypeAndDocument(Type type, [DefaultArgument("Selection.Select.GetNull()")] object document)
        {
            var doc = (document ?? DocumentManager.Instance.CurrentDBDocument) as Autodesk.Revit.DB.Document;
            return new Autodesk.Revit.DB.FilteredElementCollector(doc)
                .OfClass(type)
                .ToElements()
                .Select(x => x.ToDSType(true))
                .ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeName">Fully qualified type name. Refer to: https://docs.microsoft.com/en-us/dotnet/api/system.type.gettype?view=net-5.0 </param>
        /// <param name="document"></param>
        /// <returns></returns>
        public static IList<Element> ByTypeNameAndDocument(string typeName, [DefaultArgument("Selection.Select.GetNull()")] object document)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                throw new ArgumentException(nameof(typeName));

            var fullName = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x.FullName.StartsWith("RevitAPI,"))
                .SelectMany(x => x.GetTypes())
                .FirstOrDefault(x => x.Name == typeName)?
                .AssemblyQualifiedName;

            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException(nameof(typeName));

            var t = Type.GetType($"Autodesk.Revit.DB.{fullName}");
            var doc = (document ?? DocumentManager.Instance.CurrentDBDocument) as Autodesk.Revit.DB.Document;
            return new Autodesk.Revit.DB.FilteredElementCollector(doc)
                .OfClass(t)
                .ToElements()
                .Select(x => x.ToDSType(true))
                .ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public static IList<Element> ByCategory(Category category)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var filter = new Autodesk.Revit.DB.ElementMulticategoryFilter(new List<Autodesk.Revit.DB.ElementId> 
            { new Autodesk.Revit.DB.ElementId(category.Id) });
            
            return new Autodesk.Revit.DB.FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .WherePasses(filter)
                .Select(x => x.ToDSType(true))
                .ToList();
        }

        /// <summary>
        /// Select Elements by Category and View.
        /// </summary>
        /// <param name="category">Category to filter for.</param>
        /// <param name="view">View to filter for.</param>
        /// <returns name="Element">Element.</returns>
        public static List<Element> ByCategoryAndView(Category category, View view)
        {
            var v = (Autodesk.Revit.DB.View)view.InternalElement;
            var bic = (Autodesk.Revit.DB.BuiltInCategory)Enum.ToObject(typeof(Autodesk.Revit.DB.BuiltInCategory), category.Id);
            var doc = DocumentManager.Instance.CurrentDBDocument;

            // select elements
            var elements = (new Autodesk.Revit.DB.FilteredElementCollector(doc, v.Id)
                    .OfCategory(bic)
                    .WhereElementIsNotElementType()
                    .ToElements())
                .Select(x => x.ToDSType(true))
                .ToList();

            return elements.Any() ? elements : Enumerable.Empty<Element>().ToList();
        }

        /// <summary>
        /// Parameter value filter.
        /// </summary>
        /// <param name="elementType"></param>
        /// <param name="parameterName"></param>
        /// <param name="evaluator"></param>
        /// <param name="value"></param>
        /// <param name="rule"></param>
        /// <returns name="Element">Element.</returns>
        public static List<Element> ByParameterValue(
            Type elementType,
            string parameterName,
            object evaluator,
            object value,
            [DefaultArgument("Selection.Select.GetNull()")] string rule)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var numericRule = FilterNumericValueRule.DoubleRule;
            if (rule != null)
            {
                numericRule = (FilterNumericValueRule)Enum.Parse(typeof(FilterNumericValueRule), rule);
            }

            var e = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                .OfClass(elementType)
                .WhereElementIsNotElementType()
                .First();

            // get parameter value provider
            Autodesk.Revit.DB.ParameterValueProvider pvp;
            try
            {
                if (Enum.TryParse(parameterName, out Autodesk.Revit.DB.BuiltInParameter bip))
                {
                    // process as bip
                    pvp = new Autodesk.Revit.DB.ParameterValueProvider(new Autodesk.Revit.DB.ElementId((int)bip));
                }
                else
                {
                    // process as regular param
                    var p = e.LookupParameter(parameterName);
                    pvp = new Autodesk.Revit.DB.ParameterValueProvider(p.Id);
                }
            }
            catch (Exception)
            {
                throw new ArgumentException("parameterName");
            }


            // get filter rule
            Autodesk.Revit.DB.FilterRule filterRule;
            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.String:
                    var fsre = (Autodesk.Revit.DB.FilterStringRuleEvaluator)evaluator;
                    filterRule = new Autodesk.Revit.DB.FilterStringRule(pvp, fsre, (string)value, true);
                    break;
                default:
                    var fnre = (Autodesk.Revit.DB.FilterNumericRuleEvaluator)evaluator;
                    switch (numericRule)
                    {
                        case FilterNumericValueRule.DoubleRule:
                            filterRule = new Autodesk.Revit.DB.FilterDoubleRule(pvp, fnre, (double)value, 0.001);
                            break;
                        case FilterNumericValueRule.ElementIdRule:
                            var id = ((Element)value).InternalElement.Id;
                            filterRule = new Autodesk.Revit.DB.FilterElementIdRule(pvp, fnre, id);
                            break;
                        case FilterNumericValueRule.IntegerRule:
                            filterRule = new Autodesk.Revit.DB.FilterIntegerRule(pvp, fnre, (int)value);
                            break;
                        default:
                            throw new ArgumentException("rule");
                    }
                    break;
            }

            // create parameter filter
            var epf = new Autodesk.Revit.DB.ElementParameterFilter(filterRule);

            // return collection
            var output = new Autodesk.Revit.DB.FilteredElementCollector(doc).OfClass(elementType).WherePasses(epf).ToElements();
            return output.Any() ? output.Select(x => x.ToDSType(true)).ToList() : Enumerable.Empty<Element>().ToList();
        }

        /// <summary>
        /// Get Null
        /// </summary>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static object GetNull()
        {
            return null;
        }
    }
}
