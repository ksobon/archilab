using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Graph.Nodes;
using Revit.Elements;
using RevitServices.Persistence;
// ReSharper disable UnusedMember.Global

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class FilterRule
    {
        internal Autodesk.Revit.DB.FilterRule InternalFilterRule { get; set; }

        internal FilterRule()
        {
        }

        internal FilterRule(Autodesk.Revit.DB.FilterRule fr)
        {
            InternalFilterRule = fr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="categories"></param>
        /// <param name="parameterName"></param>
        /// <param name="ruleType"></param>
        /// <param name="parameterValue"></param>
        /// <returns></returns>
        [NodeCategory("Action")]
        public static FilterRule CreateRule(List<Category> categories, string parameterName, string ruleType, object parameterValue)
        {
            if (!categories.All(x => ParameterExists(x, parameterName)))
                throw new Exception("Parameter with given name doesn't exist for all Categories.");

            var bic = (Autodesk.Revit.DB.BuiltInCategory)Enum.ToObject(typeof(Autodesk.Revit.DB.BuiltInCategory),
                categories.First().Id);
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var elementInstance = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                .OfCategory(bic)
                .WhereElementIsNotElementType()
                .FirstElement();
            var elementType = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                .OfCategory(bic)
                .WhereElementIsElementType()
                .FirstElement();

            Autodesk.Revit.DB.ElementId elementId;
            Autodesk.Revit.DB.StorageType storageType;

            if (Enum.TryParse<Autodesk.Revit.DB.BuiltInParameter>(parameterName, out var bip))
            {
                if (elementInstance.get_Parameter(bip) != null)
                {
                    elementId = new Autodesk.Revit.DB.ElementId(bip);
                    storageType = elementInstance.get_Parameter(bip).StorageType;
                }
                else
                {
                    elementId = new Autodesk.Revit.DB.ElementId(bip);
                    storageType = elementType.get_Parameter(bip).StorageType;
                }
            }
            else if (elementInstance.LookupParameter(parameterName) != null)
            {
                var parameter = elementInstance.LookupParameter(parameterName);
                elementId = parameter.Id;
                storageType = parameter.StorageType;
            }
            else
            {
                var parameter = elementType.LookupParameter(parameterName);
                elementId = parameter.Id;
                storageType = parameter.StorageType;
            }

            switch (ruleType)
            {
                case "equals":
                    switch (storageType)
                    {
                        case Autodesk.Revit.DB.StorageType.Integer:
                            return new FilterRule(
                                Autodesk.Revit.DB.ParameterFilterRuleFactory.CreateEqualsRule(elementId,
                                    Convert.ToInt32(parameterValue)));
                        case Autodesk.Revit.DB.StorageType.Double:
                            return new FilterRule(
                                Autodesk.Revit.DB.ParameterFilterRuleFactory.CreateEqualsRule(elementId,
                                    Convert.ToDouble(parameterValue), 0.001));
                        case Autodesk.Revit.DB.StorageType.String:
                            return new FilterRule(
                                Autodesk.Revit.DB.ParameterFilterRuleFactory.CreateEqualsRule(elementId,
                                    parameterValue.ToString(), true));
                        case Autodesk.Revit.DB.StorageType.ElementId:
                            return new FilterRule(Autodesk.Revit.DB.ParameterFilterRuleFactory.CreateEqualsRule(
                                elementId, new Autodesk.Revit.DB.ElementId(((Element)parameterValue).Id)));
                        default:
                            return null;
                    }
                case "does not equal":
                    switch (storageType)
                    {
                        case Autodesk.Revit.DB.StorageType.Integer:
                            return new FilterRule(
                                Autodesk.Revit.DB.ParameterFilterRuleFactory.CreateNotEqualsRule(elementId,
                                    Convert.ToInt32(parameterValue)));
                        case Autodesk.Revit.DB.StorageType.Double:
                            return new FilterRule(
                                Autodesk.Revit.DB.ParameterFilterRuleFactory.CreateNotEqualsRule(elementId,
                                    Convert.ToDouble(parameterValue), 0.001));
                        case Autodesk.Revit.DB.StorageType.String:
                            return new FilterRule(
                                Autodesk.Revit.DB.ParameterFilterRuleFactory.CreateNotEqualsRule(elementId,
                                    parameterValue.ToString(), true));
                        case Autodesk.Revit.DB.StorageType.ElementId:
                            return new FilterRule(Autodesk.Revit.DB.ParameterFilterRuleFactory.CreateNotEqualsRule(
                                elementId, new Autodesk.Revit.DB.ElementId(((Element)parameterValue).Id)));
                        default:
                            return null;
                    }
                case "is greater than":
                    switch (storageType)
                    {
                        case Autodesk.Revit.DB.StorageType.Integer:
                            return new FilterRule(
                                Autodesk.Revit.DB.ParameterFilterRuleFactory.CreateGreaterRule(elementId,
                                    Convert.ToInt32(parameterValue)));
                        case Autodesk.Revit.DB.StorageType.Double:
                            return new FilterRule(
                                Autodesk.Revit.DB.ParameterFilterRuleFactory.CreateGreaterRule(elementId,
                                    Convert.ToDouble(parameterValue), 0.001));
                        case Autodesk.Revit.DB.StorageType.String:
                            return new FilterRule(
                                Autodesk.Revit.DB.ParameterFilterRuleFactory.CreateGreaterRule(elementId,
                                    parameterValue.ToString(), true));
                        case Autodesk.Revit.DB.StorageType.ElementId:
                            return new FilterRule(Autodesk.Revit.DB.ParameterFilterRuleFactory.CreateGreaterRule(
                                elementId, new Autodesk.Revit.DB.ElementId(((Element)parameterValue).Id)));
                        default:
                            return null;
                    }
                case "is greater than or equal to":
                    switch (storageType)
                    {
                        case Autodesk.Revit.DB.StorageType.Integer:
                            return new FilterRule(
                                Autodesk.Revit.DB.ParameterFilterRuleFactory.CreateGreaterOrEqualRule(elementId,
                                    Convert.ToInt32(parameterValue)));
                        case Autodesk.Revit.DB.StorageType.Double:
                            return new FilterRule(
                                Autodesk.Revit.DB.ParameterFilterRuleFactory.CreateGreaterOrEqualRule(elementId,
                                    Convert.ToDouble(parameterValue), 0.001));
                        case Autodesk.Revit.DB.StorageType.String:
                            return new FilterRule(
                                Autodesk.Revit.DB.ParameterFilterRuleFactory.CreateGreaterOrEqualRule(elementId,
                                    parameterValue.ToString(), true));
                        case Autodesk.Revit.DB.StorageType.ElementId:
                            return new FilterRule(Autodesk.Revit.DB.ParameterFilterRuleFactory.CreateGreaterOrEqualRule(
                                elementId, new Autodesk.Revit.DB.ElementId(((Element)parameterValue).Id)));
                        default:
                            return null;
                    }
                case "is less than":
                    return new FilterRule(
                        Autodesk.Revit.DB.ParameterFilterRuleFactory.CreateLessRule(elementId,
                            Convert.ToInt32(parameterValue)));
                case "is less than or equal to":
                    switch (storageType)
                    {
                        case Autodesk.Revit.DB.StorageType.Integer:
                            return new FilterRule(
                                Autodesk.Revit.DB.ParameterFilterRuleFactory.CreateLessOrEqualRule(elementId,
                                    Convert.ToInt32(parameterValue)));
                        case Autodesk.Revit.DB.StorageType.Double:
                            return new FilterRule(
                                Autodesk.Revit.DB.ParameterFilterRuleFactory.CreateLessOrEqualRule(elementId,
                                    Convert.ToDouble(parameterValue), 0.001));
                        case Autodesk.Revit.DB.StorageType.String:
                            return new FilterRule(
                                Autodesk.Revit.DB.ParameterFilterRuleFactory.CreateLessOrEqualRule(elementId,
                                    parameterValue.ToString(), true));
                        case Autodesk.Revit.DB.StorageType.ElementId:
                            return new FilterRule(Autodesk.Revit.DB.ParameterFilterRuleFactory.CreateLessOrEqualRule(
                                elementId, new Autodesk.Revit.DB.ElementId(((Element)parameterValue).Id)));
                        default:
                            return null;
                    }
                case "contains":
                    return new FilterRule(
                        Autodesk.Revit.DB.ParameterFilterRuleFactory.CreateContainsRule(elementId,
                            parameterValue.ToString(), true));
                case "does not contain":
                    return new FilterRule(
                        Autodesk.Revit.DB.ParameterFilterRuleFactory.CreateNotContainsRule(elementId,
                            parameterValue.ToString(), true));
                case "begins with":
                    return new FilterRule(Autodesk.Revit.DB.ParameterFilterRuleFactory.CreateBeginsWithRule(elementId,
                        parameterValue.ToString(), true));
                case "does not begin with":
                    return new FilterRule(
                        Autodesk.Revit.DB.ParameterFilterRuleFactory.CreateNotBeginsWithRule(elementId,
                            parameterValue.ToString(), true));
                case "ends with":
                    return new FilterRule(
                        Autodesk.Revit.DB.ParameterFilterRuleFactory.CreateEndsWithRule(elementId,
                            parameterValue.ToString(), true));
                case "does not end with":
                    return new FilterRule(
                        Autodesk.Revit.DB.ParameterFilterRuleFactory.CreateNotEndsWithRule(elementId,
                            parameterValue.ToString(), true));
#if !Revit2017 && !Revit2018 && !Revit2019
                case "has a value":
                    return new FilterRule(Autodesk.Revit.DB.ParameterFilterRuleFactory.CreateHasValueParameterRule(elementId));
                case "has no value":
                    return new FilterRule(Autodesk.Revit.DB.ParameterFilterRuleFactory.CreateHasNoValueParameterRule(elementId));
#endif
                default:
                    return null;
            }
        }

#region Utilities

        private static bool ParameterExists(Category category, string parameterName)
        {
            var bic = (Autodesk.Revit.DB.BuiltInCategory)Enum.ToObject(typeof(Autodesk.Revit.DB.BuiltInCategory), category.Id);
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var elementInstance = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                .OfCategory(bic)
                .WhereElementIsNotElementType()
                .FirstElement();
            var elementType = new Autodesk.Revit.DB.FilteredElementCollector(doc)
                .OfCategory(bic)
                .WhereElementIsElementType()
                .FirstElement();
            var flag = false;

            if (Enum.TryParse<Autodesk.Revit.DB.BuiltInParameter>(parameterName, out var bip))
            {
                if (elementInstance.get_Parameter(bip) != null)
                    flag = true;
                else if (elementType.get_Parameter(bip) != null)
                    flag = true;
            }
            else if (elementInstance.LookupParameter(parameterName) != null)
                flag = true;
            else if (elementType.LookupParameter(parameterName) != null)
                flag = true;
            return flag;
        }

#endregion
    }
}
