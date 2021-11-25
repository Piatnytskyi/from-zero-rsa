using RSAApplication.ViewModel;
using System.Windows;

namespace RSAApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new RSAApplicationViewModel();
        }
    }
}
