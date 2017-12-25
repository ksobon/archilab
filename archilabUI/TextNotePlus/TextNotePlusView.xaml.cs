using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;

namespace archilabUI.TextNotePlus
{
    /// <summary>
    /// Interaction logic for TextNotePlusView.xaml
    /// </summary>
    public partial class TextNotePlusView
    {
        private Cursor _cursor;
        public Grid MainGrid;

        public TextNotePlusView()
        {
            InitializeComponent();
            RichTextBox.IsEnabled = false;
            MainGrid = sizableContent;
        }

        private void OnResizeThumbDragStarted(object sender, DragStartedEventArgs e)
        {
            _cursor = Cursor;
            Cursor = Cursors.SizeNWSE;
        }

        private void OnResizeThumbDragCompleted(object sender, DragCompletedEventArgs e)
        {
            Cursor = _cursor;
        }

        private void OnResizeThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            var yAdjust = MainGrid.Height + e.VerticalChange;
            var xAdjust = MainGrid.Width + e.HorizontalChange;

            //make sure not to resize to negative width or heigth            
            xAdjust = (MainGrid.ActualWidth + xAdjust) > MainGrid.MinWidth ? xAdjust : MainGrid.MinWidth;
            yAdjust = (MainGrid.ActualHeight + yAdjust) > MainGrid.MinHeight ? yAdjust : MainGrid.MinHeight;

            MainGrid.Width = xAdjust;
            MainGrid.Height = yAdjust;
        }

        private void RichTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            RichTextBox.IsEnabled = false;
            e.Handled = true;
        }

        private void OnNoteMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                RichTextBox.IsEnabled = true;
                RichTextBox.Focus();
                e.Handled = true;
            }
        }
    }
}
