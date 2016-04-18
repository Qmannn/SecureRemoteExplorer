using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Security.RightsManagement;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ExplorerClient.Core.Network;
using ExplorerClient.Gui.View;
using Microsoft.SqlServer.Server;

namespace ExplorerClient.Core
{
    public static class Client
    {

        #region members

        private static SslChannel _sslChannel = null;

        public static string Host { get; private set; }
        public static int Port { get; private set; }

        public static string Name { get; set; }
        public static string Login { get; set; }
        public static string Password { get; set; }

        public static bool Connected => _sslChannel?.Connected ?? false;
        public static bool IsAuthorized = false;
        public static string LastError { get; set; }

        #endregion

        #region Public

        public static void Connect(string host, int port)
        {
            _sslChannel = new SslChannel(host, port);
            _sslChannel.Disconnected += SslChannalOnDisconnected;
        }

        /// <summary>
        /// Асинхронное подключение к серверу
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static Task ConnectAsync(string host, int port)
        {
            return Task.Run(() =>
            {
                try
                {
                    Connect(host, port);
                }
                catch (Exception ex)
                {
                    LastError = ex.Message;
                    //ignored
                }
            });
        }

        public static void Authorization(string login, string pass)
        {
            string command = login + "$" + pass;
            _sslChannel.SendMessage(new Message(Commands.Login, command));
            var recivedCommand = _sslChannel.ReciveMessage();
            Name = recivedCommand.StringMessage;
            IsAuthorized = recivedCommand.Command == Commands.Ok;
        }

        /// <summary>
        /// Асинхронная авторизация
        /// </summary>
        /// <param name="login"></param>
        /// <param name="pass"></param>
        /// <returns></returns>
        public static Task AuthorizationAsync(string login, string pass)
        {
            return Task.Run(() =>
            {
                Authorization(login, pass);
            });
        }

        public static string[] GetUserState()
        {
            Message recivedMessage = null;
            try
            {
                _sslChannel.SendMessage(new Message(Commands.GetUserState, String.Empty));
                recivedMessage = _sslChannel.ReciveMessage();
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return null;
            }
            return recivedMessage?.StringMessage.Split('$');
;        }

        /// <summary>
        /// Асинхронное получение статистики пользователя
        /// </summary>
        /// <returns></returns>
        public static Task<string[]> GetUserStateAsync()
        {
            return Task.Run(() => GetUserState());
        }

