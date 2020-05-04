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
        /// <returns></returns>
        public override string ToString()
        {
            return $"Reference: {InternalElement.ElementId}"; 
        }
    }
}
