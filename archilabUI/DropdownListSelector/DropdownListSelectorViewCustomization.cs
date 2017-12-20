using System.Windows.Threading;
using Dynamo.Controls;
using Dynamo.Scheduler;
using Dynamo.ViewModels;
using Dynamo.Wpf;

namespace archilabUI.DropdownListSelector
{
    internal class DropdownListSelectorViewCustomization : INodeViewCustomization<DropdownListSelector>
    {
        private DynamoViewModel _dynamoViewmodel;
        private DispatcherSynchronizationContext _syncContext;
        private DropdownListSelector _viewModel;
        private DropdownListSelectorView _view;

        public void CustomizeView(DropdownListSelector model, NodeView nodeView)
        {
            _dynamoViewmodel = nodeView.ViewModel.DynamoViewModel;
            _syncContext = new DispatcherSynchronizationContext(nodeView.Dispatcher);
            _viewModel = model;
            _viewModel.EngineController = nodeView.ViewModel.DynamoViewModel.EngineController;
            _view = new DropdownListSelectorView
            {
                DataContext = model,
                MaxHeight = 300,
                MaxWidth = 200
            };
            nodeView.inputGrid.Children.Add(_view);

            model.UpdateItemsCollection += UpdateCollection;
            UpdateCollection();
        }

        private void UpdateCollection()
        {
            // (Konrad) We pass a list of items already selected so that we can update them rather than create a new list.
            var selected = _view._listBox.SelectedItems;
            var scheduler = _dynamoViewmodel.Model.Scheduler;
            var delegateBasedAsyncTask = new DelegateBasedAsyncTask(scheduler, delegate
            {
                _viewModel.PopulateItems(selected);
            });
            delegateBasedAsyncTask.ThenSend(delegate { }, _syncContext);
            scheduler.ScheduleForExecution(delegateBasedAsyncTask);
        }

        public void Dispose()
        {
        }
    }
}
