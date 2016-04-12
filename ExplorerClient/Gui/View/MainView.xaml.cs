using System.Windows;
using ExplorerClient.Core;

namespace ExplorerClient.Gui.View
{
    /// <summary>
    /// Логика взаимодействия для MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
            Client.Connect("localhost", 3000);
        }

        private void MainViewWindow_Loaded(object sender, RoutedEventArgs e)
        {
            new LoginView().ShowDialog();
        }
    }
}
