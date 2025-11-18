using LinearCodeChannel.MathParts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LinearCodeChannel.UI.Pages
{
    internal class MatrixSettingPage : IPanel
    {
        private readonly GeneratingMatrix _matrix;

        private Button updateButton;
        private Button randomMatrixButton;
        private TextBox dimension;
        private TextBox codeLength;
        private List<List<byte>> newMatrix;
        private List<List<TextBox>> cellMatrix;
        private Grid matrixGrid;

        private Random random = new Random(DateTime.Now.Microsecond * DateTime.Now.Millisecond);
        public Canvas ContentCanvas { get; private set; }

        public MatrixSettingPage(GeneratingMatrix matrix) {
            _matrix = matrix;
            newMatrix = [.. matrix.Matrix.Select(innerList => new List<byte>(innerList))];

            ContentCanvas = new() {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Background = Brushes.White
            };

            ContentCanvas.Loaded += (s, e) => {
                ContentCanvas.Dispatcher.BeginInvoke(() => {
                    if (updateButton == null)
                        SetUpContent();
                });
            };
        }

        public void SetUpContent() {
            // Matricos atnaujinimo mygtukas
            updateButton = new Button() {
                Height = 30,
                Width = 150,
                Content = "Atnaujinti matricą"
            };

            Canvas.SetTop(updateButton, 100);
            Canvas.SetRight(updateButton, 100);

            updateButton.Click += (s, e) => { 
                _matrix.Matrix = newMatrix; 
            };

            ContentCanvas.Children.Add(updateButton);

            // Atsitiktinės matricos generavimo mygtukas
            randomMatrixButton = new Button() {
                Height = 30,
                Width = 150,
                Content = "Sugeneruoti atsitiktinę"
            };

            Canvas.SetTop(randomMatrixButton, 350);
            Canvas.SetRight(randomMatrixButton, 100);

            randomMatrixButton.Click += (s, e) => GenerateRandomMatrix();

            ContentCanvas.Children.Add(randomMatrixButton);

            // Kodo ilgio įvestis
            codeLength = new() {
                Text = _matrix.VectorSize.ToString(),
                Width = 150,
                Height = 30,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 20
            };

            codeLength.PreviewTextInput += (s, e) => {
                if (e.Text.Any(c => c < '0' || c > '9'))
                    e.Handled = true;
            };

            codeLength.PreviewKeyDown += (s, e) => {
                if (e.Key == Key.Space || e.Key == Key.Enter || e.Key == Key.Tab) {
                    e.Handled = true;
                }
            };

            codeLength.TextChanged += (s, e) => {
                bool parseSuccess = int.TryParse(codeLength.Text, out var parsedValue);
                if (parseSuccess && parsedValue > 0 && parsedValue >= newMatrix.Count)
                    UpdateNewMatrixCodeLength(parsedValue);
            };

            var border = new Border {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0.2),
                Child = codeLength
            };

            Canvas.SetTop(border, 200);
            Canvas.SetRight(border, 100);
            ContentCanvas.Children.Add(border);

            var label = new TextBlock() {
                Width = 150,
                Height = 30,
                Text = "Kodo ilgis",
                TextAlignment = TextAlignment.Center,
                FontSize = 20
            };

            Canvas.SetTop(label, 170);
            Canvas.SetRight(label, 100);
            ContentCanvas.Children.Add(label);

            // Kodo dimensijos įvestis
            dimension = new() {
                Text = _matrix.Dimension.ToString(),
                Width = 150,
                Height = 30,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 20
            };

            dimension.PreviewTextInput += (s, e) => {
                if (e.Text.Any(c => c < '0' || c > '9'))
                    e.Handled = true;
            };

            dimension.PreviewKeyDown += (s, e) => {
                if (e.Key == Key.Space || e.Key == Key.Enter || e.Key == Key.Tab) {
                    e.Handled = true;
                }
            };

            dimension.TextChanged += (s, e) => {
                bool parseSuccess = int.TryParse(dimension.Text, out var parsedValue);
                if (parseSuccess && parsedValue > 0 && parsedValue <= newMatrix[0].Count)
                    UpdateNewMatrixDimension(parsedValue);
            };

            var dimensionBorder = new Border {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0.2),
                Child = dimension
            };

            Canvas.SetTop(dimensionBorder, 280);
            Canvas.SetRight(dimensionBorder, 100);
            ContentCanvas.Children.Add(dimensionBorder);

            var dimensionLabel = new TextBlock() {
                Width = 150,
                Height = 30,
                Text = "Kodo dimensija",
                TextAlignment = TextAlignment.Center,
                FontSize = 20
            };

            Canvas.SetTop(dimensionLabel, 250);
            Canvas.SetRight(dimensionLabel, 100);
            ContentCanvas.Children.Add(dimensionLabel);

            // Vaizduojama matrica
            matrixGrid = new Grid();

            var scrollViewer = new ScrollViewer {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(5),
                Content = matrixGrid
            };

            for (int i = 0; i < _matrix.Dimension; i++) {
                matrixGrid.RowDefinitions.Add(new() { Height = new GridLength(50) });
            }
            for (int i = 0; i < _matrix.VectorSize; i++) {
                matrixGrid.ColumnDefinitions.Add(new() { Width = new GridLength(50) });
            }

            cellMatrix = new();
            for (int i = 0; i < _matrix.Dimension; i++) {
                cellMatrix.Add([]);
                for (int i2 = 0; i2 < _matrix.VectorSize; i2++) {
                    var cell = CreateMatrixCell(_matrix.Matrix[i][i2], i, i2);
                    cellMatrix[i].Add(cell);
                    matrixGrid.Children.Add(cell);
                }
            }

            var matrixBorder = new Border {
                Width = 1150,
                Height = 600,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0.2),
                Child = scrollViewer
            };

            Canvas.SetTop(matrixBorder, 50);
            Canvas.SetLeft(matrixBorder, 50);

            ContentCanvas.Children.Add(matrixBorder);
        }

        private TextBox CreateMatrixCell(int value, int row, int column) {
            var cellText = new TextBox() {
                Text = value.ToString(),
                Width = 50,
                Height = 50,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 20,
                BorderBrush = Brushes.LightGray,
            };

            cellText.PreviewTextInput += (s, e) => {
                if (e.Text[0] == '0' || e.Text[0] == '1') {
                    cellText.Text = e.Text[0].ToString();
                }
                e.Handled = true;
            };

            cellText.PreviewKeyDown += (s, e) => {
                if (e.Key == Key.Space || e.Key == Key.Enter || e.Key == Key.Tab
                 || e.Key == Key.Back || e.Key == Key.Delete) {
                    e.Handled = true;
                }
            };

            Grid.SetRow(cellText, row);
            Grid.SetColumn(cellText, column);

            cellText.TextChanged += (s, e) => {
                newMatrix[row][column] = byte.Parse(cellText.Text);
            };

            return cellText;
        }

        private void AddCellRows(int count) {
            for (int i = 0; i < count; i++) {
                cellMatrix.Add([]);
                matrixGrid.RowDefinitions.Add(new() { Height = new GridLength(50) });

                for (int i2 = 0; i2 < newMatrix[0].Count; i2++) {
                    var cell = CreateMatrixCell(0, newMatrix.Count + i, i2);
                    cellMatrix[cellMatrix.Count - 1].Add(cell);

                    matrixGrid.Children.Add(cell);
                }
            }
        }

        private void RemoveCellRows(int count) {
            for (int i = 0; i < count; i++) {
                for (int i2 = 0; i2 < newMatrix[0].Count; i2++) {
                    matrixGrid.Children.Remove(cellMatrix[cellMatrix.Count - 1][i2]);
                }
                cellMatrix.RemoveAt(cellMatrix.Count - 1);
                matrixGrid.RowDefinitions.RemoveAt(matrixGrid.RowDefinitions.Count - 1);
            }
        }

        private void UpdateNewMatrixDimension(int newDimension) {
            if (newDimension < newMatrix.Count) {
                RemoveCellRows(newMatrix.Count - newDimension);
                newMatrix.RemoveRange(newDimension, newMatrix.Count - newDimension);
            }
            else if (newDimension > newMatrix.Count) {
                AddCellRows(newDimension - newMatrix.Count);

                while (newDimension != newMatrix.Count) {
                    newMatrix.Add([.. new byte[newMatrix[0].Count]]);
                }
            }
        }

        private void AddCellColumns(int count) {
            for (int i2 = 0; i2 < count; i2++) {
                matrixGrid.ColumnDefinitions.Add(new() { Width = new GridLength(50) });
            }
            for (int i = 0; i < newMatrix.Count; i++) {
                for (int i2 = 0; i2 < count; i2++) {
                    var cell = CreateMatrixCell(0, i, cellMatrix[i].Count);
                    cellMatrix[i].Add(cell);

                    matrixGrid.Children.Add(cell);
                }
            }
        }

        private void RemoveCellColumns(int count) {
            for (int i = 0; i < newMatrix.Count; i++) {
                for (int i2 = 0; i2 < count; i2++) {
                    matrixGrid.Children.Remove(cellMatrix[i][cellMatrix[i].Count - 1]);
                    cellMatrix[i].RemoveAt(cellMatrix[i].Count - 1);
                }
            }
            for (int i2 = 0; i2 < count; i2++) {
                matrixGrid.ColumnDefinitions.RemoveAt(matrixGrid.ColumnDefinitions.Count - 1);
            }
        }

        private void UpdateNewMatrixCodeLength(int newCodeLength) {
            if (newCodeLength < newMatrix[0].Count) {
                RemoveCellColumns(newMatrix[0].Count - newCodeLength);
                foreach (var line in newMatrix) {
                    line.RemoveRange(newCodeLength, line.Count - newCodeLength);
                }
            }
            else if (newCodeLength > newMatrix[0].Count) {
                AddCellColumns(newCodeLength - newMatrix[0].Count);

                foreach (var line in newMatrix) {
                    while (newCodeLength != line.Count) {
                        line.Add(0);
                    }
                }
            }
        }

        private void GenerateRandomMatrix() {
            for (int i = 0; i < newMatrix.Count; i++) {
                for (int i2 = 0; i2 <  newMatrix.Count; i2++) {
                    newMatrix[i][i2] = (byte)(i == i2 ? 1 : 0);
                    cellMatrix[i][i2].Text = newMatrix[i][i2].ToString();
                }
            }

            for (int i = 0; i < newMatrix.Count; i++) {
                for (int i2 = newMatrix.Count; i2 < newMatrix[i].Count; i2++) {
                    newMatrix[i][i2] = (byte)random.Next(2);
                    cellMatrix[i][i2].Text = newMatrix[i][i2].ToString();
                }
            }
        }
    }
}
