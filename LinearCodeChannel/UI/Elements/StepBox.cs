using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LinearCodeChannel.UI.Elements
{
    internal class StepBox {
        private readonly int _height;

        private Canvas boxCanvas;
        private TextBox box;
        private TextBox alternativeBox;
        private Border alternativeBorder;

        public string Text {
            get {
                return box.Text;
            }
            set {
                box.Text = value;
            }
        }

        public string AlternativeText {
            get {
                return alternativeBox.Text;
            }
            private set {
                alternativeBox.Text = value;
            }
        }

        public StepBox(Canvas canvas, string name, int xPos, int yPos, int height, int width, int fontSize = 20) {
            _height = height;

            boxCanvas = new() {
                Width = width,
                Height = height + 30,
            };
            canvas.Children.Add(boxCanvas);
            Canvas.SetTop(boxCanvas, yPos);
            Canvas.SetLeft(boxCanvas, xPos);

            box = new() {
                TextWrapping = TextWrapping.Wrap,
                Width = width,
                Height = height,
                FontSize = fontSize,
                IsReadOnly = true
            };

            alternativeBox = new() {
                TextWrapping = TextWrapping.Wrap,
                Width = width,
                Height = height / 2,
                FontSize = fontSize,
                IsReadOnly = true
            };

            TextBlock blockName = new () {
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

            alternativeBorder = new Border {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0.2),
                Child = alternativeBox
            };

            Canvas.SetTop(border, 40);
            Canvas.SetTop(alternativeBorder, height / 2 + 40);
            boxCanvas.Children.Add(blockName);
            boxCanvas.Children.Add(border);
        }

        public void SetAlternativeContent(string alternativeContent) {
            box.Height = _height / 2;
            if (!boxCanvas.Children.Contains(alternativeBorder))
                boxCanvas.Children.Add(alternativeBorder);
            alternativeBox.Text = alternativeContent;
        }

        public void ClearAlternativeContent() {
            box.Height = _height;
            boxCanvas.Children.Remove(alternativeBorder);
            alternativeBox.Text = "";
        }
    }
}
