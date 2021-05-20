using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using OfficeOpenXml;
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
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="runIt"></param>
        /// <param name="worksheet"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool WriteByArray(string filePath, bool runIt, string worksheet, List<object[]> data)
        {
            if (!runIt)
                return false;

            try
            {
                using (var ep = new ExcelPackage(new FileInfo(filePath)))
                {
                    var w = ep.Workbook.Worksheets.FirstOrDefault(x => x.Name == worksheet)
                            ?? ep.Workbook.Worksheets.Add(worksheet);
                    w.Cells["A1"].LoadFromArrays(data);
                    ep.Save();
                }

                if (File.Exists(filePath))
                    Process.Start(filePath);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
