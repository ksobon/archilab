using System.ComponentModel;

namespace archilabUI.Utilities
{
    /// <summary>
    /// Wrapper class for Checkbox list items.
    /// </summary>
    public class ListItemWrapper : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public int Index { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                RaisePropertyChanged("IsSelected");
            }
        }

        public override bool Equals(object obj)
        {
            var item = obj as ListItemWrapper;

            if (item == null)
            {
                return false;
            }
            return Name.Equals(item.Name) && Index.Equals(item.Index);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Index.GetHashCode();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }
    }
}
