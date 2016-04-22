using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using ExplorerClient.Core;
using ExplorerClient.Core.Objects;
using ExplorerClient.Gui.View.DialogWindows;
using Microsoft.Win32;

namespace ExplorerClient.Gui.View
{
    /// <summary>
    /// Логика взаимодействия для ExplorerView.xaml
    /// </summary>
    public partial class ExplorerView : Window
    {
        readonly List<RemoteFile> _privateFiles = new List<RemoteFile>();
        readonly List<RemoteFile> _commonFiles= new List<RemoteFile>();

        OpenFileDialog _fileDialog = new OpenFileDialog();
        SaveFileDialog _saveFileDialog = new SaveFileDialog();

        public ExplorerView()
        {
            InitializeComponent();
            LvCommonFiles.ItemsSource = _commonFiles;
            LvPrivateFiles.ItemsSource = _privateFiles;
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateCommonFiles();
            UpdatePrivateFiles();
        }

        private async void UpdateCommonFiles()
        {
            SetWaitScreen(true);
            LvCommonFiles.ItemsSource = await Client.GetCommonFileListAsync();
            SetWaitScreen(false);
        }

        private async void UpdatePrivateFiles()
        {
            SetWaitScreen(true);
            LvPrivateFiles.ItemsSource = await Client.GetPrivateFileListAsync();
            SetWaitScreen(false);
        }

        private async void UpdateNewFiles()
        {
            SetWaitScreen(true);
            LvNewFiles.ItemsSource = await Client.GetNewFileListAsync();
            SetWaitScreen(false);
        }

        private async void UpdateReports()
        {
            SetWaitScreen(true);
            LvReports.ItemsSource = await Client.GetReportListAsync();
            SetWaitScreen(false);
        }

        private void BtnUpdateCommon_Click(object sender, RoutedEventArgs e)
        {
            UpdateCommonFiles();
        }

        private void BtnUpdatePrivate_Click(object sender, RoutedEventArgs e)
        {
            UpdatePrivateFiles();
        }

        private void BtnUpdateNewFiles_Click(object sender, RoutedEventArgs e)
        {
            UpdateNewFiles();
        }

        private void BtnUpdateReports_Click(object sender, RoutedEventArgs e)
        {
            UpdateReports();
        }

        private void SetWaitScreen(bool wait)
        {
            if (wait)
            {
                IsEnabled = false;
                WaitGrid.Visibility = Visibility.Visible;
            }
            else
            {
                IsEnabled = true;
                WaitGrid.Visibility = Visibility.Hidden;
            }
        }

        private async void BtnDownloadCommon_Click(object sender, RoutedEventArgs e)
        {
            if (LvCommonFiles.SelectedItems.Count == 0)
            {
                MessageBox.Show("Необходимо выбрать файл");
                return;
            }
            SetWaitScreen(true);
            string fileName = null;
            if (LvCommonFiles.Items.Count == 1)
            {
                _saveFileDialog.FileName = ((RemoteFile) LvCommonFiles.Items[0]).Name;
                _saveFileDialog.ShowDialog();
                fileName = _saveFileDialog.FileName;
            }
            foreach (RemoteFile file in LvCommonFiles.SelectedItems)
            {
                if (!await Client.GetCommonFileAsync(file.Uuid, file.Name))
                {
                    MessageBox.Show("Не удалось скачать файл. Имя: " + (fileName ?? file.Name));
                }
            }
            SetWaitScreen(false);
        }

        private async void BtnDeleteCommon_Click(object sender, RoutedEventArgs e)
        {
            if (LvCommonFiles.SelectedItems.Count == 0)
            {
                MessageBox.Show("Необходимо выбрать файл(ы)");
                return;
            }
            SetWaitScreen(true);
            foreach (RemoteFile file in LvCommonFiles.SelectedItems)
            {
                if (!await Client.DeleteCommonFileAsync(file.Uuid))
                {
                    MessageBox.Show("Не удалось удалить файл. Имя: " + file.Name + "\n Ошибка: " + Client.LastError);
                }
            }
            UpdateCommonFiles();
            SetWaitScreen(false);
        }

