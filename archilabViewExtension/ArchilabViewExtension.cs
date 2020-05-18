#region Reference

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using archilabViewExtension.RelayUI;
using Dynamo.Graph.Connectors;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;
// ReSharper disable UnusedMember.Global

#endregion

namespace archilabViewExtension
{
    public class ArchilabViewExtension : IViewExtension
    {
        private ViewLoadedParams _param;

        public void Dispose()
        {
        }

        public void Startup(ViewStartupParams p)
        {
        }

        public void Loaded(ViewLoadedParams p)
        {
            _param = p;
            _param.DynamoWindow.MouseLeftButtonUp += DynamoWindowOnMouseLeftButtonUp;
            _param.DynamoWindow.MouseDoubleClick += DynamoWindowOnMouseDoubleClick;
        }

        public void Shutdown()
        {
            _param.DynamoWindow.MouseLeftButtonUp -= DynamoWindowOnMouseLeftButtonUp;
            _param.DynamoWindow.MouseDoubleClick -= DynamoWindowOnMouseDoubleClick;
        }

        public string UniqueId
        {
            get { return "bdac5a46-fa1d-4aee-b34e-00778e30e779";}
        }

        public string Name
        {
            get { return "archilab"; }
        }

        internal List<DependencyObject> HitResultsList = new List<DependencyObject>();
        public HitTestResultBehavior MyHitTestResult(HitTestResult result)
        {
            HitResultsList.Add(result.VisualHit);
            return HitTestResultBehavior.Continue;
        }

        private void DynamoWindowOnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // (Konrad) Get a list of UIElements that are beneath the mouse pointer.
                var pt = e.GetPosition((UIElement)sender);
                HitResultsList.Clear();
                VisualTreeHelper.HitTest(_param.DynamoWindow, null, MyHitTestResult, new PointHitTestParameters(pt));
                if (HitResultsList.Count <= 0) return;

                // (Konrad) Check if Relay was hit.
                var relay = HitResultsList.FirstOrDefault(x => x is Grid p && p.DataContext is Relay);
                if (!(((Grid)relay)?.DataContext is Relay r)) return;

                // (Konrad) Remove Connectors.
                var conn1 = r.InPorts.FirstOrDefault()?.Connectors.FirstOrDefault();
                if (conn1 == null) return;

                var method = conn1.GetType()
                    .GetMethod("Delete", BindingFlags.NonPublic | BindingFlags.Instance);
                method?.Invoke(conn1, new object[] { });

                var conn2 = r.OutPorts.FirstOrDefault()?.Connectors.FirstOrDefault();
                if (conn2 == null) return;

                var method2 = conn2.GetType()
                    .GetMethod("Delete", BindingFlags.NonPublic | BindingFlags.Instance);
                method2?.Invoke(conn2, new object[] { });

                // (Konrad) Create new Connector.
                var unused = new ConnectorModel(conn1.Start, conn2.End, Guid.NewGuid());

                // (Konrad) Delete the Relay.
                (_param.DynamoWindow.DataContext as DynamoViewModel)?.CurrentSpaceViewModel.DynamoViewModel
                    .ExecuteCommand(new DynamoModel.DeleteModelCommand(r.GUID));
            }
            catch
            {
                // ignore
            }
        }

        private void DynamoWindowOnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ClickCount != 1) return;
                if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))) return;

                // (Konrad) Get a list of UIElements that are beneath the mouse pointer.
                var pt = e.GetPosition((UIElement)sender);
                HitResultsList.Clear();
                VisualTreeHelper.HitTest(_param.DynamoWindow, null, MyHitTestResult, new PointHitTestParameters(pt));
                if (HitResultsList.Count <= 0) return;

                // (Konrad) Check if Wire/Connector was hit.
                var path = HitResultsList.FirstOrDefault(x => x is Path p && p.DataContext is ConnectorViewModel);
                if (path == null) return;

                // (Konrad) Calculate area 25% smaller then the BoundingBox of connector.
                // This will allow us to exclude end points, so we don't trigger by accident.
                var connPath = (Path)path;
                var bounds = connPath.RenderedGeometry.Bounds;
                var topLeft = connPath.TranslatePoint(bounds.TopLeft, (UIElement)sender);
                var bottomRight = connPath.TranslatePoint(bounds.BottomRight, (UIElement)sender);
                var isXIn = (pt.X > topLeft.X + (bounds.Width * 0.25)) && (pt.X < bottomRight.X - (bounds.Width * 0.25));
                var isYIn = (pt.Y < bottomRight.Y - (bounds.Height * 0.25)) && (pt.Y > topLeft.Y + (bounds.Height * 0.25));
                if (!isYIn || !isXIn) return;

                // (Konrad) Calculate mid point between two existing nodes.
                var dataContext = (ConnectorViewModel)connPath.DataContext;
                var startNodeCenter = dataContext.ConnectorModel.Start.Center;
                var endNodeCenter = dataContext.ConnectorModel.End.Center;
                var relayCenter = new Point((startNodeCenter.X + endNodeCenter.X) / 2,
                    (startNodeCenter.Y + endNodeCenter.Y) / 2);
                
                // (Konrad) Create and place Relay node.
                var relayNode = new Relay();
                (_param.DynamoWindow.DataContext as DynamoViewModel)?.CurrentSpaceViewModel.DynamoViewModel
                    .ExecuteCommand(new DynamoModel.CreateNodeCommand(relayNode, relayCenter.X - 45, relayCenter.Y - 45, false, true));

                // (Konrad) Create and place connectors to/from Relay node.
                var start = dataContext.ConnectorModel.Start;
                var end = dataContext.ConnectorModel.End;

                // (John) use recordable commands to connect the relay this allows for undo commands
                var dvm = (_param.DynamoWindow.DataContext as DynamoViewModel)?.CurrentSpaceViewModel.DynamoViewModel;

                // (John) connect the input of the relay
                dvm?.ExecuteCommand(
                    new DynamoModel.MakeConnectionCommand(start.Owner.GUID, start.Index, PortType.Output,
                        DynamoModel.MakeConnectionCommand.Mode.Begin));
                dvm?.ExecuteCommand(
                    new DynamoModel.MakeConnectionCommand(relayNode.GUID, 0, PortType.Input, DynamoModel.MakeConnectionCommand.Mode.End));

                // (John) connect the output of the relay
                dvm?.ExecuteCommand(
                    new DynamoModel.MakeConnectionCommand(relayNode.GUID,0,PortType.Output,DynamoModel.MakeConnectionCommand.Mode.Begin));
                dvm?.ExecuteCommand(
                    new DynamoModel.MakeConnectionCommand(end.Owner.GUID, end.Index, PortType.Input, DynamoModel.MakeConnectionCommand.Mode.End));

                // (Konrad) Remove existing connector.
                var method = dataContext.ConnectorModel.GetType()
                    .GetMethod("Delete", BindingFlags.NonPublic | BindingFlags.Instance);
                method?.Invoke(dataContext.ConnectorModel, new object[] { });
            }
            catch
            {
                // ignore
            }

            e.Handled = true;
        }
    }
}
