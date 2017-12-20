using System;
using Xceed.Wpf.Toolkit;

namespace archilabUI.TextNotePlus
{
    /// <summary>
    /// Interaction logic for TextNotePlusView.xaml
    /// </summary>
    public partial class TextNotePlusView
    {
        public TextNotePlusView()
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
