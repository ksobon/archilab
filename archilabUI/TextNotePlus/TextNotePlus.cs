using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Autodesk.DesignScript.Runtime;
using Dynamo.Engine;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using ProtoCore.AST.AssociativeAST;

namespace archilabUI.TextNotePlus
{
    [NodeName("Text Note Plus")]
    [NodeCategory("archilab.Core.Lists")]
    [NodeDescription("Use this node to create a resizable text note.")]
    [IsDesignScriptCompatible]
    public class TextNotePlus : NodeModel
    {
        internal EngineController EngineController { get; set; }
        public string Notes { get; set; } = "Please enter text here...";
        public int NoteWidth { get; set; } = 225;
        public int NoteHeight { get; set; } = 60;

        public TextNotePlus()
        {
            RegisterAllPorts();
            ArgumentLacing = LacingStrategy.Disabled;
        }

        protected override void SerializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.SerializeCore(nodeElement, context);
            if (nodeElement.OwnerDocument == null) return;

            var note = nodeElement.OwnerDocument.CreateElement("notes");
            note.InnerText = Notes;

            var noteWidth = nodeElement.OwnerDocument.CreateElement("noteWidth");
            noteWidth.InnerText = NoteWidth.ToString();

            var noteHeight = nodeElement.OwnerDocument.CreateElement("noteHeight");
            noteHeight.InnerText = NoteHeight.ToString();

            nodeElement.AppendChild(note);
            nodeElement.AppendChild(noteWidth);
            nodeElement.AppendChild(noteHeight);
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context);

            var note = nodeElement.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Name == "notes")?.InnerText ?? "Please enter text here...";
            var noteWidth = nodeElement.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Name == "noteWidth")?.InnerText ?? "225";
            var noteHeight = nodeElement.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Name == "noteHeight")?.InnerText ?? "60";

            Notes = note;
            NoteWidth = int.Parse(noteWidth);
            NoteHeight = int.Parse(noteHeight);
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
