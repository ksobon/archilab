using System;
using Xceed.Wpf.Toolkit;

namespace archilabUI.ListSelector
{
    /// <summary>
    /// Interaction logic for ListSelectorView.xaml
    /// </summary>
    public partial class ListSelectorView
    {
        public ListSelectorView()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }
    }
}
