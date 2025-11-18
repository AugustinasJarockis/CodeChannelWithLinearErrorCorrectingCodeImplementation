using LinearCodeChannel.MathParts;
using LinearCodeChannel.UI.Elements;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LinearCodeChannel.UI.Pages
{
    internal class ImageChannelPage : IPanel
    {
        private readonly Encoder _encoder;
        private readonly Decoder _decoder;
        private readonly Channel _channel;
        public Canvas ContentCanvas { get; private set; }

        private ErrorChanceControl errorChanceControl;
        private ImageBox originalImageBox;
        private ImageBox resultBox;
        private CheckBox _encodeCheckbox;
        private bool showUnencoded = false;

        private int originalMessageSize => 
            originalImageBox.Image.PixelWidth 
            * originalImageBox.Image.PixelHeight
            * originalImageBox.Image.Format.BitsPerPixel;
        public ImageChannelPage(Encoder encoder, Decoder decoder, Channel channel) {
            _encoder = encoder;
            _decoder = decoder;
            _channel = channel;

            ContentCanvas = new() {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Background = Brushes.White
            };

            ContentCanvas.Loaded += (s, e) => {
                ContentCanvas.Dispatcher.BeginInvoke(() => {
                    if (errorChanceControl == null)
                        SetUpContent();
                });
            };
        }

        private void SetUpContent() {
            errorChanceControl = new(ContentCanvas, _channel);

            int boxY = 125;
            int altBoxY = 25;
            int boxOffset = 800;
            int boxHeight = 300;
            int boxWidth = 350;
            int startingBoxX = (int)ContentCanvas.ActualWidth / 2 - (int)(0.5 * (boxOffset + boxWidth));

            originalImageBox = new(ContentCanvas, "Originalus paveikslėlis", startingBoxX, boxY, boxHeight, boxWidth, uploadable: true, OnUpload: UpdateChannelFields);
            resultBox = new(ContentCanvas, "Atkoduotas paveikslėlis", startingBoxX + boxOffset, boxY, boxHeight, boxWidth, alternativeYPos: altBoxY);

            _encodeCheckbox = new() {
                Content = new TextBlock {
                    Text = "Rodyti be kodavimo",
                    Foreground = Brushes.Black,
                    FontSize = 16
                },
                IsChecked = showUnencoded,
                Width = 200,
                Height = 30
            };

            _encodeCheckbox.Checked += (s, e) => { showUnencoded = true; if (originalImageBox.Image != null) UpdateChannelFields(); };
            _encodeCheckbox.Unchecked += (s, e) => { showUnencoded = false; if (originalImageBox.Image != null) UpdateChannelFields(); };

            ContentCanvas.Children.Add(_encodeCheckbox);

            SetElementPlaces();
        }

        private void SetElementPlaces() {
            Canvas.SetTop(_encodeCheckbox, 650);
            Canvas.SetLeft(_encodeCheckbox, ContentCanvas.ActualWidth / 2 - 100);
        }

        private void UpdateChannelFields() {
            var convertedMessage = originalImageBox.Image.ToBitArray();
            var encodedMessage = _encoder.Encode(convertedMessage);
            var passedMessage = _channel.PassMessage(encodedMessage);
            var decodedMessage = _decoder.Decode(passedMessage, originalMessageSize);
            resultBox.Image = decodedMessage.ToImage(originalImageBox.Image);

            if (showUnencoded) {
                var unencodedPassedMessage = _channel.PassMessage(convertedMessage);
                resultBox.SetAlternativeContent(unencodedPassedMessage.ToImage(originalImageBox.Image));
            }
            else {
                resultBox.ClearAlternativeContent();
            }
        }
    }
}
