using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace LinearCodeChannel.UI.Elements
{
    internal class WritableStepBox
    {
        private readonly int _height;

        private Canvas boxCanvas;
        private RichTextBox box;
        private RichTextBox alternativeBox;
        private Border alternativeBorder;
        private TextChangedEventHandler? OnTextChange;
        private TextChangedEventHandler? OnAlternativeTextChange;

        public bool AllowStrings { get; set; } = false;
        
        public string Text {
            get {
                TextRange textRange = new TextRange(box.Document.ContentStart, box.Document.ContentEnd);
                return textRange.Text.TrimEnd('\r', '\n');
            }
            set {
                box.Document.Blocks.Clear();
                box.Document.Blocks.Add(new Paragraph(new Run(value)));
            }
        }

        public string AlternativeText {
            get {
                TextRange textRange = new TextRange(alternativeBox.Document.ContentStart, alternativeBox.Document.ContentEnd);
                return textRange.Text;
            }
            set {
                alternativeBox.Document.Blocks.Clear();
                alternativeBox.Document.Blocks.Add(new Paragraph(new Run(value)));
            }
        }

        public WritableStepBox(
            Canvas canvas, 
            string name, 
            int xPos, 
            int yPos, 
            int height, 
            int width,
            TextChangedEventHandler? OnTextChange = null, 
            TextChangedEventHandler? OnAlternativeTextChange = null, 
            int fontSize = 20, 
            bool OnlyBinary = false
            ) {

            this.OnTextChange = OnTextChange;
            this.OnAlternativeTextChange = OnAlternativeTextChange;
            AllowStrings = !OnlyBinary;
            _height = height;

            boxCanvas = new() {
                Width = width,
                Height = height + 30,
            };
            canvas.Children.Add(boxCanvas);
            Canvas.SetTop(boxCanvas, yPos);
            Canvas.SetLeft(boxCanvas, xPos);

            box = new() {
                Width = width,
                Height = height,
                FontSize = fontSize
            };

            alternativeBox = new() {
                Width = width,
                Height = height / 2,
                FontSize = fontSize
            };

            TextBlock blockName = new() {
                Text = name,
                Width = width,
                Height = 30,
                FontSize = fontSize,
            };

            var border = new Border {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0.2),
                Child = box
            };

            alternativeBorder = new () {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0.2),
                Child = alternativeBox
            };

            Canvas.SetTop(border, 40);
            Canvas.SetTop(alternativeBorder, height / 2 + 40);
            boxCanvas.Children.Add(blockName);
            boxCanvas.Children.Add(border);

            box.PreviewTextInput += (s, e) => {
                if (!AllowStrings && e.Text.Any(c => c != '0' && c != '1')) {
                    e.Handled = true;
                }
            };

            box.PreviewKeyDown += (s, e) => {
                if (!AllowStrings && (e.Key == Key.Space || e.Key == Key.Enter || e.Key == Key.Tab
                || e.Key == Key.Back && box.CaretPosition.GetTextInRun(LogicalDirection.Backward).Length > 0 && box.CaretPosition.GetTextInRun(LogicalDirection.Backward)[^1] == ' ')) {
                    e.Handled = true;
                }
            };

            alternativeBox.PreviewTextInput += (s, e) => {
                if (!AllowStrings && e.Text.Any(c => c != '0' && c != '1')) {
                    e.Handled = true;
                }
            };

            alternativeBox.PreviewKeyDown += (s, e) => {
                if (!AllowStrings && (e.Key == Key.Space || e.Key == Key.Enter || e.Key == Key.Tab
                )) {
                    e.Handled = true;
                }
            };

            if (OnTextChange != null)
                box.TextChanged += OnTextChange;
            
            if (OnAlternativeTextChange != null)
                alternativeBox.TextChanged += OnAlternativeTextChange;
        }

        public int SetTextAndApplyMistakeHighlight(string text, string originalString) => SetWithMistakeHighlight(box, text, originalString);

        public int SetAlternativeTextAndApplyMistakeHighlight(string text, string originalString) => SetWithMistakeHighlight(alternativeBox, text, originalString);

        private int SetWithMistakeHighlight(RichTextBox textBox, string text, string originalString) {
            int carretIndex = textBox.Document.ContentStart.GetOffsetToPosition(textBox.CaretPosition);
            textBox.Document.Blocks.Clear();
            int mistakeCount = 0;
            string normalString = "";
            var paragraph = new Paragraph();
            for (int i = 0; i < originalString.Length; i++) {
                if (text[i] == originalString[i]) {
                    normalString += text[i];
                }
                else {
                    mistakeCount++;
                    if (normalString != "") {
                        paragraph.Inlines.Add(new Run(normalString) { Foreground = Brushes.Black });
                        normalString = "";
                    }
                    paragraph.Inlines.Add(new Run(text[i].ToString()) { Foreground = Brushes.Red });
                }
            }
            if (normalString != "") {
                paragraph.Inlines.Add(new Run(normalString) { Foreground = Brushes.Black });
            }
            textBox.Document.Blocks.Add(paragraph);
            TextPointer start = textBox.Document.ContentStart;
            TextPointer newCaret = start.GetPositionAtOffset(carretIndex);
            if (newCaret != null)
                textBox.CaretPosition = newCaret;
            return mistakeCount;
        }

        public void SetOnTextChange(TextChangedEventHandler? NewOnTextChange) {
            if (OnTextChange != null)
                box.TextChanged -= OnTextChange;
            box.TextChanged += NewOnTextChange;
            OnTextChange = NewOnTextChange;
        }

        public void SetAlternativeOnTextChange(TextChangedEventHandler? NewOnAlternativeTextChange) {
            if (OnAlternativeTextChange != null)
                alternativeBox.TextChanged -= OnAlternativeTextChange;
            alternativeBox.TextChanged += NewOnAlternativeTextChange;
            OnAlternativeTextChange = NewOnAlternativeTextChange;
        }

        public void SetAlternativeContent(string alternativeContent) {
            box.Height = _height / 2;
            if (!boxCanvas.Children.Contains(alternativeBorder))
                boxCanvas.Children.Add(alternativeBorder);
            AlternativeText = alternativeContent;
        }

        public int SetAlternativeContentWithHighlight(string alternativeContent, string originalAlternativeContent) {
            box.Height = _height / 2;
            if (!boxCanvas.Children.Contains(alternativeBorder))
                boxCanvas.Children.Add(alternativeBorder);
            return SetAlternativeTextAndApplyMistakeHighlight(alternativeContent, originalAlternativeContent);
        }

        public void ClearAlternativeContent() {
            box.Height = _height;
            boxCanvas.Children.Remove(alternativeBorder);
            AlternativeText = "";
        }
    }
}
