using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace archilabUI.Utilities
{
    /// <summary>
    /// Wrapper class for Checkbox list items.
    /// </summary>
    public class ListItemWrapper : INotifyPropertyChanged
    {
        //bool disposed;
        public string Name { get; set; }
        public int Index { get; set; }
        public bool IsSelected { get; set; }

        //private bool _isSelected;
        //public bool IsSelected
        //{
        //    get { return _isSelected; }
        //    set
        //    {
        //        _isSelected = value;
        //        RaisePropertyChanged("IsSelected");
        //    }
        //}

        [JsonConstructor]
        public ListItemWrapper()
        {
        }

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

        //protected virtual void Dispose(bool disposing)
        //{
        //    if (!disposed)
        //    {
        //        if (disposing)
        //        {
        //            //dispose managed resources
        //        }
        //    }
        //    disposed = true;
        //}

        //public void Dispose()
        //{
        //    Dispose(true);
        //    GC.SuppressFinalize(this);
        //}

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }
    }
}
