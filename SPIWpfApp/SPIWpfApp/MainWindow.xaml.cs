using SPIWpfApp.AppWnd;
using System.Windows;

namespace SPIWpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnRegisterValueEdit_Click(object sender, RoutedEventArgs e)
        {
            WndRegisterValue wndRegValue = new WndRegisterValue();
            wndRegValue.ShowDialog();
        }

        private void BtnAd4111Example_Click(object sender, RoutedEventArgs e)
        {
            WndAd4111Example wndSpiExample = new WndAd4111Example();
            wndSpiExample.ShowDialog();
        }
    }
}
