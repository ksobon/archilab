// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace archilab.Pdf
{
    /// <summary>
    /// 
    /// </summary>
    public class PDF
    {
        internal PDF()
        {
        }

        /// <summary>
        /// Returns page count of a PDF document.
        /// </summary>
        /// <param name="filePath">File Path to a PDF document.</param>
        /// <returns>Number of pages in the PDF.</returns>
        public static int PageCount(string filePath)
        {
            var pdfReader = new iTextSharp.text.pdf.PdfReader(filePath);
            return pdfReader.NumberOfPages;
        }
    }
}
