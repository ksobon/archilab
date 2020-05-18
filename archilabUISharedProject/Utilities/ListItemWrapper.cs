namespace archilabUI.Utilities
{
    /// <summary>
    /// Wrapper class for Checkbox list items.
    /// </summary>
    public class ListItemWrapper
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ListItemWrapper item)) return false;

            return Name.Equals(item.Name) && Index.Equals(item.Index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Index.GetHashCode();
        }
    }
}
