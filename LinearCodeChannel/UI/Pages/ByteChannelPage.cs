using LinearCodeChannel.MathParts;
using LinearCodeChannel.UI.Elements;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LinearCodeChannel.UI.Pages
{
    internal class ByteChannelPage : IPanel
    {
        private readonly Encoder _encoder;
        private readonly Decoder _decoder;
        private readonly Channel _channel;

        public Canvas ContentCanvas { get; private set; }
        
        private ErrorChanceControl errorChanceControl;
        private StepBox encodedBox;
        private StepBox resultBox;
        private WritableStepBox messageBox;
        private WritableStepBox passedThroughChannelBox;
        private TextBlock errorCount;
        private string errorCountText = "Klaidų skaičius: ";
        private TextBlock alternativeErrorCount;
        private string alternativeErrorCountText = "Nekoduotos žinutės klaidų skaičius: ";
        private CheckBox _encodeCheckbox;
        private CheckBox _stringModeCheckbox;
        private bool showUnencoded = false;
        private bool isStringMode = false;
        private int supressTextChangeEvent = 0;
        private int supressAlternativeTextChangeEvent = 0;

        private int originalMessageSize => messageBox.Text.Length * (isStringMode ? 8 : 1);
        public ByteChannelPage(Encoder encoder, Decoder decoder, Channel channel) {
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
            int boxOffset = 375;
            int boxHeight = 450;
            int boxWidth = 350;
            int startingBoxX = (int)ContentCanvas.ActualWidth / 2 - 2 * boxOffset + (int)(0.5 * (boxOffset - boxWidth));

            encodedBox = new(ContentCanvas, "Užkoduota žinutė", startingBoxX + boxOffset, boxY, boxHeight, boxWidth);
            resultBox = new(ContentCanvas, "Dekoduota žinutė", startingBoxX + 3 * boxOffset, boxY, boxHeight, boxWidth);
            messageBox = new(ContentCanvas, "Originali žinutė", startingBoxX, boxY, boxHeight, boxWidth, OnlyBinary: true);
            passedThroughChannelBox = new(ContentCanvas, "Žinutė po perėjimo per kanalą", startingBoxX + 2 * boxOffset, boxY, boxHeight, boxWidth,
                OnTextChange: (s, e) => {
                    if (supressTextChangeEvent > 0) {
                        supressTextChangeEvent--;
                        return;
                    }

                    List<byte> decodedMessage;
                    var bitsToDecode = passedThroughChannelBox.Text.ToBitRepresentation();
                    decodedMessage = _decoder.Decode(bitsToDecode, originalMessageSize);
                    resultBox.Text = isStringMode ? decodedMessage.ToCharString() : decodedMessage.ToStringRepresentation();

                    if (passedThroughChannelBox.Text.Length == encodedBox.Text.Length && passedThroughChannelBox.Text != "") {
                        supressTextChangeEvent = 2;
                        passedThroughChannelBox.SetTextAndApplyMistakeHighlight(passedThroughChannelBox.Text, encodedBox.Text);
                    }
                },
                OnAlternativeTextChange: (s, e) => {
                    List<byte> decodedMessage;
                    var bitsToDecode = passedThroughChannelBox.AlternativeText.ToBitRepresentation();
                    decodedMessage = bitsToDecode;
                    resultBox.SetAlternativeContent(isStringMode ? decodedMessage.ToCharString() : decodedMessage.ToStringRepresentation());
                },
                OnlyBinary: true);

            messageBox.SetOnTextChange((s, e) => UpdateChannelFields());

            errorCount = new() {
                Text = errorCountText + "0",
                Width = 350,
                Height = 50,
                FontSize = 20
            };
            ContentCanvas.Children.Add(errorCount);

            alternativeErrorCount = new() {
                Text = alternativeErrorCountText + "0",
                Width = 350,
                Height = 50,
                FontSize = 20,
                Visibility = Visibility.Hidden,
            };
            ContentCanvas.Children.Add(alternativeErrorCount);

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

            _encodeCheckbox.Checked += (s, e) => { showUnencoded = true; alternativeErrorCount.Visibility = Visibility.Visible; UpdateChannelFields(); };
            _encodeCheckbox.Unchecked += (s, e) => { showUnencoded = false; alternativeErrorCount.Visibility = Visibility.Hidden; UpdateChannelFields(); };

            ContentCanvas.Children.Add(_encodeCheckbox);

            _stringModeCheckbox = new() {
                Content = new TextBlock {
                    Text = "Leisti tekstines eilutes kaip įeitį",
                    Foreground = Brushes.Black,
                    FontSize = 16
                },
                IsChecked = isStringMode,
                Width = 300,
                Height = 30
            };

            _stringModeCheckbox.Checked += (s, e) => SetStringMode(true);
            _stringModeCheckbox.Unchecked += (s, e) => SetStringMode(false);

            ContentCanvas.Children.Add(_stringModeCheckbox);

            SetElementPlaces();
        }

        private void SetElementPlaces() {
            Canvas.SetTop(errorCount, 30);
            Canvas.SetRight(errorCount, 100);

            Canvas.SetTop(alternativeErrorCount, 80);
            Canvas.SetRight(alternativeErrorCount, 100);

            Canvas.SetTop(_encodeCheckbox, 650);
            Canvas.SetLeft(_encodeCheckbox, 500);

            Canvas.SetTop(_stringModeCheckbox, 650);
            Canvas.SetLeft(_stringModeCheckbox, 750);
        }

        private void SetStringMode(bool value) {
            messageBox.Text = "";
            isStringMode = value;
            messageBox.AllowStrings = value;
            UpdateChannelFields();
        }

        private void UpdateChannelFields() {
            var convertedMessage = isStringMode ? messageBox.Text.ToBits() : messageBox.Text.ToBitRepresentation();
            var encodedMessage = _encoder.Encode(convertedMessage);
            encodedBox.Text = encodedMessage.ToStringRepresentation();
            var passedMessage = _channel.PassMessage(encodedMessage);
            int mistakeCount = passedThroughChannelBox.SetTextAndApplyMistakeHighlight(passedMessage.ToStringRepresentation(), encodedBox.Text);
            errorCount.Text = errorCountText + mistakeCount.ToString();
            var decodedMessage = _decoder.Decode(passedMessage, originalMessageSize);
            resultBox.Text = isStringMode ? decodedMessage.ToCharString() : decodedMessage.ToStringRepresentation();

            if (showUnencoded) {
                var convertedMessageString = convertedMessage.ToStringRepresentation();
                encodedBox.SetAlternativeContent(convertedMessageString);
                var unencodedPassedMessage = _channel.PassMessage(convertedMessage);
                int alternativeMistakeCount = passedThroughChannelBox.SetAlternativeContentWithHighlight(unencodedPassedMessage.ToStringRepresentation(), convertedMessageString);
                alternativeErrorCount.Text = alternativeErrorCountText + alternativeMistakeCount.ToString();
                resultBox.SetAlternativeContent(isStringMode ? unencodedPassedMessage.ToCharString() : unencodedPassedMessage.ToStringRepresentation());
            }
            else {
                alternativeErrorCount.Text = alternativeErrorCountText + 0;
                encodedBox.ClearAlternativeContent();
                passedThroughChannelBox.ClearAlternativeContent();
                resultBox.ClearAlternativeContent();
            }
        }
    }
}
