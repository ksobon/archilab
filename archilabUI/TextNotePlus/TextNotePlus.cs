using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
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
    [NodeCategory("archilab.Core.Utilities")]
    [NodeDescription("Use this node to create a resizable text note.")]
    [IsDesignScriptCompatible]
    public class TextNotePlus : NodeModel
    {
        #region Properties

        public RichTextBox TextBox { get; set; }
        public Grid MainGrid { get; set; }
        private Cursor _cursor;
        public Cursor Cursor { get; set; }

        private const string DefaultValue =
                "{\\rtf1\\ansi\\ansicpg1252\\uc1\\htmautsp\\deff2{\\fonttbl{\\f0\\fcharset0 Times New Roman;}" +
                "{\\f2\\fcharset0 ../../Fonts/#Open Sans;}}" +
                "{\\colortbl\\red0\\green0\\blue0;\\red255\\green255\\blue255;}\\loch\\hich\\dbch\\pard\\plain\\ltrpar\\itap0" +
                "{\\lang1033\\fs18\\f2\\cf0 \\cf0\\ql{\\f2 {\\ltrch New Note Plus}\\li0\\ri0\\sa0\\sb0\\fi0\\ql\\par}\r\n}\r\n}";

        public string Notes { get; set; } = DefaultValue;
        public int NoteWidth { get; set; } = 225;
        public int NoteHeight { get; set; } = 90;
        public List<string> TextFonts { get; set; }
        public List<string> TextSizes { get; set; } = new List<string>
        {
            "8","9","10","11","12","14","16","18","20","22","24","26","28","36","48","72"
        };

        public RelayCommand ShowHideRow { get; set; }
        public RelayCommand<DragDeltaEventArgs> ResizeThumbDragDelta { get; set; }
        public RelayCommand ResizeThumbDragStarted { get; set; }
        public RelayCommand ResizeThumbDragCompleted { get; set; }
        public RelayCommand<bool> TextBold { get; set; }
        public RelayCommand<bool> TextItalic { get; set; }

        private string _selectedTextSize = "11";
        public string SelectedTextSize {
            get { return _selectedTextSize; }
            set
            {
                _selectedTextSize = value;
                RaisePropertyChanged("SelectedTextSize");
                OnTextFormattingChanged(value, FormattingAction.ChangeFontSize);
            }
        }

        private string _selectedTextFont = "Arial";
        public string SelectedTextFont
        {
            get { return _selectedTextFont; }
            set
            {
                _selectedTextFont = value;
                RaisePropertyChanged("SelectedTextFont");
                OnTextFormattingChanged(value, FormattingAction.ChangeFontFamily);
            }
        }

        private bool _isHiddenRow;
        public bool IsHiddenRow {
            get { return _isHiddenRow; }
            set { _isHiddenRow = value; RaisePropertyChanged("IsHiddenRow"); }
        }

        #endregion

        public TextNotePlus()
        {
            RegisterAllPorts();
            ArgumentLacing = LacingStrategy.Disabled;

            TextFonts = CollectInstalledFonts();

            ShowHideRow = new RelayCommand(OnShowHideRow);
            ResizeThumbDragDelta = new RelayCommand<DragDeltaEventArgs>(OnResizeThumbDragDelta);
            ResizeThumbDragStarted = new RelayCommand(OnResizeThumbDragStarted);
            ResizeThumbDragCompleted = new RelayCommand(OnResizeThumbDragCompleted);
            TextBold = new RelayCommand<bool>(OnTextBold);
            TextItalic = new RelayCommand<bool>(OnTextItalic);
        }

        #region UI Methods

        /// <summary>
        /// Changes text to Italic/Normal.
        /// </summary>
        private void OnTextItalic(bool isChecked)
        {
            OnTextFormattingChanged("do not use", FormattingAction.ChangeItalic, isChecked);
        }

        /// <summary>
        /// Changes text to Bold/Normal.
        /// </summary>
        private void OnTextBold(bool isChecked)
        {
            OnTextFormattingChanged("do not use", FormattingAction.ChangeBold, isChecked);
        }

        /// <summary>
        /// Toggles the formatting menu on/off.
        /// </summary>
        private void OnShowHideRow()
        {
            NoteHeight = NoteHeight + 30;
            IsHiddenRow = !IsHiddenRow;
        }

        /// <summary>
        /// Sets proper formatting to text in the RichTexBox.
        /// </summary>
        /// <param name="input">Property used by dropdowns passing through selected value.</param>
        /// <param name="action">Type of formatting to be applied to the text.</param>
        /// <param name="isChecked">Property used by button actions indicating whether toggled button is checked or not.</param>
        private void OnTextFormattingChanged(string input, FormattingAction action, bool isChecked = false)
        {
            if (TextBox == null) return;
            if (string.IsNullOrEmpty(input)) return;

            var target = TextBox;

            // Check whether there is text selected or just sitting at cursor
            if (target.Selection.IsEmpty)
            {
                // Check to see if we are at the start of the textbox and nothing has been added yet
                if (target.Selection.Start.Paragraph == null)
                {
                    // Add a new paragraph object to the richtextbox with the fontsize
                    var p = new Paragraph();

                    switch (action)
                    {
                        case FormattingAction.ChangeFontSize:
                            p.FontSize = double.Parse(input);
                            break;
                        case FormattingAction.ChangeFontFamily:
                            p.FontFamily = new FontFamily(input);
                            break;
                        case FormattingAction.ChangeBold:
                            p.FontWeight = isChecked ? FontWeights.Bold : FontWeights.Normal;
                            break;
                        case FormattingAction.ChangeItalic:
                            p.FontStyle = isChecked ? FontStyles.Italic : FontStyles.Normal;
                            break;
                    }

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
                        var newRun = new Run();

                        switch (action)
                        {
                            case FormattingAction.ChangeFontSize:
                                newRun.FontSize = double.Parse(input);
                                break;
                            case FormattingAction.ChangeFontFamily:
                                newRun.FontFamily = new FontFamily(input);
                                break;
                            case FormattingAction.ChangeBold:
                                newRun.FontWeight = isChecked ? FontWeights.Bold : FontWeights.Normal;
                                break;
                            case FormattingAction.ChangeItalic:
                                newRun.FontStyle = isChecked ? FontStyles.Italic : FontStyles.Normal;
                                break;
                        }

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

                switch (action)
                {
                    case FormattingAction.ChangeFontSize:
                        selectionTextRange.ApplyPropertyValue(TextElement.FontSizeProperty, double.Parse(input));
                        break;
                    case FormattingAction.ChangeFontFamily:
                        selectionTextRange.ApplyPropertyValue(TextElement.FontFamilyProperty, new FontFamily(input));
                        break;
                    case FormattingAction.ChangeBold:
                        selectionTextRange.ApplyPropertyValue(TextElement.FontWeightProperty,
                            isChecked ? FontWeights.Bold : FontWeights.Normal);
                        break;
                    case FormattingAction.ChangeItalic:
                        selectionTextRange.ApplyPropertyValue(TextElement.FontStyleProperty,
                            isChecked ? FontStyles.Italic : FontStyles.Normal);
                        break;
                }
            }
            // Reset the focus onto the richtextbox after selecting the font in a toolbar etc
            target.Focus();
        }

        /// <summary>
        /// Collects all installed fonts from user machine.
        /// </summary>
        /// <returns>List of installed font names.</returns>
        public List<string> CollectInstalledFonts()
        {
            var fonts = new List<string>();
            using (var fontsCollection = new InstalledFontCollection())
            {
                var fontFamilies = fontsCollection.Families;
                fonts.AddRange(fontFamilies.Select(font => font.Name));
            }
            return fonts;
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

        #endregion

        #region Dynamo Serialization

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

            const string defaultValue = "{\\rtf1\\ansi\\ansicpg1252\\uc1\\htmautsp\\deff2{\\fonttbl{\\f0\\fcharset0 Times New Roman;}" +
                                        "{\\f2\\fcharset0 ../../Fonts/#Open Sans;}}" +
                                        "{\\colortbl\\red0\\green0\\blue0;\\red255\\green255\\blue255;}\\loch\\hich\\dbch\\pard\\plain\\ltrpar\\itap0" +
                                        "{\\lang1033\\fs18\\f2\\cf0 \\cf0\\ql{\\f2 {\\ltrch New Note Plus}\\li0\\ri0\\sa0\\sb0\\fi0\\ql\\par}\r\n}\r\n}";

            var note = nodeElement.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Name == "notes")?.InnerText ?? defaultValue;
            var noteWidth = nodeElement.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Name == "noteWidth")?.InnerText ?? "225";
            var noteHeight = nodeElement.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Name == "noteHeight")?.InnerText ?? "60";

            Notes = note;
            NoteWidth = int.Parse(noteWidth);
            NoteHeight = int.Parse(noteHeight);
        }

        #endregion

        [IsVisibleInDynamoLibrary(false)]
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            return new[]
            {
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode())
            };
        }
    }

    /// <summary>
    /// Types of formatting actions that can be applied to RichTextBox.
    /// </summary>
    public enum FormattingAction
    {
        ChangeFontSize,
        ChangeFontFamily,
        ChangeBold,
        ChangeItalic
    }
}
