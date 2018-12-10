﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using RevitServices.Persistence;
using Workset = archilab.Revit.Elements.Workset;

namespace archilab.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public static class FilePathUtilities
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string VerifyFilePath(object filePath)
        {
            string filePath2;
            try
            {
                filePath2 = (string)filePath;
            }
            catch
            {
                filePath2 = ((FileInfo)filePath).FullName;
            }

            // created unescaped file path removes %20 from path etc.
            var finalFilePath = filePath2;

            var uri = new Uri(filePath2);
            var absoluteFilePath = Uri.UnescapeDataString(uri.AbsoluteUri);

            if (!Uri.IsWellFormedUriString(absoluteFilePath, UriKind.RelativeOrAbsolute)) return finalFilePath;
            var newUri = new Uri(absoluteFilePath);
            finalFilePath = newUri.LocalPath;

            return finalFilePath;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class ListUtilities
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputList"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static List<object> DropdownToList(List<object> inputList)
        {
            return inputList;
        }
    }

    /// <summary>
    /// Utilities for retrieving elements
    /// </summary>
    public static class ElementSelector
    {
        /// <summary>
        /// Workset selector.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [IsVisibleInDynamoLibrary(false)]
        public static Workset GetWorksetById(int id)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;

            var wid = new Autodesk.Revit.DB.WorksetId(id);
            var workset = Autodesk.Revit.DB.WorksetId.InvalidWorksetId == wid
                ? null
                : doc.GetWorksetTable().GetWorkset(wid);

            return new Workset(workset);
        }
    }

    /// <summary>
    /// Parameter Filter Numeric Rules.
    /// </summary>
    [SupressImportIntoVM]
    public enum FilterNumericValueRule
    {
        /// <summary>
        /// 
        /// </summary>
        DoubleRule,
        /// <summary>
        /// 
        /// </summary>
        ElementIdRule,
        /// <summary>
        /// 
        /// </summary>
        GlobalParameterRule,
        /// <summary>
        /// 
        /// </summary>
        IntegerRule
    }

    /// <summary>
    /// Built in View Template parameters.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class ViewTemplateParameters
    {
        /// <summary>
        /// Dictionary holding all parameter key, value mappings.
        /// </summary>
        public Dictionary<string, int> Parameters { get; set; }

        /// <summary>
        /// Public constructor.
        /// </summary>
        public ViewTemplateParameters()
        {
            Parameters = new Dictionary<string, int>
            {
                { "View Scale", -1005151 },
                { "View Scale2", -1005152 },
                { "View Scale0", -1005150 },
                { "Display Model", -1005161 },
                { "Parts Visibility", -1011003 },
                { "Detail Level", -1011002 },
                { "VG Overrides Model", -1006961 },
                { "VG Overrides Annotation", -1006962 },
                { "VG Overrides Analytical Model", -1006967 },
                { "VG Overrides Imports", -1006963 },
                { "VG Overrides Filters", -1006964 },
                { "VG Overrides Revit Links", -1006965 },
                { "VG Overrides Design Options", -1006966 },
                { "VG Overrides Worksets", -1006968 },
                { "Model Display", -1005131 },
                { "Shadows", -1005132 },
                { "Sketchy Lines", -1154615 },
                { "Lighting", -1005133 },
                { "Photographic Exposure", -1005137 },
                { "Underlay Orientation", -1005177 },
                { "View Range", -1005162 },
                { "Orientation", -1005168 },
                { "Phase Filter", -1012103 },
                { "Discipline", -1005163 },
                { "Show Hidden Lines", -1154613 },
                { "Color Scheme Location", -1005183 },
                { "Color Scheme", -1005148 },
                { "System Color Scheme", -1133900 },
                { "Depth Clipping", -1005181 }
            };
        }

        /// <summary>
        /// Retrieves View Template parameter by its name.
        /// </summary>
        /// <param name="name">Name of the built in parameter.</param>
        /// <returns>Parameter ElementId</returns>
        public static int ByName(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("name");

            return new ViewTemplateParameters().Parameters[name];
        }

        /// <summary>
        /// Retrieves the name of the View Template parameter by its value.
        /// </summary>
        /// <param name="value">Value of the parameter.</param>
        /// <returns>Name of the parameter.</returns>
        public static string GetName(int value)
        {
            if(value == -1) throw new ArgumentException("name");

            return new ViewTemplateParameters().Parameters.FirstOrDefault(x => x.Value == value).Key;
        }
    }

    /// <summary>
    /// Parameter Filter String Rules.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class FilterStringRuleEvaluator
    {
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, Autodesk.Revit.DB.FilterStringRuleEvaluator> Rules { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public FilterStringRuleEvaluator()
        {
            Rules = new Dictionary<string, Autodesk.Revit.DB.FilterStringRuleEvaluator>
            {
                { "Begins With", new Autodesk.Revit.DB.FilterStringBeginsWith()},
                { "Contains", new Autodesk.Revit.DB.FilterStringContains()},
                { "Ends With", new Autodesk.Revit.DB.FilterStringEndsWith()},
                { "Equals", new Autodesk.Revit.DB.FilterStringEquals()},
                { "Greater", new Autodesk.Revit.DB.FilterStringGreater()},
                { "Greater or Equal", new Autodesk.Revit.DB.FilterStringGreaterOrEqual()},
                { "Less", new Autodesk.Revit.DB.FilterStringLess()},
                { "Less or Equal", new Autodesk.Revit.DB.FilterStringLessOrEqual()}
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Autodesk.Revit.DB.FilterStringRuleEvaluator ByName(string name)
        {
            if (name == null)
            {
                throw new ArgumentException("name");
            }
            var evaluator = new FilterStringRuleEvaluator().Rules[name];
            return evaluator;
        }
    }

    /// <summary>
    ///     Parameter Filter Numeric Rule Evaluator options.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class FilterNumericRuleEvaluator
    {
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, Autodesk.Revit.DB.FilterNumericRuleEvaluator> Rules { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public FilterNumericRuleEvaluator()
        {
            Rules = new Dictionary<string, Autodesk.Revit.DB.FilterNumericRuleEvaluator>
            {
                { "Equals", new Autodesk.Revit.DB.FilterNumericEquals()},
                { "Greater Than", new Autodesk.Revit.DB.FilterNumericGreater()},
                { "Greater or Equal", new Autodesk.Revit.DB.FilterNumericGreaterOrEqual()},
                { "Less", new Autodesk.Revit.DB.FilterNumericLess()},
                { "Less or Equal", new Autodesk.Revit.DB.FilterNumericLessOrEqual()}
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Autodesk.Revit.DB.FilterNumericRuleEvaluator ByName(string name)
        {
            if (name == null)
            {
                throw new ArgumentException("name");
            }
            var evaluator = new FilterNumericRuleEvaluator().Rules[name];
            return evaluator;
        }
    }

    /// <summary>
    ///     View Types enumeration for View.ByType node.
    /// </summary>
    [IsVisibleInDynamoLibrary(false)]
    public class ViewTypes
    {
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, string> Types { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ViewTypes()
        {
            var dict = Enum.GetValues(typeof(Autodesk.Revit.DB.ViewType))
                .Cast<Autodesk.Revit.DB.ViewType>()
                .ToDictionary(t => t.ToString(), t => t.ToString());

            dict.Add("View Template", "View Template");

            Types = dict;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string ByName(string name)
        {
            if (name == null)
            {
                throw new ArgumentException("name");
            }
            var viewType = new ViewTypes().Types[name];
            return viewType;
        }
    }
}