        private async void BtnUpload_Click(object sender, RoutedEventArgs e)
        {
            SetWaitScreen(true);
            _fileDialog.ShowDialog();
            var fileName = _fileDialog.FileName;
            if (!await Client.PutCommonFileAsync(fileName))
            {
                MessageBox.Show("Не удалось загрузить файл. " + "\n Ошибка: " + Client.LastError, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("Готово.", "Результат отправки файла",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            UpdateCommonFiles();
            SetWaitScreen(false);
        }

        private async void BtnDownloadPrivate_Click(object sender, RoutedEventArgs e)
        {
            if (LvPrivateFiles.SelectedItems.Count == 0)
            {
                MessageBox.Show("Необходимо выбрать файл");
                return;
            }
            SetWaitScreen(true);
            StaticValueBox.Key = null;
            new GetKeyDialog().ShowDialog();
            if (StaticValueBox.Key == null)
            {
                SetWaitScreen(false);
                return;
            }
            string fileName = null; 
            _saveFileDialog.FileName = ((RemoteFile)LvPrivateFiles.SelectedItems[0]).Name;
            _saveFileDialog.ShowDialog();
            fileName = _saveFileDialog.FileName;
            if (!await Client.GetPrivateFileAsync(((RemoteFile)LvPrivateFiles.SelectedItems[0]).Uuid, fileName ?? ((RemoteFile)LvPrivateFiles.SelectedItems[0]).Name, StaticValueBox.Key))
            {
                MessageBox.Show("Не удалось скачать файл. Имя: " + fileName + "\nОшибка: " + Client.LastError);
            }
            else
            {
                MessageBox.Show("Готово.", "Результат загрузки файла",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            SetWaitScreen(false);
        }

        private async void BtnDeletePrivate_Click(object sender, RoutedEventArgs e)
        {
            if (LvPrivateFiles.SelectedItems.Count == 0)
            {
                MessageBox.Show("Необходимо выбрать файл");
                return;
            }
            SetWaitScreen(true);
            if (!await Client.DeletePrivateFileAsync(((RemoteFile)LvPrivateFiles.SelectedItems[0]).Uuid))
            {
                MessageBox.Show("Не удалось удалить файл. Имя: " + ((RemoteFile)LvPrivateFiles.SelectedItems[0]).Name + "\nОшибка: " + Client.LastError);
            }
            UpdatePrivateFiles();
            SetWaitScreen(false);
        }

        private async void BtnUploadPrivate_Click(object sender, RoutedEventArgs e)
        {
            SetWaitScreen(true);
            _fileDialog.ShowDialog();
            var fileName = _fileDialog.FileName;
            StaticValueBox.Key = null;
            new GetKeyDialog().ShowDialog();
            if (StaticValueBox.Key == null)
            {
                SetWaitScreen(false);
                return;
            }
            if (!await Client.PutPrivateFileAsync(fileName, StaticValueBox.Key))
            {
                MessageBox.Show("Не удалось загрузить файл. " + "\nОшибка: " + Client.LastError, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("Готово.", "Результат отправки файла",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            UpdatePrivateFiles();
            SetWaitScreen(false);
        }

        private async void BtnChKey_Click(object sender, RoutedEventArgs e)
        {
            if (LvPrivateFiles.SelectedItems.Count == 0)
            {
                MessageBox.Show("Необходимо выбрать файл");
                return;
            }
            StaticValueBox.Key = null;
            SetWaitScreen(true);
            new GetKeyDialog("Старый ключ").ShowDialog();
            if (StaticValueBox.Key == null)
            {
                SetWaitScreen(false);
                return;
            }
            var oldKey = StaticValueBox.Key;
            StaticValueBox.Key = null;
            new GetKeyDialog("Новый ключ").ShowDialog();
            if (StaticValueBox.Key == null)
            {
                SetWaitScreen(false);
                return;
            }
            var newKey = StaticValueBox.Key;
            if (!await Client.ChangeFileKeyAsync(((RemoteFile)LvPrivateFiles.SelectedItems[0]).Uuid, oldKey, newKey))
            {
                MessageBox.Show("Не удалось изменить ключ файла." + "\nОшибка: " + Client.LastError, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("Готово.", "Результат изменения ключа",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            SetWaitScreen(false);

        }

        private async void BtnRecalc_Click(object sender, RoutedEventArgs e)
        {
            if (LvPrivateFiles.SelectedItems.Count == 0)
            {
                MessageBox.Show("Необходимо выбрать файл");
                return;
            }
            SetWaitScreen(true);
            bool success = true;
            foreach (RemoteFile file in LvPrivateFiles.SelectedItems)
            {
                if (!await Client.RecalcHashAsync(file.Uuid))
                {
                    MessageBox.Show("Не удалось пересчитать контрольную сумму!", "Результат пересчета контрольной суммы",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    file.IsDamaged = true;
                    success = false;
                }
            }
            SetWaitScreen(false);
            if (success)
            {
                MessageBox.Show("Готово.", "Результат пересчета контрольной суммы",
                        MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                UpdatePrivateFiles();
            }
        }

        private async void BtnShare_Click(object sender, RoutedEventArgs e)
        {
            if (LvPrivateFiles.SelectedItems.Count == 0)
            {
                MessageBox.Show("Необходимо выбрать файл");
                return;
            }
            SetWaitScreen(true);
            StaticValueBox.Users = null;
            StaticValueBox.Key = null;
            StaticValueBox.Comment = null;
            new FileShareDialog().ShowDialog();
            if (StaticValueBox.Users == null || StaticValueBox.Key == null)
            {
                SetWaitScreen(false);
                return;
            }
            foreach (var user in StaticValueBox.Users)
            {
                if (!await
                        Client.ShareFileAsync(user.Uuid, ((RemoteFile) LvPrivateFiles.SelectedItems[0]).Uuid,
                            ((RemoteFile) LvPrivateFiles.SelectedItems[0]).Name, StaticValueBox.Key,
                            StaticValueBox.Comment ?? String.Empty))
                {
                    MessageBox.Show("Не удалось отправить файл пользователю " + user.Name + ".\nОшибка: " + Client.LastError,
                        "Результат отправки файла",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            MessageBox.Show("Готово.", "Результат отправки файла",
                        MessageBoxButton.OK, MessageBoxImage.Information);
            SetWaitScreen(false);
        }

        private async void BtnCheckFile_Click(object sender, RoutedEventArgs e)
        {
            if (LvPrivateFiles.SelectedItems.Count == 0)
            {
                MessageBox.Show("Необходимо выбрать файл");
                return;
            }
            SetWaitScreen(true);
            if (!await Client.CheckHashFileAsync(((RemoteFile)LvPrivateFiles.SelectedItems[0]).Uuid))
            {
                MessageBox.Show("Файл поврежден!", "Результат проверки контрльной суммы",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                ((RemoteFile)LvPrivateFiles.SelectedItems[0]).IsDamaged = true;
            }
            else
            {
                MessageBox.Show("Файл не поврежден.", "Результат проверки контрльной суммы",
                        MessageBoxButton.OK, MessageBoxImage.Information);
            }
            SetWaitScreen(false);
        }

        private async void BtnReciveFile_Click(object sender, RoutedEventArgs e)
        {
            if (LvNewFiles.SelectedItems.Count == 0)
            {
                MessageBox.Show("Необходимо выбрать файл");
                return;
            }
            SetWaitScreen(true);
            StaticValueBox.Key = null;
            new GetKeyDialog("Ключ обмена файлами").ShowDialog();
            if (StaticValueBox.Key == null)
            {
                SetWaitScreen(false);
                return;
            }
            string shareKey = StaticValueBox.Key;
            new GetKeyDialog("Новый ключ для файла").ShowDialog();
            if (StaticValueBox.Key == null)
            {
                SetWaitScreen(false);
                return;
            }
            string newKey = StaticValueBox.Key;
            if (!await Client.ReciveNewFileAsync(((SentFile) LvNewFiles.SelectedItems[0]).Uuid, shareKey, newKey))
            {
                MessageBox.Show("Не удалось принять файл.\nОшибка: " + Client.LastError,
                        "Результат принятия файла",
                        MessageBoxButton.OK, MessageBoxImage.Error);

            }
            else
            {
                MessageBox.Show("Готово.", "Результат принятия файла",
                        MessageBoxButton.OK, MessageBoxImage.Information);
            }
            UpdateNewFiles();
            SetWaitScreen(false);
        }

        private async void BtnDeleteNewFile_Click(object sender, RoutedEventArgs e)
        {
            if (LvNewFiles.SelectedItems.Count == 0)
            {
                MessageBox.Show("Необходимо выбрать файл");
                return;
            }
            SetWaitScreen(true);
            if (!await Client.DeleteNewFileAsync(((SentFile)LvNewFiles.SelectedItems[0]).Uuid))
            {
                MessageBox.Show("Не удалось удалить файл.\nОшибка: " + Client.LastError,
                        "Результат удаления файла",
                        MessageBoxButton.OK, MessageBoxImage.Error);

            }
            else
            {
                MessageBox.Show("Готово.", "Результат удаления файла",
                        MessageBoxButton.OK, MessageBoxImage.Information);
            }
            SetWaitScreen(false);
            UpdateNewFiles();
        }
    }
}
