using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HtmlAgilityPack;
using RevitServices.Persistence;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local

namespace archilab.Revit.Elements
{
    /// <summary>
    /// Warning class used for parsing HTML Revit warnings.
    /// </summary>
    public class Warning
    {
        private string _assignedErrorMessage;

        /// <summary>
        /// Custom Error Message
        /// </summary>
        public string AssignedErrorMessage
        {
            get
            {
                return string.IsNullOrEmpty(_assignedErrorMessage) ? ErrorMessage : _assignedErrorMessage;
            }
            set
            {
                _assignedErrorMessage = value;
            }
        }

        /// <summary>
        /// Error Message
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// List of Elements
        /// </summary>
        public List<WarningElement> Elements { get; set; }

        /// <summary>
        /// Rating of severity of warning where 1 is ignorable and 3 is severe.
        /// </summary>
        public int Rating { get; set; }

        internal Warning()
        {
        }

        private Warning(Autodesk.Revit.DB.FailureMessage failure)
        {
            ErrorMessage = failure.GetDescriptionText();

            var elementIds = failure.GetFailingElements();
            var additionalElementIds = failure.GetAdditionalElements();

            var allIds = new List<Autodesk.Revit.DB.ElementId>();
            allIds.AddRange(elementIds);
            allIds.AddRange(additionalElementIds);

            if (!allIds.Any())
                return;

            var doc = DocumentManager.Instance.CurrentDBDocument;
            var warningElements = new List<WarningElement>();
            foreach (var id in allIds)
            {
                var e = doc.GetElement(id);
                if (e == null)
                    continue;

                var we = new WarningElement {Id = id.IntegerValue};
                warningElements.Add(we);
            }

            Elements = warningElements;
        }

#if !Revit2017
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List<Warning> GetWarnings()
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var warnings = doc.GetWarnings();

            return !warnings.Any() 
                ? new List<Warning>() 
                : warnings.Select(x => new Warning(x)).ToList();
        }
#endif

        /// <summary>
        /// Assigns a rating to warning based on custom library of ratings.
        /// </summary>
        /// <param name="library">Custom library of Warnings and ratings.</param>
        /// <returns name="Warning">Warning</returns>
        public Warning AssignRating(WarningLibrary library)
        {
            if (library.Ratings.ContainsKey(ErrorMessage))
            {
                Rating = library.Ratings[ErrorMessage];
            }
            else if (ErrorMessage.Contains("Design Option"))
            {
                Rating = 3;
            }
            return this;
        }

        /// <summary>
        /// Assign a custom error message to warning.
        /// </summary>
        /// <param name="message">Custom warning message.</param>
        /// <returns name="Warning">Warning that custom message was assigned to.</returns>
        public Warning AssignErrorMessage(string message)
        {
            AssignedErrorMessage = message;
            return this;
        }

        /// <summary>
        /// Parse HTML Warning Report.
        /// </summary>
        /// <param name="filePath">File Path to HTML report.</param>
        /// <returns name="Warnings">List of Warning objects.</returns>
        public static List<Warning> ParseHtml(object filePath)
        {
            var cleanFilePath = Utilities.FilePathUtilities.VerifyFilePath(filePath);

            // read html string
            var html = File.ReadAllText(cleanFilePath);

            // create a doc
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var warnings = new List<Warning>();
            foreach (var row in htmlDoc.DocumentNode.SelectNodes("//table/tr"))
            {
                var w = new Warning();
                var columns = row.SelectNodes("td");
                if (columns.Count > 1)
                {
                    // assign error message
                    w.ErrorMessage = columns[0].InnerText.Trim();

                    // generate elements
                    var elements = new List<WarningElement>();
                    var elementLines = columns[1].InnerText.Trim().Replace("\\\"", "").Split(new[] { "   " }, StringSplitOptions.None);
                    foreach (var e in elementLines)
                    {
                        var toProcess = e.Replace(": :", ":");
                        var contents = toProcess.Split(new[] { " : " }, StringSplitOptions.None);
                        var ew = new WarningElement();
                        switch (contents.Count())
                        {
                            // model lines don't have category and family
                            // well they do but it gets swallowed by the html parser due to use of <>
                            case 3:
                                ew = CreateElement(new[] { 0, -1, -1, 1, 2, -1, -1 }, contents);
                                break;
                            // system elements that are hosted ex: Wall Opening
                            case 4:
                                ew = CreateElement(new[] { 0, -1, 1, 2, 3, -1, -1 }, contents);
                                break;
                            // typical elements
                            case 5:
                                ew = CreateElement(new[] { 0, 1, 2, 3, 4, -1, -1 }, contents);
                                break;
                            // rebar for example has a 5th argument that is "Shape 2A". I am not sure what that is yet.
                            case 6:
                                ew = CreateElement(new[] {0, 1, 2, 3, 5, -1, -1}, contents);
                                break;
                            // elements with design options
                            case 7:
                                ew = CreateElement(new[] { 2, 3, 4, 5, 6, 0, 1 }, contents);
                                break;
                        }
                        elements.Add(ew);
                    }
                    w.Elements = elements;
                }
                warnings.Add(w);
            }

            return warnings;
        }

        private static WarningElement CreateElement(int[] pointers, string[] contents)
        {
            var ew = new WarningElement();
            try { ew.Workset = contents[pointers[0]].Trim(); } catch { ew.Workset = ""; }
            try { ew.Category = contents[pointers[1]].Trim(); } catch { ew.Category = ""; }
            try { ew.Family = contents[pointers[2]].Trim(); } catch { ew.Family = ""; }
            try { ew.Type = contents[pointers[3]].Trim(); } catch { ew.Type = ""; }
            try { ew.Id = int.Parse(contents[pointers[4]].Trim().Remove(0, 3)); } catch { ew.Id = -1; }
            try { ew.DesignOptionSet = contents[pointers[5]].Trim().TrimStart('('); } catch { ew.DesignOptionSet = ""; }
            try { ew.DesignOption = contents[pointers[6]].Trim().TrimEnd(')'); } catch { ew.DesignOption = ""; }
            return ew;
        }
    }

    /// <summary>
    /// Element Wrapper class for elements associated with Warnings.
    /// </summary>
    public class WarningElement
    {
        internal WarningElement()
        {
        }

        /// <summary>
        /// Workset Name
        /// </summary>
        public string Workset { get; set; }

        /// <summary>
        /// Category Name
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Family Name
        /// </summary>
        public string Family { get; set; }

        /// <summary>
        /// Type Name
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Revit Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Design Option Set Name
        /// </summary>
        public string DesignOptionSet { get; set; }

        /// <summary>
        /// Design Option Name
        /// </summary>
        public string DesignOption { get; set; }

        /// <summary>
        /// Warning rating on scale from 1-3 where 1 is ignorable and 3 is severe.
        /// </summary>
        public int Rating { get; set; }
    }
}
