using System;
using System.Windows;
using System.Windows.Input;
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
                RichTextBox.IsEnabled = false;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }

        private void OnNoteMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount < 2) return;

            RichTextBox.IsEnabled = true;
            e.Handled = true;
        }

        private void RichTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            RichTextBox.IsEnabled = false;
            e.Handled = true;
        }

        public Point StartPosition;
        private bool _isResizing;
        private void ResizeGrip_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Mouse.Capture(ResizeGrip)) return;

            _isResizing = true;
            StartPosition = Mouse.GetPosition(this);
        }

        private void window_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isResizing) return;

            var currentPosition = Mouse.GetPosition(this);
            var diffX = currentPosition.X - StartPosition.X;
            var diffY = currentPosition.Y - StartPosition.Y;
            var currentLeft = GridResize.Margin.Left;
            var currentTop = GridResize.Margin.Top;
            GridResize.Margin = new Thickness(currentLeft + diffX, currentTop + diffY, 0, 0);
            StartPosition = currentPosition;
        }

        private void ResizeGrip_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isResizing) return;

            _isResizing = false;
            Mouse.Capture(null);
        }
    }
}
