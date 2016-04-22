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

namespace ExplorerClient.Gui.View.DialogWindows
{
    /// <summary>
    /// Логика взаимодействия для GetKeyDialog.xaml
    /// </summary>
    public partial class GetKeyDialog : Window
    {

        public GetKeyDialog(string header = null)
        {
            InitializeComponent();
            if(header != null)
                Title = header;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (Password.Password.Length < 3)
            {
                MessageBox.Show("Короткий ключ");
                return;
            }
            StaticValueBox.Key = Password.Password;
            Close();
        }
    }
}
