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
using ExplorerClient.Core.Objects;

namespace ExplorerClient.Gui.View.DialogWindows
{
    /// <summary>
    /// Логика взаимодействия для GetUsersDialog.xaml
    /// </summary>
    public partial class FileShareDialog : Window
    {
        public FileShareDialog()
        {
            InitializeComponent();
        }

        private async void UserWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UserWindow.IsEnabled = false;
            LvUsers.ItemsSource = await Client.GetUserListAsync();
            UserWindow.IsEnabled = true;
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            if (LvUsers.SelectedItems.Count == 0)
            {
                MessageBox.Show("Выберите пользователя");
                return;
            }
            if (PbKey.Password.Length < 3)
            {
                MessageBox.Show("Короткий ключ!");
                return;
            }
            StaticValueBox.Key = PbKey.Password;
            StaticValueBox.Users = new List<User>();
            foreach (User user in LvUsers.SelectedItems)
            {
                StaticValueBox.Users.Add(user);
            }
            StaticValueBox.Comment = TbComment.Text;
            Close();
        }

        
    }
}
