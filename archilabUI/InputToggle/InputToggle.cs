using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;
using Dynamo.ViewModels;
using ProtoCore.AST.AssociativeAST;
using GalaSoft.MvvmLight.Command;

namespace archilabUI.InputToggle
{
    [NodeName("Input Toggle")]
    [NodeCategory("archilab.Core.Utilities")]
    [NodeDescription("Use this node to toggle the IsInput for ALL nodes in the definition ON/OFF.")]
    [IsDesignScriptCompatible]
    public class InputToggle : NodeModel
    {
        public WorkspaceViewModel WorkspaceViewModel { get; set; }
        public RelayCommand ToggleInput { get; set; }

        private bool _toggleState = true;
        public bool ToggleState {
            get { return _toggleState; }
            set { _toggleState = value; RaisePropertyChanged("ToggleState"); }
        }

        public InputToggle()
        {
            RegisterAllPorts();
            ArgumentLacing = LacingStrategy.Disabled;

            ToggleInput = new RelayCommand(OnToggleInput);
        }

        private void OnToggleInput()
        {
            ToggleState = !ToggleState;
            foreach (var node in WorkspaceViewModel.Nodes)
            {
                if (!node.IsInput) continue;

                node.IsSetAsInput = ToggleState;
            }
        }

        [IsVisibleInDynamoLibrary(false)]
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            return new[]
            {
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode())
            };
        }
    }
}
