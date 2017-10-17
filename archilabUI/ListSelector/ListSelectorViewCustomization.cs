using System.Windows.Threading;
using Dynamo.Controls;
using Dynamo.Scheduler;
using Dynamo.ViewModels;
using Dynamo.Wpf;

namespace archilabUI.ListSelector
{
    internal class ListSelectorViewCustomization : INodeViewCustomization<ListSelector>
    {
        private DynamoViewModel DynamoViewmodel;
        private DispatcherSynchronizationContext SyncContext;
        private ListSelector ViewModel;
        private ListSelectorView View;

        public void CustomizeView(ListSelector model, NodeView nodeView)
        {
            DynamoViewmodel = nodeView.ViewModel.DynamoViewModel;
            SyncContext = new DispatcherSynchronizationContext(nodeView.Dispatcher);
            ViewModel = model;
            ViewModel.EngineController = nodeView.ViewModel.DynamoViewModel.EngineController;
            View = new ListSelectorView
            {
                DataContext = model,
                MaxHeight = 300,
                MaxWidth = 200
            };
            nodeView.inputGrid.Children.Add(View);

            model.UpdateItemsCollection += UpdateCollection;
            UpdateCollection();
        }

        private void UpdateCollection()
        {
            // (Konrad) We pass a list of items already selected so that we can update them rather than create a new list.
            var selected = View._listBox.SelectedItems;
            var scheduler = DynamoViewmodel.Model.Scheduler;
            var delegateBasedAsyncTask = new DelegateBasedAsyncTask(scheduler, delegate
            {
                ViewModel.PopulateItems(selected);
            });
            delegateBasedAsyncTask.ThenSend(delegate { }, SyncContext);
            scheduler.ScheduleForExecution(delegateBasedAsyncTask);
        }

        public void Dispose()
        {
        }
    }
}
