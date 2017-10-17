using System.Collections.Generic;
using System.IO;
using LumenWorks.Framework.IO.Csv;

namespace archilab.Revit.Elements
{
    /// <summary>
    /// Library of custom warnings and ratings.
    /// </summary>
    public class WarningLibrary
    {
        internal WarningLibrary()
        {
        }

        /// <summary>
        /// Dictionary of stored Warning - Rating relationships.
        /// </summary>
        public Dictionary<string, int> Ratings { get; set; }

        /// <summary>
        /// Creates a custom warning library that stores all ratings for warnings.
        /// </summary>
        /// <param name="filePath">File path to CSV file that contains all warnings/ratings.</param>
        /// <returns name="WarningLibrary">Warning Library</returns>
        public static WarningLibrary WarningLibraryFromCsv(object filePath)
        {
            var cleanFilePath = Utilities.FilePathUtilities.VerifyFilePath(filePath);

            var warningLibrary = new Dictionary<string, int>();
            using (var csv = new CsvReader(new StreamReader(cleanFilePath), true))
            {
                while (csv.ReadNextRecord())
                {
                    warningLibrary.Add(csv[0].Trim(), int.Parse(csv[1]));
                }
            }
            return new WarningLibrary { Ratings = warningLibrary };
        }
    }
}
