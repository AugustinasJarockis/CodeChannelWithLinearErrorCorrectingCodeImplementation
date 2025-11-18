using LinearCodeChannel.UI;
using System.Windows;

namespace LinearCodeChannel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MainCanvas.Loaded += (s, e) => {
                MainCanvas.Dispatcher.BeginInvoke(() => {
                    Navigation navigation = new(MainCanvas);
                });
            };
        }
    }
}