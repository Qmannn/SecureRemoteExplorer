using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using ExplorerClient.Core;

namespace ExplorerClient.Gui.View.DialogWindows
{
    /// <summary>
    /// Логика взаимодействия для LoginView.xaml
    /// </summary>
    public partial class LoginView
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private string Host { get; set; } = "localhost";
        private int Port { get; set; } = 3000;

        private void ReadConfig()
        {
            if (!File.Exists("config.cfg"))
            {
                using (FileStream fs = new FileInfo("config.cfg").OpenWrite())
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.WriteLine("Host:");
                        sw.WriteLine("");
                        sw.WriteLine("Port:");
                        sw.WriteLine("");
                    }
                }
                return;
            }
            using (StreamReader sr = File.OpenText("config.cfg"))
            {
                sr.ReadLine();
                Host = sr.ReadLine();
                sr.ReadLine();
                int port;
                Int32.TryParse(sr.ReadLine(), out port);
                Port = port;
            }
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            BtnLogin.IsEnabled = false;
            ReadConfig();
            Regex reg = new Regex("[A-Za-z1-9_@$]+");
            if (!reg.IsMatch(TbLogin.Text) || !reg.IsMatch(TbPass.Password))
            {
                MessageBox.Show("Поля логин или пароль заполнены неверно", "Неверно заполнены поля", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                BtnLogin.IsEnabled = true;
                return;
            }
            if (!Client.Connected)
            {
                await Client.ConnectAsync(Host, Port);
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
