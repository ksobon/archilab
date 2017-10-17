using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using CoreNodeModels;
using Dynamo.Configuration;
using Dynamo.Controls;
using Dynamo.Wpf;
using System.Windows;

namespace archilabUI
{
    public class DropDownNodeViewCustomization : INodeViewCustomization<DSDropDownBase>
    {
        private DSDropDownBase _model;

        public void CustomizeView(DSDropDownBase model, NodeView nodeView)
        {
            _model = model;

            //add a drop down list to the window
            var combo = new ComboBox
            {
                Width = double.NaN,
                MinWidth = 100,
                Height = Configurations.PortHeightInPixels,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center
            };
            nodeView.inputGrid.Children.Add(combo);
            Grid.SetColumn(combo, 0);
            Grid.SetRow(combo, 0);

            combo.DropDownOpened += combo_DropDownOpened;
            combo.SelectionChanged += delegate
            {
                if (combo.SelectedIndex != -1)
                    model.OnNodeModified();
            };

            combo.DropDownClosed += delegate
            {
                //disallow selection of nothing
                if (combo.SelectedIndex == -1)
                {
                    model.SelectedIndex = 0;
                }
            };

            combo.DataContext = model;
            //bind this combo box to the selected item hash

            var bindingVal = new Binding("Items") { Mode = BindingMode.TwoWay, Source = model };
            combo.SetBinding(ItemsControl.ItemsSourceProperty, bindingVal);

            //bind the selected index to the 
            var indexBinding = new Binding("SelectedIndex")
            {
                Mode = BindingMode.TwoWay,
                Source = model
            };
            combo.SetBinding(Selector.SelectedIndexProperty, indexBinding);
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// When the dropdown is opened, the node's implementation of PopulateItems is called
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void combo_DropDownOpened(object sender, EventArgs e)
        {
            _model.PopulateItems();
        }

    }
}
