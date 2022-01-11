using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;
using OfficeOpenXml;
using OfficeOpenXml.Style;
// ReSharper disable UnusedMember.Global

namespace archilab.Excel
{
    /// <summary>
    /// 
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class EPPlus
    {
        internal EPPlus()
        {
        }

        /// <summary>
        /// Merges ranges of cells into a single cell, setting the value and centering the merged cell.
        /// </summary>
        /// <param name="worksheet">Worksheet name. If doesn't exist, new worksheet will be created.</param>
        /// <param name="range">Range to be merged. Accepted format is 'A1:D1'.</param>
        /// <param name="value">Value to be set for merged cells.</param>
        /// <returns name="action">An Excel action that will merge cells.</returns>
        [NodeCategory("Action")]
        public static MergeCells MergeCells(string worksheet, string range, object value)
        {
            return new MergeCells(worksheet, range, value);
        }

        /// <summary>
        /// Use this action to write data to Excel by array.
        /// </summary>
        /// <param name="worksheet">Worksheet name. If doesn't exist, new worksheet will be created.</param>
        /// <param name="data">An array of data to be written into Excel. Make sure no null values exist.</param>
        /// <param name="origin">An origin at which to start writing data e.g. A1.</param>
        /// <returns name="action">An Excel action that will write data into Excel.</returns>
        [NodeCategory("Action")]
        public static WriteByArray WriteByArray(string worksheet, List<object[]> data, string origin = "A1")
        {
            return new WriteByArray(worksheet, origin, data);
        }


        /// <summary>
        /// Use this action to open Excel file.
        /// </summary>
        /// <param name="filePath">File path to Excel file that will be open.</param>
        /// <returns name="action">An Excel action that will open the Excel File when processing is completed.</returns>
        [NodeCategory("Action")]
        public static OpenFile OpenFile(string filePath)
        {
            return new OpenFile(filePath);
        }

        /// <summary>
        /// This node will execute all Excel actions. 
        /// </summary>
        /// <param name="filePath">File to Excel file. If file doesn't exist at this path, new file will be created.</param>
        /// <param name="actions">List of Excel Actions to be executed. They will be executed in order that they are supplied.</param>
        [NodeCategory("Action")]
        public static void ExecuteExcelActions(string filePath, List<object> actions)
        {
            OpenFile openFileAction = null;

            using (var ep = new ExcelPackage(new FileInfo(filePath)))
            {
                foreach (ExcelAction ea in actions)
                {
                    if (ea is OpenFile ofa)
                    {
                        openFileAction = ofa;
                        continue;
                    }

                    var ws = ep.Workbook.Worksheets.FirstOrDefault(x => x.Name == ea.Worksheet) ??
                             ep.Workbook.Worksheets.Add(ea.Worksheet);

                    switch (ea)
                    {
                        case MergeCells mergeCells:
                            var mergeRange = ws.Cells[mergeCells.Range];
                            mergeRange.Merge = true;
                            mergeRange.Value = mergeCells.Value;
                            mergeRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            break;
                        case WriteByArray writeByArray:
                            ws.Cells[writeByArray.Origin].LoadFromArrays(writeByArray.Data);
                            ws.Cells[ws.Dimension.Address].AutoFitColumns();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(ea));
                    }
                }

                ep.Save(); 
            }

            if (openFileAction != null && File.Exists(openFileAction.FilePath))
                Process.Start(openFileAction.FilePath);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [SupressImportIntoVM]
    public class ExcelAction
    {
        /// <summary>
        /// 
        /// </summary>
        public string Worksheet { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [SupressImportIntoVM]
    public class MergeCells : ExcelAction
    {
        /// <summary>
        /// 
        /// </summary>
        public string Range { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="range"></param>
        /// <param name="value"></param>
        public MergeCells(string ws, string range, object value)
        {
            Worksheet = ws;
            Range = range;
            Value = value;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [SupressImportIntoVM]
    public class WriteByArray : ExcelAction
    {
        /// <summary>
        /// 
        /// </summary>
        public List<object[]> Data { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Origin { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="origin"></param>
        /// <param name="data"></param>
        public WriteByArray(string ws, string origin, List<object[]> data)
        {
            Worksheet = ws;
            Origin = origin;
            Data = data;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [SupressImportIntoVM]
    public class OpenFile : ExcelAction
    {
        /// <summary>
        /// 
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        public OpenFile(string filePath)
        {
            FilePath = filePath;
        }
    }
}
