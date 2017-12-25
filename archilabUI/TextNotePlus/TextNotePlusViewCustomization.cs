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
            _viewModel.EngineController = nodeView.ViewModel.DynamoViewModel.EngineController;
            _view = new TextNotePlusView
            {
                DataContext = model,
                MaxHeight = 1000,
                MaxWidth = 1000,
                MainGrid =
                {
                    Width = _viewModel.NoteWidth,
                    Height = _viewModel.NoteHeight
                }
            };

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
