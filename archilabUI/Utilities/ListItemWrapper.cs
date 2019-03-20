namespace archilabUI.Utilities
{
    /// <summary>
    /// Wrapper class for Checkbox list items.
    /// </summary>
    public class ListItemWrapper
    {
        public string Name { get; set; }
        public int Index { get; set; }
        public bool IsSelected { get; set; }

        public override bool Equals(object obj)
        {
            var item = obj as ListItemWrapper;
            if (item == null) return false;

            return Name.Equals(item.Name) && Index.Equals(item.Index);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Index.GetHashCode();
        }
    }
}
