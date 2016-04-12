using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
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
            LoginErrorGrid.Visibility = GetVisibility(true);
            WorkGrid.Visibility = GetVisibility(false);
        }

        #region Private

        Visibility GetVisibility(bool visible)
        {
            return visible ? Visibility.Visible : Visibility.Hidden;
        }

        void Login()
        {
            new LoginView().ShowDialog();
            WorkGrid.Visibility = GetVisibility(Client.IsAuthorized);
            LoginErrorGrid.Visibility = GetVisibility(!Client.IsAuthorized);
            LbName.Content = LbName.Content.ToString().Split(':').First() + ": " + Client.Name;
        }

        private async void SetUserState()
        {
            var state = await Client.GetUserStateAsync();
            if (state.Length < 5)
            {
                MessageBox.Show("Ошибка получения статистики хранилища", "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            LbUserFileCount.Content += state[0];
            LbControlFileCount.Content += state[1];
            LbSecureFileCount.Content += state[2];
            LbNewFileCount.Content += state[3];
            LbFreeSpace.Content += state[4] + " Мб.";
        }

        #endregion
        

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }

        /// <summary>
        /// Подгрузка пользовательской статистики
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WorkGrid_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetUserState();
        }
    }
}
