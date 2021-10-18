using RevitServices.Persistence;

namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class References
    {
        internal Autodesk.Revit.DB.Reference InternalElement { get; }

        internal References(Autodesk.Revit.DB.Reference r)
        {
            InternalElement = r;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reference"></param>
        /// <returns></returns>
        public static References CreateByReference(Autodesk.Revit.DB.Reference reference)
        {
            return new References(reference);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reference"></param>
        /// <returns></returns>
        public static string ElementReferenceType(References reference)
        {
            return reference.InternalElement.ElementReferenceType.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reference"></param>
        /// <returns></returns>
        public static string GetStableRepresentation(References reference)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;

            return reference.InternalElement.ConvertToStableRepresentation(doc);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Reference: {InternalElement.ElementId}"; 
        }
    }
}
