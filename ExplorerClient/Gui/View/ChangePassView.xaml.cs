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
    /// Логика взаимодействия для ChangePassView.xaml
    /// </summary>
    public partial class ChangePassView : Window
    {
        public ChangePassView()
        {
            InitializeComponent();
        }

        private async void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (PbxPass.Password != PbxRepPass.Password)
            {
                MessageBox.Show("Пароли должны совпадать", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MainGrid.IsEnabled = false;
            if (!await Client.ChangePasswordAsync(PbxPass.Password))
            {
                MessageBox.Show("Не удалось изменить пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                MainGrid.IsEnabled = true;
                return;
            }
            MessageBox.Show("Готово!", "", MessageBoxButton.OK, MessageBoxImage.Information);
            MainGrid.IsEnabled = true;
            StaticValueBox.PassBeenChanged = true;
            Close();
        }
    }
}
