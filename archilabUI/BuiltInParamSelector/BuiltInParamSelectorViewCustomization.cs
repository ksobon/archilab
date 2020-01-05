using System.Linq;
using System.Windows.Threading;
using Dynamo.Controls;
using Dynamo.Scheduler;
using Dynamo.ViewModels;
using Dynamo.Wpf;
// ReSharper disable UnusedMember.Global

namespace archilabUI.BuiltInParamSelector
{
    internal class ParameterSelectorViewCustomization : INodeViewCustomization<BuiltInParamSelector>
    {
        private DynamoViewModel _dynamoViewmodel;
        private DispatcherSynchronizationContext _syncContext;
        private BuiltInParamSelector _viewModel;
        private BuiltInParamSelectorView _view;

        public void CustomizeView(BuiltInParamSelector model, NodeView nodeView)
        {
            if (nodeView?.Dispatcher == null) return;

            _dynamoViewmodel = nodeView.ViewModel.DynamoViewModel;
            _syncContext = new DispatcherSynchronizationContext(nodeView.Dispatcher);
            _viewModel = model;
            _viewModel.EngineController = nodeView.ViewModel.DynamoViewModel.EngineController;
            _view = new BuiltInParamSelectorView();
            nodeView.inputGrid.Children.Add(_view);
            _view.DataContext = model;
            model.RequestChangeBuiltInParamSelector += UpdateParameterSelector;
            UpdateParameterSelector();
        }

        private void UpdateParameterSelector()
        {
            var scheduler = _dynamoViewmodel.Model.Scheduler;
            var delegateBasedAsyncTask = new DelegateBasedAsyncTask(scheduler, delegate
            {
                _viewModel.PopulateItems();
            });

            delegateBasedAsyncTask.ThenSend(delegate
            {
                _viewModel.SelectedItem = _viewModel.SelectedItem ?? _viewModel.ItemsCollection.FirstOrDefault();
            }, _syncContext);

            scheduler.ScheduleForExecution(delegateBasedAsyncTask);
        }

        public void Dispose()
        {
        }
    }
}
