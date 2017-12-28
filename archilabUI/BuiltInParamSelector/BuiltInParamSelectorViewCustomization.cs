using System;
using System.Linq;
using System.Windows.Threading;
using archilabUI.Utilities;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Scheduler;
using Dynamo.ViewModels;
using Dynamo.Wpf;

namespace archilabUI.BuiltInParamSelector
{
    internal class ParameterSelectorViewCustomization : INodeViewCustomization<BuiltInParamSelector>, IDisposable
    {
        private DynamoViewModel DynamoViewmodel;
        private DispatcherSynchronizationContext SyncContext;
        private DynamoModel DynamoModel;
        private BuiltInParamSelector ViewModel;
        private BuiltInParamSelectorView View;

        public void CustomizeView(BuiltInParamSelector model, NodeView nodeView)
        {
            DynamoModel = nodeView.ViewModel.DynamoViewModel.Model;
            DynamoViewmodel = nodeView.ViewModel.DynamoViewModel;
            SyncContext = new DispatcherSynchronizationContext(nodeView.Dispatcher);
            ViewModel = model;
            ViewModel.EngineController = nodeView.ViewModel.DynamoViewModel.EngineController;
            View = new BuiltInParamSelectorView();
            nodeView.inputGrid.Children.Add(View);
            View.DataContext = model;
            model.RequestChangeBuiltInParamSelector += new Action(UpdateParameterSelector);
            this.UpdateParameterSelector();
        }

        private void UpdateParameterSelector()
        {
            DynamoScheduler scheduler = DynamoViewmodel.Model.Scheduler;
            DelegateBasedAsyncTask delegateBasedAsyncTask = new DelegateBasedAsyncTask(scheduler, delegate
            {
                ViewModel.PopulateItems();
            });
            AsyncTaskExtensions.ThenSend(delegateBasedAsyncTask, delegate (AsyncTask _)
            {
                if (ViewModel.SelectedItem != null)
                {
                    ViewModel.SelectedItem = ViewModel.SelectedItem;
                }
                else
                {
                    ViewModel.SelectedItem = ViewModel.ItemsCollection.FirstOrDefault<ParameterWrapper>();
                }
            }, SyncContext);
            scheduler.ScheduleForExecution(delegateBasedAsyncTask);
        }

        public void Dispose()
        {
        }
    }
}
