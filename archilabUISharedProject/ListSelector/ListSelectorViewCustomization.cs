using System.Windows.Threading;
using Dynamo.Controls;
using Dynamo.Scheduler;
using Dynamo.ViewModels;
using Dynamo.Wpf;
// ReSharper disable UnusedMember.Global

namespace archilabUI.ListSelector
{
    internal class ListSelectorViewCustomization : INodeViewCustomization<ListSelector>
    {
        private DynamoViewModel _dynamoViewmodel;
        private DispatcherSynchronizationContext _syncContext;
        private ListSelector _viewModel;
        private ListSelectorView _view;

        public void CustomizeView(ListSelector model, NodeView nodeView)
        {
            if (nodeView?.Dispatcher == null) return;

            _dynamoViewmodel = nodeView.ViewModel.DynamoViewModel;
            _syncContext = new DispatcherSynchronizationContext(nodeView.Dispatcher);
            _viewModel = model;
            _viewModel.EngineController = nodeView.ViewModel.DynamoViewModel.EngineController;
            _view = new ListSelectorView
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
            var selected = _view.ListBox.SelectedItems;
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
