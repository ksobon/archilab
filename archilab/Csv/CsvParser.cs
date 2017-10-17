using System;
using System.Collections.Generic;
using System.IO;
using LumenWorks.Framework.IO.Csv;

namespace archilab.Csv
{
    /// <summary>
    /// CSV Parsing library.
    /// </summary>
    public class Csv
    {
        #region Internal Constructors

        internal CsvReader InternalCsv { get; private set; }
        internal Dictionary<Tuple<int, int>, object> Data { get; set; }
        internal int Rows { get; private set; }

        private Csv(string filePath)
        {
            InternalSetCsvReader(filePath);
        }

        private void InternalSetCsvReader(string filePath)
        {
            var count = 0;
            var data = new Dictionary<Tuple<int, int>, object>();
            using (var csv = new CsvReader(new StreamReader(filePath), true))
            {
                InternalCsv = csv;

                var fieldCount = csv.FieldCount;
                while (csv.ReadNextRecord())
                {
                    count++;
                    for (var i = 0; i < fieldCount; i++)
                    {
                        data.Add(new Tuple<int, int>((int)csv.CurrentRecordIndex, i), csv[i]);
                    }
                }
            }
            Data = data;
            Rows = count;
        }

        #endregion

        /// <summary>
        /// Get values by header name. Use Headers component to get all the names.
        /// </summary>
        /// <param name="header">Name of the header.</param>
        /// <returns></returns>
        public List<object> ValuesByHeaderName(string header)
        {
            var output = new List<object>();
            var headerIndex = InternalCsv.GetFieldIndex(header);
            for (var i = 0; i < RowCount; i++)
            {
                output.Add(Data[new Tuple<int, int>(i, headerIndex)]);
            }
            return output;
        }

        /// <summary>
        /// Get values by header index. You can use ColumnCount to see how many are available or simply grab one row and inspect it.
        /// </summary>
        /// <param name="header">Index of the header to be retrieved.</param>
        /// <returns></returns>
        public List<object> ValuesByHeaderNumber(int header)
        {
            var output = new List<object>();
            for (var i = 0; i < RowCount; i++)
            {
                output.Add(Data[new Tuple<int, int>(i, header)]);
            }
            return output;
        }

        /// <summary>
        /// Get values by row index. You can use RowCount to see how many are available or simply grab one column and inspect it.
        /// </summary>
        /// <param name="row">Index of the row to be  retrieved.</param>
        /// <returns></returns>
        public List<object> ValuesByRowNumber(int row)
        {
            var output = new List<object>();
            for (var i = 0; i < InternalCsv.FieldCount; i++)
            {
                output.Add(Data[new Tuple<int, int>(row, i)]);
            }
            return output;
        }

        /// <summary>
        /// Get single value by its row and column index.
        /// </summary>
        /// <param name="row">Row index.</param>
        /// <param name="column">Column index.</param>
        /// <returns></returns>
        public object ValueByRowAndColumn(int row, int column)
        {
            return Data[new Tuple<int, int>(row, column)];
        }

        /// <summary>
        /// Get row number based on column and matching string.
        /// </summary>
        /// <param name="searchFor">Search string.</param>
        /// <param name="header">Header name.</param>
        /// <returns></returns>
        public List<int> RowNumberByHeaderAndString(string searchFor, string header)
        {
            var output = new List<int>();
            var headerIndex = InternalCsv.GetFieldIndex(header);
            for (var i = 0; i < RowCount; i++)
            {
                if (Data[new Tuple<int, int>(i, headerIndex)].ToString().Contains(searchFor))
                {
                    output.Add(i);
                }
            }
            return output;
        }

        /// <summary>
        /// Create CSV file to be parsed.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static Csv ByFilePath(object filePath)
        {
            var cleanFilePath = Utilities.FilePathUtilities.VerifyFilePath(filePath);
            return new Csv(cleanFilePath);
        }

        /// <summary>
        /// Get Headers.
        /// </summary>
        public string[] Headers
        {
            get { return InternalCsv.GetFieldHeaders(); }
        }

        /// <summary>
        /// Get Column count.
        /// </summary>
        public int ColumnCount
        {
            get { return InternalCsv.FieldCount; }
        }

        /// <summary>
        /// Get Row count.
        /// </summary>
        public int RowCount
        {
            get { return Rows; }
        }
    }
}
