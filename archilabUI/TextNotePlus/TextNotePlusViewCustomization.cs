using System.Windows;
using System.Windows.Controls;
using Dynamo.Controls;
using Dynamo.Wpf;

namespace archilabUI.TextNotePlus
{
    internal class TextNotePlusViewCustomization : INodeViewCustomization<TextNotePlus>
    {
        private TextNotePlus _viewModel;
        private TextNotePlusView _view;

        public void CustomizeView(TextNotePlus model, NodeView nodeView)
        {
            _viewModel = model;
            _view = new TextNotePlusView
            {
                DataContext = model,
                MainGrid =
                {
                    Width = _viewModel.NoteWidth,
                    Height = _viewModel.NoteHeight
                }
            };

            _viewModel.TextBox = _view.TextBox;
            _viewModel.MainGrid = _view.MainGrid;
            _viewModel.Cursor = _view.Cursor;

            nodeView.inputGrid.Children.Add(_view);

            // (Konrad) Minimize node name and glyphs below.
            // Turn off the slide out when hovered over.
            var children = nodeView.grid.Children;
            foreach (var child in children)
            {
                var e = child as FrameworkElement;
                if(e == null) continue;

                if (e.Name == "nickNameBlock")
                {
                    var rows = ((Grid)e.Parent).RowDefinitions;
                    for (var i = 0; i < rows.Count; i++)
                    {
                        if (i == 2) continue;
                        var row = rows[i];
                        row.Height = new GridLength(0);
                    }
                }
                else if (e.Name == "expansionBay")
                {
                    e.Visibility = Visibility.Collapsed;
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
