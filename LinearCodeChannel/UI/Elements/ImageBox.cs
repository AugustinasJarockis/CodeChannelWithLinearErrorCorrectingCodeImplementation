using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LinearCodeChannel.UI.Elements
{
    internal class ImageBox
    {
        private readonly int _yPos;
        private readonly int? _alternativeYPos;
        private readonly Action? OnUpload;

        private Canvas boxCanvas;

        private Image image;
        public BitmapSource Image {
            get {
                return (BitmapSource)image.Source;
            }
            set {
                image.Source = value;
            } 
        }

        private Image alternativeImage;
        public BitmapSource AlternativeImage {
            get {
                return (BitmapSource)alternativeImage.Source;
            }
            set {
                alternativeImage.Source = value;
            }
        }
        private Border alternativeBorder;

        public ImageBox(
            Canvas canvas, 
            string name, 
            int xPos, int yPos, 
            int height, int width, 
            bool uploadable = false,
            Action? OnUpload = null,
            int? alternativeYPos = null,
            int fontSize = 20) {
            _yPos = yPos;
            _alternativeYPos = alternativeYPos;

            boxCanvas = new() {
                Width = width,
                Height = 2 * height + 30,
            };
            canvas.Children.Add(boxCanvas);
            Canvas.SetTop(boxCanvas, yPos);
            Canvas.SetLeft(boxCanvas, xPos);

            image = new() {
                Stretch = Stretch.Fill
            };

            alternativeImage = new() {
                Stretch = Stretch.Fill
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
                Width = width,
                Height = height,
                Child = image
            };

            alternativeBorder = new Border {
                BorderBrush = Brushes.Black,
                Width = width,
                Height = height,
                BorderThickness = new Thickness(0.2),
                Child = alternativeImage
            };

            Canvas.SetTop(border, 40);
            Canvas.SetTop(alternativeBorder, height + 40);
            boxCanvas.Children.Add(blockName);
            boxCanvas.Children.Add(border);

            if (uploadable) {
                this.OnUpload = OnUpload; 

                var uploadButton = new Button() {
                    Width = width * 0.6,
                    Height = 30,
                    Content = "Įkelti paveiklėlį"
                };

                uploadButton.Click += (s, e) => UploadImage();

                Canvas.SetTop(uploadButton, height + 55);
                Canvas.SetLeft(uploadButton, width * 0.2);
                boxCanvas.Children.Add(uploadButton);
            }
        }

        public void SetAlternativeContent(BitmapSource alternativeImage) {
            if (!boxCanvas.Children.Contains(alternativeBorder))
                boxCanvas.Children.Add(alternativeBorder);

            Canvas.SetTop(boxCanvas, _alternativeYPos ?? _yPos);

            this.alternativeImage.Source = alternativeImage;
        }

        public void ClearAlternativeContent() {
            boxCanvas.Children.Remove(alternativeBorder);

            Canvas.SetTop(boxCanvas, _yPos);

            alternativeImage.Source = null;
        }

        private void UploadImage() {
            image.Source = null;

            var dialog = new OpenFileDialog {
                Title = "Pasirink paveikslėlį",
                Filter = "Paveikslėliai|*.png;*.jpg;*.jpeg;*.bmp;"
            };

            if (dialog.ShowDialog() == true) {
                try {
                    var bitmap = new FormatConvertedBitmap(
                        new BitmapImage(new Uri(dialog.FileName, UriKind.Absolute)),
                        PixelFormats.Bgr24,
                        null,
                        0
                        );
                    image.Source = bitmap;

                    OnUpload?.Invoke();
                }
                catch { }
            }
        }
    }
}
