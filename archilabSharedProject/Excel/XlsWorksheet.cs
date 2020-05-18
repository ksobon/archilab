using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;
using ClosedXML.Excel;

namespace archilab.Excel
{
    /// <summary>
    /// Workbook Wrapper
    /// </summary>
    public class Worksheet
    {
        #region Internal Methods

        internal IXLWorksheet InternalWorksheet { get; private set; }
        internal Dictionary<Tuple<int, int>, object> Data { get; set; }

        private void InternalSetWorksheet(Workbook workbook, string worksheetName)
        {
            var data = new Dictionary<Tuple<int, int>, object>();
            using (var wb = new XLWorkbook(workbook.FilePath))
            {
                var ws = wb.Worksheet(worksheetName);

                if (ws == null) throw new Exception("Worksheet with given name was not found.");
                InternalWorksheet = ws;

                var r = ws.RangeUsed();
                var rowNumber = r.RangeAddress.FirstAddress.RowNumber;

                for (var i = rowNumber; i < rowNumber + r.RowCount() + 1; i++)
                {
                    var colNumber = r.RangeAddress.FirstAddress.ColumnNumber;

                    for (var j = colNumber; j < colNumber + r.ColumnCount() + 1; j++)
                    {
                        data.Add(new Tuple<int, int>(i, j), ws.Cell(i, j).GetFormattedString());
                    }
                }
            }
            Data = data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workbook"></param>
        /// <param name="worksheetName"></param>
        [IsVisibleInDynamoLibrary(false)]
        public Worksheet(Workbook workbook, string worksheetName)
        {
            InternalSetWorksheet(workbook, worksheetName);
        }

        #endregion

        #region Public Methods



        #endregion

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public string Name
        {
            get { return InternalWorksheet.Name; }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Worksheet: Name={(InternalWorksheet != null ? Name : string.Empty)}";
        }

        #endregion
    }
}
