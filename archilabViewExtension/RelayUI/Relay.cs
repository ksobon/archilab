 #region References

 using System;
 using System.Collections.Generic;
 using Dynamo.Engine;
 using Dynamo.Graph.Nodes;
 using Dynamo.Scheduler;
 using Dynamo.Visualization;
 using Newtonsoft.Json;
 using ProtoCore.AST.AssociativeAST;
 using VMDataBridge;

 #endregion

namespace archilabViewExtension.RelayUI
{
    [NodeName("Relay")]
    [NodeCategory("archilab.Core.Utilities")]
    [NodeDescription("Relays data. Useful for Wire management.")]
    [NodeSearchTags("Relay, Data Relay")]
    [OutPortTypes("Node Output: var")]
    [IsDesignScriptCompatible]
    public class Relay : NodeModel
    {
        public event Action<object> EvaluationComplete;

        [JsonIgnore]
        public new object CachedValue;

        [JsonIgnore]
        public bool HasRunOnce { get; private set; }

        public Relay()
        {
            InPorts.Add(new PortModel(PortType.Input, this, new PortData("", "Input")));
            OutPorts.Add(new PortModel(PortType.Output, this, new PortData("", "Output")));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;

            ShouldDisplayPreviewCore = false;
            HasRunOnce = false;
        }

        [JsonConstructor]
        protected Relay(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts) : base(inPorts, outPorts)
        {
            ArgumentLacing = LacingStrategy.Disabled;
            ShouldDisplayPreviewCore = false;
            HasRunOnce = false;
        }

        protected override void OnBuilt()
        {
            base.OnBuilt();
            DataBridge.Instance.RegisterCallback(GUID.ToString(), OnEvaluationComplete);
        }

        public override void Dispose()
        {
            base.Dispose();
            DataBridge.Instance.UnregisterCallback(GUID.ToString());
        }

        private void OnEvaluationComplete(object obj)
        {
            CachedValue = obj;
            HasRunOnce = true;

            EvaluationComplete?.Invoke(obj);
        }


        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (IsPartiallyApplied)
            {
                return new[]
                {
                    AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildFunctionObject(
                        new IdentifierListNode
                        {
                            LeftNode = AstFactory.BuildIdentifier("DataBridge"),
                            RightNode = AstFactory.BuildIdentifier("BridgeData")
                        }, 2, new[] {0}, new List<AssociativeNode>
                        {
                            AstFactory.BuildStringNode(GUID.ToString()),
                            AstFactory.BuildNullNode()
                        }))
                };
            }

            var resultAst = new[]
            {
                AstFactory.BuildAssignment(
                    AstFactory.BuildIdentifier(AstIdentifierBase + "_dummy"),
                    DataBridge.GenerateBridgeDataAst(GUID.ToString(), inputAstNodes[0])),
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), inputAstNodes[0])
            };

            return resultAst;
        }

        public override bool RequestVisualUpdateAsync(IScheduler scheduler, EngineController engine,
            IRenderPackageFactory factory, bool forceUpdate = false)
        {
            return false;
        }
    }
}