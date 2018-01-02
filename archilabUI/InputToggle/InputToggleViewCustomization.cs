using Dynamo.Controls;
using Dynamo.Wpf;

namespace archilabUI.InputToggle
{
    internal class InputToggleViewCustomization : INodeViewCustomization<InputToggle>
    {
        private InputToggle _viewModel;
        private InputToggleView _view;

        public void CustomizeView(InputToggle model, NodeView nodeView)
        {
            _viewModel = model;
            _viewModel.WorkspaceViewModel = nodeView.ViewModel.WorkspaceViewModel;
            _view = new InputToggleView
            {
                DataContext = model,
                MaxHeight = 300,
                MaxWidth = 200
            };
            nodeView.inputGrid.Children.Add(_view);
        }

        public void Dispose()
        {
        }
    }
}
