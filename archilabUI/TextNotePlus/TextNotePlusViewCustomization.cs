using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using archilabUI.ListSelector;
using Dynamo.Controls;
using Dynamo.Scheduler;
using Dynamo.ViewModels;
using Dynamo.Wpf;

namespace archilabUI.TextNotePlus
{
    internal class TextNotePlusViewCustomization : INodeViewCustomization<TextNotePlus>
    {
        private DynamoViewModel _dynamoViewmodel;
        private DispatcherSynchronizationContext _syncContext;
        private TextNotePlus _viewModel;
        private TextNotePlusView _view;

        public void CustomizeView(TextNotePlus model, NodeView nodeView)
        {
            _dynamoViewmodel = nodeView.ViewModel.DynamoViewModel;
            _syncContext = new DispatcherSynchronizationContext(nodeView.Dispatcher);
            _viewModel = model;
            _viewModel.EngineController = nodeView.ViewModel.DynamoViewModel.EngineController;
            _view = new TextNotePlusView
            {
                DataContext = model,
                MaxHeight = 300,
                MaxWidth = 300
            };
            nodeView.inputGrid.Children.Add(_view);
            var children = nodeView.grid.Children;
            foreach (var child in children)
            {
                var e = child as FrameworkElement;
                if(e == null) continue;
                var name = e.Name;
                if (name == "expansionBay") e.Visibility = Visibility.Collapsed;
            }


            //model.UpdateItemsCollection += UpdateCollection;
            //UpdateCollection();
        }

        //private void UpdateCollection()
        //{
        //    // (Konrad) We pass a list of items already selected so that we can update them rather than create a new list.
        //    var selected = _view._listBox.SelectedItems;
        //    var scheduler = _dynamoViewmodel.Model.Scheduler;
        //    var delegateBasedAsyncTask = new DelegateBasedAsyncTask(scheduler, delegate
        //    {
        //        _viewModel.PopulateItems(selected);
        //    });
        //    delegateBasedAsyncTask.ThenSend(delegate { }, _syncContext);
        //    scheduler.ScheduleForExecution(delegateBasedAsyncTask);
        //}

        public void Dispose()
        {
        }
    }
}
