namespace archilab.Revit.Elements
{
    /// <summary>
    /// 
    /// </summary>
    public class Faces
    {
        private Autodesk.Revit.DB.Face InternalElement { get; }

        internal Faces(Autodesk.Revit.DB.Face f)
        {
            InternalElement = f;
        }

        /// <summary>
        /// 
        /// </summary>
        public References Reference
        {
            get { return new References(InternalElement.Reference); }
        }
    }
}
