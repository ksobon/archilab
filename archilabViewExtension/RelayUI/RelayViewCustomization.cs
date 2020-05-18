#region References

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using Dynamo.Controls;
using Dynamo.Wpf;

// ReSharper disable UnusedMember.Global

#endregion

namespace archilabViewExtension.RelayUI
{
    internal class RelayViewCustomization : INodeViewCustomization<Relay>
    {
        private RelayView _view;

        public void CustomizeView(Relay model, NodeView nodeView)
        {
            _view = new RelayView
            {
                DataContext = model
            };

            nodeView.inputGrid.Children.Add(_view);

            // (Konrad) Hide Node Name and move Background one Row down.
            var grid = nodeView.grid;
            var children = grid.Children.Cast<UIElement>().Where(x => Grid.GetRow(x) == 1);
            foreach (var e in children)
            {
                if (e is Rectangle r && r.Name == "nodeBackground")
                {
                    r.SetValue(Grid.RowProperty, 2);
                    r.SetValue(Grid.RowSpanProperty, 1);
                    continue;
                }

                e.Visibility = Visibility.Hidden;
            }
        }

        public void Dispose()
        {
        }
    }
}
