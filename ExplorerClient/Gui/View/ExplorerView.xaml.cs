using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using ExplorerClient.Core;

namespace ExplorerClient.Gui.View
{
    /// <summary>
    /// Логика взаимодействия для ExplorerView.xaml
    /// </summary>
    public partial class ExplorerView : Window
    {
        readonly List<RemoteFile> _privateFiles = new List<RemoteFile>();
        readonly List<RemoteFile> _commonFiles= new List<RemoteFile>();

        public ExplorerView()
        {
            InitializeComponent();
            LwCommonFiles.ItemsSource = _commonFiles;
            LvPrivateFiles.ItemsSource = _privateFiles;
        }

        private void BtnSaveToPrivate_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateCommonFiles();
            UpdatePrivateFiles();
        }

        private async void UpdateCommonFiles()
        {
            LwCommonFiles.ItemsSource = await Client.GetCommonFileListAsync();
        }

        private async void UpdatePrivateFiles()
        {
            LvPrivateFiles.ItemsSource = await Client.GetPrivateFileListAsync();
        }


        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void BtnUpdateCommon_Click(object sender, RoutedEventArgs e)
        {
            UpdateCommonFiles();
        }

        private void BtnUpdatePrivate_Click(object sender, RoutedEventArgs e)
        {
            UpdatePrivateFiles();
        }
    }
}
