using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Documents;
using System.Xml;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using GalaSoft.MvvmLight.Command;
using ProtoCore.AST.AssociativeAST;
using Xceed.Wpf.Toolkit;

namespace archilabUI.TextNotePlus
{
    [NodeName("Text Note Plus")]
    [NodeCategory("archilab.Core.Lists")]
    [NodeDescription("Use this node to create a resizable text note.")]
    [IsDesignScriptCompatible]
    public class TextNotePlus : NodeModel
    {
        public string Notes { get; set; } = "Please enter text here...";
        public int NoteWidth { get; set; } = 225;
        public int NoteHeight { get; set; } = 60;

        public ObservableCollection<string> TextSizes { get; set; } = new ObservableCollection<string>
        {
            "8","9","10","11","12","14","16","18","20","22","24","26","28","36","48","72"
        };

        public RichTextBox TextBox { get; set; }

        public RelayCommand ShowHideRow { get; set; }

        private string _selectedTextSize;
        public string SelectedTextSize {
            get { return _selectedTextSize; }
            set
            {
                _selectedTextSize = value;
                RaisePropertyChanged("SelectedTextSize");
                OnTextSizeChanged(value);
            }
        }

        private bool _isHiddenRow = true;
        public bool IsHiddenRow {
            get { return _isHiddenRow; }
            set { _isHiddenRow = value; RaisePropertyChanged("IsHiddenRow"); }
        }

        private void OnShowHideRow()
        {
            IsHiddenRow = !IsHiddenRow;
        }

        private void OnTextSizeChanged(string input)
        {
            if (TextBox == null) return;
            if (string.IsNullOrEmpty(input)) return;

            var value = double.Parse(input);
            var target = TextBox;

            // Make sure we have a selection. Should have one even if there is no text selected.
            if (target.Selection != null)
            {
                // Check whether there is text selected or just sitting at cursor
                if (target.Selection.IsEmpty)
                {
                    // Check to see if we are at the start of the textbox and nothing has been added yet
                    if (target.Selection.Start.Paragraph == null)
                    {
                        // Add a new paragraph object to the richtextbox with the fontsize
                        Paragraph p = new Paragraph();
                        p.FontSize = value;
                        target.Document.Blocks.Add(p);
                    }
                    else
                    {
                        // Get current position of cursor
                        TextPointer curCaret = target.CaretPosition;
                        // Get the current block object that the cursor is in
                        Block curBlock = target.Document.Blocks.Where
                            (x => x.ContentStart.CompareTo(curCaret) == -1 && x.ContentEnd.CompareTo(curCaret) == 1).FirstOrDefault();
                        if (curBlock != null)
                        {
                            Paragraph curParagraph = curBlock as Paragraph;
                            // Create a new run object with the fontsize, and add it to the current block
                            Run newRun = new Run();
                            newRun.FontSize = value;
                            curParagraph.Inlines.Add(newRun);
                            // Reset the cursor into the new block. 
                            // If we don't do this, the font size will default again when you start typing.
                            target.CaretPosition = newRun.ElementStart;
                        }
                    }
                }
                else // There is selected text, so change the fontsize of the selection
                {
                    TextRange selectionTextRange = new TextRange(target.Selection.Start, target.Selection.End);
                    selectionTextRange.ApplyPropertyValue(TextElement.FontSizeProperty, value);
                }
            }
            // Reset the focus onto the richtextbox after selecting the font in a toolbar etc
            target.Focus();
        }

        public TextNotePlus()
        {
            RegisterAllPorts();
            ArgumentLacing = LacingStrategy.Disabled;

            ShowHideRow = new RelayCommand(OnShowHideRow);
        }

        private bool _canExecuteMyCommand()
        {
            return true;
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
