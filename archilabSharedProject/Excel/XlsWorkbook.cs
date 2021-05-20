using System;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;

namespace archilab.Excel
{
    /// <summary>
    /// Workbook Wrapper
    /// </summary>
    public class Workbook
    {
        #region Internal Methods

        internal string FilePath { get; private set; }
        internal XLWorkbook InternalWorkbook { get; private set; }
        internal Dictionary<Tuple<int, int>, object> Data { get; set; }

        private Workbook(string filePath)
        {
            InternalSetWorkbook(filePath);
        }

        private void InternalSetWorkbook(string filePath)
        {
            using (var wb = new XLWorkbook(filePath))
            {
                InternalWorkbook = wb;
            }
            FilePath = filePath;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static Workbook ByFilePath(object filePath)
        {
            var cleanFilePath = Utilities.FilePathUtilities.VerifyFilePath(filePath);
            return new Workbook(cleanFilePath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="namedRange"></param>
        /// <param name="worksheet"></param>
        /// <returns></returns>
        public List<List<object>> GetNamedRangeValues(NamedRange namedRange, Worksheet worksheet)
        {
            var ranges = namedRange.InternalNamedRange.Ranges;
            var output = new List<List<object>>();

            foreach (var r in ranges)
            {
                var rowNumber = r.RangeAddress.FirstAddress.RowNumber;
                for (var i = rowNumber; i < rowNumber + r.RowCount(); i++)
                {
                    var rowValues = new List<object>();
                    var colNumber = r.RangeAddress.FirstAddress.ColumnNumber;

                    for (var j = colNumber; j < colNumber + r.ColumnCount(); j++)
                    {
                        rowValues.Add(worksheet.Data[new Tuple<int, int>(i, j)]);
                    }
                    output.Add(rowValues);
                }
            }
            return output;
        }

        #endregion

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<NamedRange> NamedRanges()
        {
            return InternalWorkbook.NamedRanges.Select(x => new NamedRange(x)).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Worksheet> Worksheets()
        {
            return InternalWorkbook.Worksheets.Select(x => new Worksheet(this, x.Name)).ToList();
        }

        #endregion

        #region Overrides

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Workbook: FilePath={(InternalWorkbook != null ? FilePath : string.Empty)}";
        }

        #endregion
    }
}
