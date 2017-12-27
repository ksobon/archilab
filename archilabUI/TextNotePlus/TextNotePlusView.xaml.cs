using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace archilabUI.TextNotePlus
{
    /// <summary>
    /// Interaction logic for TextNotePlusView.xaml
    /// </summary>
    public partial class TextNotePlusView
    {
        public Grid MainGrid;
        public Xceed.Wpf.Toolkit.RichTextBox TextBox;

        public TextNotePlusView()
        {
            InitializeComponent();
            RichTextBox.IsEnabled = false;
            MainGrid = SizableContent;
            TextBox = RichTextBox;
        }

        private void RichTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            RichTextBox.IsEnabled = false;
            e.Handled = true;
        }

        private void OnNoteMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount < 2) return;

            RichTextBox.IsEnabled = true;
            RichTextBox.Focus();
            e.Handled = true;
        }
    }
}
