using System;

namespace archilabUI.DropdownListSelector
{
    /// <summary>
    /// Interaction logic for DropdownListSelectorView.xaml
    /// </summary>
    public partial class DropdownListSelectorView
    {
        public DropdownListSelectorView()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                var unused = ex.Message;
            }
        }
    }
}
