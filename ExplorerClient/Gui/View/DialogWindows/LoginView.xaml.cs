using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ExplorerClient.Core;

namespace ExplorerClient.Gui.View
{
    /// <summary>
    /// Логика взаимодействия для LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            BtnLogin.IsEnabled = false;
            Regex reg = new Regex("[A-Za-z1-9_]");
            if (!reg.IsMatch(TbLogin.Text) || !reg.IsMatch(TbPass.Password))
            {
                MessageBox.Show("Поля логин или пароль заполнены неверно", "Неверно заполнены поля", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                BtnLogin.IsEnabled = true;
                return;
            }
            if (!Client.Connected)
            {
                await Client.ConnectAsync("192.168.0.103", 3000);
                if (!Client.Connected)
                {
                    MessageBox.Show("Не удалось подключиться к серверу\nПопробуйте позже", "Ошибка подключения", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                    BtnLogin.IsEnabled = true;
                    return;
                }
            }
            await Client.AuthorizationAsync(TbLogin.Text, TbPass.Password);

            if (Client.IsAuthorized)
            {
                Close();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль", "Ошибка авторизации", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                BtnLogin.IsEnabled = true;
            }
        }
    }
}
