using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using System.Windows.Documents;
using System.Windows.Media;
using ExplorerClient.Core;
using ExplorerClient.Gui.View.DialogWindows;

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
            new DialogWindows.LoginView().ShowDialog();
            WorkGrid.Visibility = GetVisibility(Client.IsAuthorized);
            LoginErrorGrid.Visibility = GetVisibility(!Client.IsAuthorized);
            Client.OnDisconnected += ClientOnOnDisconnected;
            LbName.Content = LbName.Content.ToString().Split(':').First() + ": " + Client.Name;
        }

        private void ClientOnOnDisconnected()
        {
            WorkGrid.Visibility = GetVisibility(Client.IsAuthorized);
            LoginErrorGrid.Visibility = GetVisibility(!Client.IsAuthorized);
        }

        private async void SetUserState()
        {
            var state = await Client.GetUserStateAsync();
            if (state?.Length < 2)
            {
                MessageBox.Show("Ошибка получения статистики хранилища", "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            if (state != null)
            {
                LbUserFileCount.Content = state[0];
                LbNewFileCountN.Content = state[1];
                if (state[1] != "0")
                {
                    LbNewFileCountN.Foreground = Brushes.Red;
                }
                else
                {
                    LbNewFileCountN.Foreground = Brushes.Black;
                }
            }
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

        private void BtnChangePass_Click(object sender, RoutedEventArgs e)
        {
            WorkGrid.IsEnabled = false;
            new ChangePassView().ShowDialog();
            WorkGrid.IsEnabled = true;
        }

        private async void CbAllowGetFiles_Checked(object sender, RoutedEventArgs e)
        {
            WorkGrid.IsEnabled = false;
            WaitingGrid.Visibility = Visibility.Visible;
            if (!await Client.SetShareStatusAsync(CbAllowGetFiles?.IsChecked == true))
            {
                WorkGrid.IsEnabled = true;
                WaitingGrid.Visibility = Visibility.Hidden;
                MessageBox.Show(Client.LastError, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                if (CbAllowGetFiles != null)
                    CbAllowGetFiles.IsChecked = !CbAllowGetFiles.IsChecked;
                return;
            }
            WorkGrid.IsEnabled = true;
            WaitingGrid.Visibility = Visibility.Hidden;
        }

        private void BtnOpenExplorer_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;
            new ExplorerView().ShowDialog();
            Visibility = Visibility.Visible;
        }

        private async void BtnCreateKeys_Click(object sender, RoutedEventArgs e)
        {
            StaticValueBox.Key = null;
            new GetKeyDialog("Новый ключ для обмена файлами").ShowDialog();
            if (StaticValueBox.Key == null)
            {
                MessageBox.Show("Создание ключа отменено");
                return;
            }
            var newShareKey = StaticValueBox.Key;
            StaticValueBox.Key = null;
            new GetKeyDialog("Повторите новый ключ для обмена файлами").ShowDialog();
            if (StaticValueBox.Key == null)
            {
                MessageBox.Show("Создание ключа отменено");
                return;
            }
            if (newShareKey != StaticValueBox.Key)
            {
                MessageBox.Show("Ключи не совпадают");
                return;
            }
            if (!await Client.CreateShareKeyAsync(newShareKey))
            {
                MessageBox.Show("Не удалось создать ключ.\nОшибка: " + Client.LastError,
                        "Результат создания ключа обмена файлами",
                        MessageBoxButton.OK, MessageBoxImage.Error);

            }
            else
            {
                MessageBox.Show("Готово.", "Результат создания ключа обмена файлами",
                        MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            Client.Logout();
        }
    }
}
