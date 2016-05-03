using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Логика взаимодействия для AdminPanelView.xaml
    /// </summary>
    public partial class AdminPanelView : Window
    {
        public AdminPanelView()
        {
            InitializeComponent();
        }

        private void BtnGeneratePass_Click(object sender, RoutedEventArgs e)
        {
            TbPass.Text = GenerateNewPass(10);
        }

        private static string GenerateNewPass(int length)
        {
            Random rnd = new Random();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                sb.Append((char)rnd.Next(48, 120));
            }
            return sb.ToString();
        }

        private async void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            AdminWindow.IsEnabled = false;
            if (TbName.Text.Length < 3)
            {
                MessageBox.Show("Короткое имя");
                AdminWindow.IsEnabled = true;
                return;
            }
            if (TbLogin.Text.Length < 3)
            {
                MessageBox.Show("Короткий логин");
                AdminWindow.IsEnabled = true;
                return;
            }
            if (TbPass.Text.Length < 8)
            {
                MessageBox.Show("Короткий пароль");
                AdminWindow.IsEnabled = true;
                return;
            }
            if (!await Client.CreateUserAsync(TbName.Text, TbLogin.Text, TbPass.Text, CbAdmin?.IsChecked == true))
            {
                MessageBox.Show("Не удалось создать пользоватля." + "\nОшибка: " + Client.LastError, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("Пользователь создан.", "Результат создания пользователя",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            AdminWindow.IsEnabled = true;
        }

        private void BthMustChangePass_Click(object sender, RoutedEventArgs e)
        {
            if (!Client.MustChangeAllPass())
            {
                MessageBox.Show("Не удалось активировать политику." + "\nОшибка: " + Client.LastError, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show(
                    "Готово!\nПри следующем входе в систему все пользователи " +
                    "\nбудут оповещены о необходимости сменить пароль.",
                    "Результат установки политики",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void BtnApplyPassParams_Click(object sender, RoutedEventArgs e)
        {
            int minPassLength;
            if (!Int32.TryParse(TbMiPassLength.Text, out minPassLength))
            {
                MessageBox.Show("Неверная длина пароля." + "\nОшибка: " + Client.LastError, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            int passPolicy = 0;
            if (RbSimple.IsChecked == true)
            {
                passPolicy = 1;
            }
            if (RbPower.IsChecked == true)
            {
                passPolicy = 2;
            }
            if (RbPowerfull.IsChecked == true)
            {
                passPolicy = 3;
            }
            if (!await Client.SetPassPolicyAsync(minPassLength, passPolicy))
            {
                MessageBox.Show("Не удалось применить политику.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            MessageBox.Show("Политика паролей применена","Готово",
                    MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void AdminWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var passPolices = await Client.GetPassPolicyAsync();
            if (passPolices == null)
            {
                MessageBox.Show("Не удалось получить параметры политики паролей." + "\nОшибка: " + Client.LastError, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var policies = passPolices.Split('$');
            int minPassLength;
            if (!Int32.TryParse(policies[0], out minPassLength))
            {
                MessageBox.Show("Не удалось получить параметры политики паролей." + "\nОшибка: " + Client.LastError, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            int passPolicy;
            if (!Int32.TryParse(policies[1], out passPolicy))
            {
                MessageBox.Show("Не удалось получить параметры политики паролей." + "\nОшибка: " + Client.LastError, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            TbMiPassLength.Text = minPassLength.ToString();
            switch (passPolicy)
            {
                case 1:
                    RbSimple.IsChecked = true;
                    break;
                case 2:
                    RbPower.IsChecked = true;
                    break;
                case 3:
                    RbPowerfull.IsChecked = true;
                    break;
                default:
                    RbSimple.IsChecked = true;
                    break;
            }

        }
    }
}
