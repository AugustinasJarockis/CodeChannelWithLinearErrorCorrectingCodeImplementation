using LinearCodeChannel.MathParts;
using LinearCodeChannel.UI.Pages;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LinearCodeChannel.UI
{
    internal class Navigation
    {
        private readonly Canvas _canvas;

        private Border contentFrame;

        private ByteChannelPage byteChannelPage;
        private ImageChannelPage imageChannelPage;
        private MatrixSettingPage matrixSettingPage;

        private Button byteChannelPageButton;
        private Button imageChannelPageButton;
        private Button matrixSettingPageButton;

        private readonly int buttonWidth = 200;
        public Navigation(Canvas canvas) {
            _canvas = canvas;

            GeneratingMatrix matrix = new();
            matrix.Matrix = [
                [1, 0, 1, 0, 1, 0],
                [0, 1, 0, 1, 0, 1],
                ];
            Encoder encoder = new(matrix);
            Channel channel = new();
            channel.errorChance = 0.05;
            Decoder decoder = new(matrix);

            byteChannelPage = new(encoder, decoder, channel);
            imageChannelPage = new(encoder, decoder, channel);
            matrixSettingPage = new(matrix);

            byteChannelPageButton = CreateButton("Bitų ir teksto kanalas", 200);
            imageChannelPageButton = CreateButton("Paveikslėlių kanalas", ((int)_canvas.ActualWidth - buttonWidth) / 2);
            matrixSettingPageButton = CreateButton("Matrica", (int)_canvas.ActualWidth  - buttonWidth - 200);

            byteChannelPageButton.Click += (s, e) => ActivatePanel(byteChannelPage);
            imageChannelPageButton.Click += (s, e) => ActivatePanel(imageChannelPage);
            matrixSettingPageButton.Click += (s, e) => ActivatePanel(matrixSettingPage); 

            contentFrame = new() {
                Width = _canvas.ActualWidth,
                Height = _canvas.ActualHeight * 0.9,
                BorderThickness = new Thickness(0.2),
                BorderBrush = Brushes.Black,
                Child = byteChannelPage.ContentCanvas
            };

            Canvas.SetTop(contentFrame, _canvas.ActualHeight * 0.1);
            _canvas.Children.Add(contentFrame);
        }

        private Button CreateButton(string text, int xPos) {
            Button btn = new() {
                Width = buttonWidth,
                Height = 30,
                Content = text
            };

            Canvas.SetTop(btn, 30);
            Canvas.SetLeft(btn, xPos);
            _canvas.Children.Add(btn);

            return btn;
        }

        private void ActivatePanel(IPanel panel) {
            contentFrame.Child = panel.ContentCanvas;
        }
    }
}
