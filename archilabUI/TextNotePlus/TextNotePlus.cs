using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xml;
using Autodesk.DesignScript.Runtime;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using GalaSoft.MvvmLight.Command;
using ProtoCore.AST.AssociativeAST;
using RichTextBox = Xceed.Wpf.Toolkit.RichTextBox;

namespace archilabUI.TextNotePlus
{
    [NodeName("Text Note Plus")]
    [NodeCategory("archilab.Core.Lists")]
    [NodeDescription("Use this node to create a resizable text note.")]
    [IsDesignScriptCompatible]
    public class TextNotePlus : NodeModel
    {
        public RichTextBox TextBox { get; set; }
        public Grid MainGrid { get; set; }
        public Cursor Cursor { get; set; }
        private Cursor _cursor;
        private const string DefaultText = "<Section xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" " +
                                           "xml:space=\"preserve\" TextAlignment=\"Left\" LineHeight=\"Auto\" IsHyphenationEnabled=\"False\" " +
                                           "xml:lang=\"en-us\" FlowDirection=\"LeftToRight\" NumberSubstitution.CultureSource=\"User\" " +
                                           "NumberSubstitution.Substitution=\"AsCulture\" FontFamily=\"Segoe UI\" FontStyle=\"Normal\" " +
                                           "FontWeight=\"Normal\" FontStretch=\"Normal\" FontSize=\"12\" Foreground=\"#FF000000\" " +
                                           "Typography.StandardLigatures=\"True\" Typography.ContextualLigatures=\"True\" " +
                                           "Typography.DiscretionaryLigatures=\"False\" Typography.HistoricalLigatures=\"False\" " +
                                           "Typography.AnnotationAlternates=\"0\" Typography.ContextualAlternates=\"True\" " +
                                           "Typography.HistoricalForms=\"False\" Typography.Kerning=\"True\" Typography.CapitalSpacing=\"False\" " +
                                           "Typography.CaseSensitiveForms=\"False\" Typography.StylisticSet1=\"False\" Typography.StylisticSet2=\"False\" " +
                                           "Typography.StylisticSet3=\"False\" Typography.StylisticSet4=\"False\" Typography.StylisticSet5=\"False\" " +
                                           "Typography.StylisticSet6=\"False\" Typography.StylisticSet7=\"False\" Typography.StylisticSet8=\"False\" " +
                                           "Typography.StylisticSet9=\"False\" Typography.StylisticSet10=\"False\" Typography.StylisticSet11=\"False\" " +
                                           "Typography.StylisticSet12=\"False\" Typography.StylisticSet13=\"False\" Typography.StylisticSet14=\"False\" " +
                                           "Typography.StylisticSet15=\"False\" Typography.StylisticSet16=\"False\" Typography.StylisticSet17=\"False\" " +
                                           "Typography.StylisticSet18=\"False\" Typography.StylisticSet19=\"False\" Typography.StylisticSet20=\"False\" " +
                                           "Typography.Fraction=\"Normal\" Typography.SlashedZero=\"False\" Typography.MathematicalGreek=\"False\" " +
                                           "Typography.EastAsianExpertForms=\"False\" Typography.Variants=\"Normal\" Typography.Capitals=\"Normal\" " +
                                           "Typography.NumeralStyle=\"Normal\" Typography.NumeralAlignment=\"Normal\" Typography.EastAsianWidths=\"Normal\" " +
                                           "Typography.EastAsianLanguage=\"Normal\" Typography.StandardSwashes=\"0\" Typography.ContextualSwashes=\"0\"" +
                                           " Typography.StylisticAlternates=\"0\">This is the <Run FontWeight=\"Bold\">RichTextBox";

        public string Notes { get; set; } = "Note...";
        public int NoteWidth { get; set; } = 225;
        public int NoteHeight { get; set; } = 90;
        public ObservableCollection<string> TextSizes { get; set; } = new ObservableCollection<string>
        {
            "8","9","10","11","12","14","16","18","20","22","24","26","28","36","48","72"
        };

        public RelayCommand ShowHideRow { get; set; }
        public RelayCommand<DragDeltaEventArgs> ResizeThumbDragDelta { get; set; }
        public RelayCommand ResizeThumbDragStarted { get; set; }
        public RelayCommand ResizeThumbDragCompleted { get; set; }

        private string _selectedTextSize = "11";
        public string SelectedTextSize {
            get { return _selectedTextSize; }
            set
            {
                _selectedTextSize = value;
                RaisePropertyChanged("SelectedTextSize");
                OnTextSizeChanged(value);
            }
        }

        private bool _isHiddenRow;
        public bool IsHiddenRow {
            get { return _isHiddenRow; }
            set { _isHiddenRow = value; RaisePropertyChanged("IsHiddenRow"); }
        }

        private void OnShowHideRow()
        {
            NoteHeight = NoteHeight + 30;
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
                        var p = new Paragraph {FontSize = value};
                        target.Document.Blocks.Add(p);
                    }
                    else
                    {
                        // Get current position of cursor
                        var curCaret = target.CaretPosition;
                        // Get the current block object that the cursor is in
                        var curBlock = target.Document.Blocks.FirstOrDefault(x => x.ContentStart.CompareTo(curCaret) == -1 && x.ContentEnd.CompareTo(curCaret) == 1);
                        if (curBlock != null)
                        {
                            var curParagraph = curBlock as Paragraph;
                            // Create a new run object with the fontsize, and add it to the current block
                            var newRun = new Run {FontSize = value};
                            curParagraph?.Inlines.Add(newRun);
                            // Reset the cursor into the new block. 
                            // If we don't do this, the font size will default again when you start typing.
                            target.CaretPosition = newRun.ElementStart;
                        }
                    }
                }
                else // There is selected text, so change the fontsize of the selection
                {
                    var selectionTextRange = new TextRange(target.Selection.Start, target.Selection.End);
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
            ResizeThumbDragDelta = new RelayCommand<DragDeltaEventArgs>(OnResizeThumbDragDelta);
            ResizeThumbDragStarted = new RelayCommand(OnResizeThumbDragStarted);
            ResizeThumbDragCompleted = new RelayCommand(OnResizeThumbDragCompleted);
        }

        /// <summary>
        /// Reset cursor to before drag started.
        /// </summary>
        private void OnResizeThumbDragCompleted()
        {
            Cursor = _cursor;
        }

        /// <summary>
        /// Set cursor to resize mode.
        /// </summary>
        private void OnResizeThumbDragStarted()
        {
            _cursor = Cursor;
            Cursor = Cursors.SizeNWSE;
        }

        /// <summary>
        /// Resizes the grid when thumb is dragged.
        /// </summary>
        /// <param name="e">Thumb event arguments.</param>
        private void OnResizeThumbDragDelta(DragDeltaEventArgs e)
        {
            var yAdjust = MainGrid.Height + e.VerticalChange;
            var xAdjust = MainGrid.Width + e.HorizontalChange;

            //make sure not to resize to negative width or heigth            
            xAdjust = (MainGrid.ActualWidth + xAdjust) > MainGrid.MinWidth ? xAdjust : MainGrid.MinWidth;
            yAdjust = (MainGrid.ActualHeight + yAdjust) > MainGrid.MinHeight ? yAdjust : MainGrid.MinHeight;

            MainGrid.Width = xAdjust;
            MainGrid.Height = yAdjust;
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