        public static bool ChangePassword(string pass)
        {
            Message recivedMessage = null;
            try
            {
                _sslChannel.SendMessage(new Message(Commands.ChangePass, pass));
                recivedMessage = _sslChannel.ReciveMessage();
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
            if (recivedMessage?.Command == Commands.Error)
            {
                LastError = recivedMessage.StringMessage;
            }
            return recivedMessage?.Command == Commands.Ok;
        }

        /// <summary>
        /// Асинхронное изменение пароля
        /// </summary>
        /// <param name="pass">Новый пароль</param>
        /// <returns>Результат изменения</returns>
        public static Task<bool> ChangePasswordAsync(string pass)
        {
            return Task.Run(() => ChangePassword(pass));
        }

        public static bool SetShareStatus(bool allow)
        {
            Message recivedMessage;
            try
            {
                _sslChannel.SendMessage(new Message(Commands.SetShareStatus, allow.ToString()));
                recivedMessage = _sslChannel.ReciveMessage();
            }
            catch (Exception)
            {
                return false;
            }
            if (recivedMessage?.Command == Commands.Error)
            {
                LastError = recivedMessage.StringMessage;
            }
            return recivedMessage?.Command == Commands.Ok;
        }

        /// <summary>
        /// Асинхронное изменение разрешения на обмен файлами
        /// </summary>
        /// <param name="allow"></param>
        /// <returns></returns>
        public static Task<bool> SetShareStatusAsync(bool allow)
        {
            return Task.Run(() => SetShareStatus(allow));
        }

        public static bool PutCommonFile(string fileName)
        {
            try
            {
                _sslChannel.SendMessage(new Message(Commands.PutCommonFile, fileName));
                return _sslChannel.SendFile(fileName);
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Асинхронная загрузка файла на сервер
        /// </summary>
        /// <param name="fileName">Путь до файла</param>
        /// <returns>Результта отправки</returns>
        public static Task<bool> PutCommonFileAsync(string fileName)
        {
            return Task.Run(() => PutCommonFile(fileName));
        }

        public static bool GetCommonFile(string fileId, string fileName)
        {
            try
            {
                _sslChannel.SendMessage(new Message(Commands.GetCommonFile, fileId));
                _sslChannel.ReciveFile(fileName);
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Асинхронное получение файла из общей папки с сервера
        /// </summary>
        /// <param name="fileId">Идентификатор файла</param>
        /// <param name="fileName">Имя новго файла</param>
        /// <returns></returns>
        public static Task<bool> GetCommonFileAsync(string fileId, string fileName)
        {
            return Task.Run(() => GetCommonFile(fileId, fileName));
        }

        public static List<RemoteFile> GetCommonFileList()
        {
            lock (_sslChannel)
            {
                _sslChannel.SendMessage(new Message(Commands.GetCommonFileList, String.Empty));
                var recivedMessage = _sslChannel.ReciveMessage();
                var result = new List<RemoteFile>();
                while (recivedMessage.Command == Commands.GetCommonFileList)
                {
                    var fields = recivedMessage.StringMessage.Split('$');
                    if (fields.Length < 5)
                    {
                        continue;
                    }
                    result.Add(new RemoteFile()
                    {
                        Uuid = fields[0],
                        Name = fields[1],
                        Size = fields[2],
                        LoadTime = fields[3],
                        Owner = fields[4]
                    });
                    recivedMessage = _sslChannel.ReciveMessage();
                }
                return result;
            }
            
        }

        /// <summary>
        /// Асинхронное получение списка общих файлов с сервера
        /// </summary>
        /// <returns>Список файлов</returns>
        public static Task<List<RemoteFile>> GetCommonFileListAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    return GetCommonFileList();
                }
                catch (Exception ex)
                {
                    LastError = ex.Message;
                    return new List<RemoteFile>();
                }
            });
        }

        public static List<RemoteFile> GetPrivateFileList()
        {
            lock (_sslChannel)
            {
                _sslChannel.SendMessage(new Message(Commands.GetPrivateFileList, String.Empty));
                var recivedMessage = _sslChannel.ReciveMessage();
                var result = new List<RemoteFile>();
                while (recivedMessage.Command == Commands.GetPrivateFileList)
                {
                    var fields = recivedMessage.StringMessage.Split('$');
                    recivedMessage = _sslChannel.ReciveMessage();
                    if (fields.Length < 8)
                    {
                        continue;
                    }
                    result.Add(new RemoteFile()
                    {
                        Uuid = fields[0],
                        Name = fields[1],
                        Size = fields[2],
                        LoadTime = fields[3],
                        Owner = fields[4],
                        IsDamaged = fields[5] == true.ToString(),
                        IsEncrypted = fields[6] == true.ToString(),
                        LastDamageCheck = fields[7]
                    });
                }
                return result;
            }
        }

        /// <summary>
        /// Асинхронное получение списка приватных файлов с сервера
        /// </summary>
        /// <returns>Список файлов</returns>
        public static Task<List<RemoteFile>> GetPrivateFileListAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    return GetPrivateFileList();
                }
                catch (Exception ex)
                {
                    LastError = ex.Message;
                    return new List<RemoteFile>();
                }
            });
        }

        public static bool PutPrivateFile(string fileName, string key = null, bool mustEncrypt = true)
        {
            if (!File.Exists(fileName))
            {
                return false;
            }
            var stringMessage = fileName + "$" + (key ?? String.Empty) + "$" + mustEncrypt;
            _sslChannel.SendMessage(new Message(Commands.PutPrivateFile, stringMessage));
            if (_sslChannel.ReciveMessage().Command != Commands.Ok)
            {
                return false;
            }
            _sslChannel.SendFile(fileName);
            return _sslChannel.ReciveMessage().Command == Commands.Ok;
        }

        /// <summary>
        /// Асинхронная отправка файла на сервер
        /// </summary>
        /// <param name="fileName">Путь до файла</param>
        /// <param name="key">Клуч</param>
        /// <param name="mustEncrypt">Шифровать ли файл</param>
        /// <returns>Результат</returns>
        public static Task<bool> PutPrivateFileAsync(string fileName, string key = null, bool mustEncrypt = true)
        {
            return Task.Run(() => PutPrivateFile(fileName, key, mustEncrypt));
        }

        public static bool GetPrivateFile(string fileId, string fileName, string key = null)
        {
            var stringMessage = fileId + "$" + (key ?? String.Empty);
            _sslChannel.SendMessage(new Message(Commands.GetPrivateFile, stringMessage));
            var recived = _sslChannel.ReciveMessage();
            if (recived.Command != Commands.Ok)
            {
                LastError = recived.StringMessage;
                return false;
            }
            _sslChannel.ReciveFile(fileName);
            recived = _sslChannel.ReciveMessage();
            if (recived.Command == Commands.Ok)
                return true;
            LastError = recived.StringMessage;
            return false;
        }

        /// <summary>
        /// Асинхронная загрузка файла с сервера
        /// </summary>
        /// <param name="fileId">Идентификатор файла</param>
        /// <param name="fileName">Имя нового файла</param>
        /// <param name="key">Ключ шифрования (если файл зашифрован)</param>
        /// <returns>Результат</returns>
        public static Task<bool> GetPrivateFileAsync(string fileId, string fileName, string key = null)
        {
            return Task.Run(() => GetPrivateFile(fileId, fileName, key));
        }

        public static bool DeleteCommonFile(string fileId)
        {
            lock (_sslChannel)
            {
                _sslChannel.SendMessage(new Message(Commands.DeleteCommonFile, fileId));
                var recived = _sslChannel.ReciveMessage();
                if (recived.Command == Commands.Ok)
                {
                    return true;
                }
                LastError = recived.StringMessage;
            }
            return false;
        }

        public static bool DeletePrivateFile(string fileId)
        {
            lock (_sslChannel)
            {
                _sslChannel.SendMessage(new Message(Commands.DeletePrivateFile, fileId));
                var recived = _sslChannel.ReciveMessage();
                if (recived.Command == Commands.Ok)
                {
                    return true;
                }
                LastError = recived.StringMessage;
            }
            return false;
        }

        public static bool ChangeFileKey(string fileId, string oldKey, string newKey)
        {
            var message = fileId + "$" + oldKey + "$" + newKey;
            _sslChannel.SendMessage(new Message(Commands.ChangeFileKey, message));
            var recived = _sslChannel.ReciveMessage();
            if (recived.Command != Commands.Ok)
            {
                LastError = recived.StringMessage;
                return false;
            }
            return true;
        }

        public static bool CheckHashFile(string fileId)
        {
            _sslChannel.SendMessage(new Message(Commands.CheckFile, fileId));
            var recived = _sslChannel.ReciveMessage();
            LastError = recived.StringMessage;
            return recived.Command == Commands.Ok;
        }

        public static bool RecalcHash(string fileId)
        {
            _sslChannel.SendMessage(new Message(Commands.RecalcHash, fileId));
            var recived = _sslChannel.ReciveMessage();
            LastError = recived.StringMessage;
            return recived.Command == Commands.Ok;
        }

        public static bool CreateShareKey(string key)
        {
            _sslChannel.SendMessage(new Message(Commands.CreateShareKey, key));
            return _sslChannel.ReciveMessage().Command == Commands.Ok;
        }

        public static List<SentFile> GetNewFileList()
        {
            lock (_sslChannel)
            {
                _sslChannel.SendMessage(new Message(Commands.GetNewFileList, String.Empty));
                var recivedMessage = _sslChannel.ReciveMessage();
                var result = new List<SentFile>();
                while (recivedMessage.Command == Commands.GetNewFileList)
                {
                    var fields = recivedMessage.StringMessage.Split('$');
                    recivedMessage = _sslChannel.ReciveMessage();
                    if (fields.Length < 5)
                    {
                        continue;
                    }
                    result.Add(new SentFile()
                    {
                        From = fields[0],
                        Comment = fields[1],
                        SendTime = fields[2],
                        Name = fields[3],
                        Uuid = fields[4]
                    });
                }
                return result;
            }
        }

        public static List<SentFile> GetReportList()
        {
            lock (_sslChannel)
            {
                _sslChannel.SendMessage(new Message(Commands.GetReportList, String.Empty));
                var recivedMessage = _sslChannel.ReciveMessage();
                var result = new List<SentFile>();
                while (recivedMessage.Command == Commands.GetReportList)
                {
                    var fields = recivedMessage.StringMessage.Split('$');
                    recivedMessage = _sslChannel.ReciveMessage();
                    if (fields.Length < 6)
                    {
                        continue;
                    }
                    result.Add(new SentFile()
                    {
                        To = fields[0],
                        Comment = fields[1],
                        SendTime = fields[2],
                        Name = fields[3],
                        Uuid = fields[4],
                        Recived = fields[5]
                    });
                }
                return result;
            }
        }

        #region Events

        public delegate void OnDisconnectedDelegate();

        public static event OnDisconnectedDelegate OnDisconnected;

        #endregion#

        #region EventHandlers

        private static void SslChannalOnDisconnected()
        {
            IsAuthorized = false;
            OnDisconnected?.Invoke();
        }

        #endregion

        #endregion#

    }
}