using LinearCodeChannel.MathParts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LinearCodeChannel.UI.Elements
{
    internal class ErrorChanceControl
    {
        private readonly Canvas _canvas;
        private readonly Channel _channel;
        private TextBox errorChance;

        public ErrorChanceControl(Canvas canvas, Channel channel) {
            _canvas = canvas;
            _channel = channel;

            errorChance = new() {
                Text = _channel.errorChance.ToString(),
                Width = 200,
                Height = 30,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 20
            };

            errorChance.PreviewTextInput += (s, e) => {
                if (!errorChance.Text.StartsWith("0,") || e.Text.Any(c => c < '0' || c > '9'))
                    e.Handled = true;
            };

            errorChance.PreviewKeyDown += (s, e) => {
                if (e.Key == Key.Space || e.Key == Key.Enter || e.Key == Key.Tab
                || (errorChance.CaretIndex <= 2 && e.Key == Key.Back)) {
                    e.Handled = true;
                }
            };

            errorChance.TextChanged += (s, e) => {
                bool parseSuccess = double.TryParse(errorChance.Text, out var parsedValue);
                if (parseSuccess)
                    _channel.errorChance = parsedValue;
            };

            var border = new Border {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0.2),
                Child = errorChance
            };

            Canvas.SetTop(border, 70);
            Canvas.SetLeft(border, _canvas.ActualWidth / 2 - 100);
            _canvas.Children.Add(border);

            var label = new TextBlock() {
                Width = 200,
                Height = 30,
                Text = "Klaidos tikimybė",
                TextAlignment = TextAlignment.Center,
                FontSize = 20
            };

            Canvas.SetTop(label, 40);
            Canvas.SetLeft(label, _canvas.ActualWidth / 2 - 100);
            _canvas.Children.Add(label);
        }
    }
}
