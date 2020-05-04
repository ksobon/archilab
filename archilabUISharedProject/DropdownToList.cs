using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using CoreNodeModels;
using Dynamo.Controls;
using Dynamo.Engine;
using Dynamo.Graph.Nodes;
using Dynamo.Models;
using Dynamo.Scheduler;
using Dynamo.ViewModels;
using Dynamo.Wpf;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;

namespace archilabUI
{
    [NodeName("Dropdown > List")]
    [NodeCategory("archilab.Core.Lists")]
    [NodeDescription("Do something.")]
    [IsDesignScriptCompatible]
    public class DropdownToListNodeModel : NodeModel
    {
        public event Action RequestUpdateNode;
        protected virtual void OnRequestUpdateNode()
        {
            RequestUpdateNode?.Invoke();
        }

        private List<object> _input;
        public List<object> Input
        {
            get { return _input; }
            set
            {
                if (_input != value)
                {
                    _input = value;
                    OnNodeModified();
                }
            }
        }

        public DropdownToListNodeModel()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("dropdown", "Feed in a dropdown node.")));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("List", "No one knows for sure what this does...")));
            RegisterAllPorts();
            ArgumentLacing = LacingStrategy.Disabled;

            foreach (var port in InPorts)
            {
                port.Connectors.CollectionChanged += Connectors_CollectionChanged;
            }
        }

        [JsonConstructor]
        protected DropdownToListNodeModel(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts) { }

        private void Connectors_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnRequestUpdateNode();
        }

        public List<object> GetInputString(EngineController engine)
        {
            var output = new List<object>();

            if (InPorts[0].Connectors[0].Start.Owner is DSDropDownBase node) output = node.Items.Select(x => x.Item).ToList();

            return output;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (IsPartiallyApplied)
            {
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            }

            if (Input == null)
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            var nodes = new List<AssociativeNode>();
            foreach (var o in Input)
            {
                nodes.Add(AstFactory.BuildStringNode(o.ToString()));
            }

            AssociativeNode listNode = AstFactory.BuildExprList(nodes);

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), listNode) };
        }
    }

    public class NodeViewCustomization : INodeViewCustomization<DropdownToListNodeModel>
    {
        private DynamoModel _dynamoModel;
        private DynamoViewModel _dynamoViewModel;
        private DropdownToListNodeModel _customNode;

        public void CustomizeView(DropdownToListNodeModel model, NodeView nodeView)
        {
            _dynamoModel = nodeView.ViewModel.DynamoViewModel.Model;
            _dynamoViewModel = nodeView.ViewModel.DynamoViewModel;
            _customNode = model;

            _customNode.RequestUpdateNode += UpdateNode;
        }

        private void UpdateNode()
        {
            var s = _dynamoViewModel.Model.Scheduler;
            var t = new DelegateBasedAsyncTask(s, () =>
            {
                _customNode.Input = _customNode.GetInputString(_dynamoModel.EngineController);
            });

            s.ScheduleForExecution(t);
        }

        public void Dispose() { }
    }
}
