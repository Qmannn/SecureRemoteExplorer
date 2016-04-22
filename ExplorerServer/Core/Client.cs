using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using ExplorerServer.Core.Cryptography;
using ExplorerServer.Core.DataBase;
using ExplorerServer.Core.Network;
using Npgsql;

namespace ExplorerServer.Core
{
    public class Client
    {
        private readonly TcpClient _client;
        private readonly X509Certificate _certificate;
        private readonly SslChannel _sslChannel;
        private Thread _clientThread;
        private readonly DbController _dbController;
        private string _userId;
        private string _login;
        private string _name;


        #region Public#

        public Client(TcpClient client, X509Certificate certificate, NpgsqlConnection dbConnection)
        {
            _client = client;
            _certificate = certificate;
            _sslChannel = new SslChannel(_client, _certificate);
            _dbController = new DbController(dbConnection);
        }

        private void Work()
        {
            Message recivedMessage;
            do
            {
                recivedMessage = _sslChannel.ReciveMessage();
            } while (MessageHandler(recivedMessage));
        }

        public void Start()
        {
            _clientThread = new Thread(Work);
            _clientThread.Start();
        }

        public void Stop()
        {
            if(_client.Connected)
                _client.Close();
        }

        #endregion#

        #region Private

        #region Comunicating with client

        private bool MessageHandler(Message message)
        {
            switch (message.Command)
            {
                case Commands.Login:
                    _sslChannel.SendMessage(new Message(Authorization(message) ? Commands.Ok : Commands.Error, _name));
                    break;
                case Commands.GetUserState:
                    _sslChannel.SendMessage(new Message(Commands.Ok, GetUserState()));
                    break;
                case Commands.Ok:
                    break;
                case Commands.Error:
                    Console.WriteLine("Клиент отключен. Логин: " + _login);
                    return false;
                case Commands.ChangePass:
                    _sslChannel.SendMessage(new Message(ChangePassword(message) ? Commands.Ok : Commands.Error, String.Empty));
                    break;
                case Commands.SetShareStatus:
                    bool setStatusResult = false;
                    try
                    {
                        setStatusResult = SetShareStatus(message);
                    }
                    catch (Exception ex)
                    {
                        _sslChannel.SendMessage(new Message(setStatusResult ? Commands.Ok : Commands.Error, ex.Message));
                        break;
                    }
                    _sslChannel.SendMessage(new Message(setStatusResult ? Commands.Ok : Commands.Error, String.Empty));
                    break;
                case Commands.PutCommonFile:
                    ReciveCommonFile(message);
                    break;
                case Commands.GetCommonFile:
                    SendCommonFile(message);
                    break;
                case Commands.PutPrivateFile:
                    RecivePrivateFile(message);
                    break;
                case Commands.GetPrivateFile:
                    SendPrivateFile(message);
                    break;
                case Commands.GetCommonFileList:
                    SendCommonFileList();
                    break;
                case Commands.GetPrivateFileList:
                    SendPrivateFileList();
                    break;
                case Commands.DeleteCommonFile:
                    DeleteCommonFile(message);
                    break;
                case Commands.DeletePrivateFile:
                    DeletePrivateFile(message);
                    break;
                case Commands.ChangeFileKey:
                    ChangeFileKey(message);
                    break;
                case Commands.CheckFile:
                    CheckFile(message);
                    break;
                case Commands.RecalcHash:
                    RecalcHash(message);
                    break;
                case Commands.SendFile:
                    break;
                case Commands.CreateShareKey:
                    SetNewRsaKeys(message);
                    break;
                case Commands.GetNewFileList:
                    SendNewFileList();
                    break;
                case Commands.GetReportList:
                    SendReportList();
                    break;
                case Commands.ReciveNewFile:
                    ReciveNewFile(message);
                    break;
                case Commands.ShareFile:
                    ShareFile(message);
                    break;
                case Commands.DeleteNewFile:
                    DeleteNewFile(message);
                    break;
                case Commands.GetUserList:
                    SendUserList();
                    break;
                default:
                    Console.WriteLine("Получена неизвестная команда");
                    return false;
            }
            return true;
        }
        private bool Authorization(Message loginMessage)
        {
            string login = loginMessage.StringMessage.Split('$').First();
            string pass = loginMessage.StringMessage.Split('$').Last();
            _login = login;
            var sha1 = SHA1.Create();
            pass = ByteToStringConverter(sha1.ComputeHash(StringToByteConverter(pass)));
            try
            {
                _name = _dbController.Authorization(login, pass);
                _userId = _dbController.GetUserId(login);
                return _name != null;
            }
            catch (Exception)
            {
                Console.WriteLine("Авторизация клиента не удалась. Логин: " + login);
            }

            return false;
        }

        private string GetUserState()
        {
            string result = String.Empty;
            result += _dbController.GetCountUserFiles() + "$";
            result += _dbController.GetCountControlFiles() + "$";
            result += _dbController.GetCountEncryptedFiles() + "$";
            result += _dbController.GetCountNewFiles() + "$";
            result += _dbController.GetFreeMemCount();
            return result;
        }

        private bool ChangePassword(Message passMessage)
        {
            if (!CheckPassPolicy(passMessage.StringMessage))
            {
                return false;
            }

            var sha1 = SHA1.Create();

            var pass = ByteToStringConverter(sha1.ComputeHash(StringToByteConverter(passMessage.StringMessage)));
            if (!_dbController.SetNewPassword(pass))
            {
                return false;
            }
            return true;
        }

        private bool SetShareStatus(Message message)
        {
            var newStatus = message.StringMessage == true.ToString();
            if (newStatus && _dbController.GetPublicKeyPath(_userId) == null)
            {
                throw new Exception("Ключ для обмена файлами не создан");
            }
            return _dbController.SetShareStatus(newStatus);
        }

        private bool ReciveCommonFile(Message message)
        {
            string fileName = CreateCommonFileName(message.StringMessage);
            SendResult(true);
            if (_sslChannel.ReciveFile(fileName))
            {
                var fileSize = new FileInfo(fileName).Length.ToString();
                if (!_dbController.SaveNewCommonFile(message.StringMessage, fileName, fileSize))
                {
                    return true;
                }
            }
            return false;
        }

        private void SendCommonFile(Message message)
        {
            string fileId = message.StringMessage;
            string filePath = _dbController.GetCommonFilePath(fileId);
            if (!File.Exists(filePath))
            {
                SendResult(false, "Файл поврежден или не найден");
                Console.WriteLine("Файл не найден в файловой системе. Path: " + filePath);
                return;
            }
            SendResult(true);
            _sslChannel.SendFile(filePath);
            return;
        }

        private void SendResult(bool result, string message = null)
        {
            _sslChannel.SendMessage(new Message(result ? Commands.Ok : Commands.Error, message ?? String.Empty));
        }

        private void SendCommonFileList()
        {
            var commonFiles = _dbController.GetCommonFileList();
            foreach (var file in commonFiles)
            {
                _sslChannel.SendMessage(new Message(Commands.GetCommonFileList, file));
            }
            //Сообщение ERROR отправляется как флаг окончания списка
            _sslChannel.SendMessage(new Message(Commands.Error, String.Empty));
        }

        private void SendPrivateFileList()
        {
            var privateFiles = _dbController.GetPrivateFileList();
            foreach (var file in privateFiles)
            {
                _sslChannel.SendMessage(new Message(Commands.GetPrivateFileList, file));
            }
            //Сообщение ERROR отправляется как флаг окончания списка
            _sslChannel.SendMessage(new Message(Commands.Error, String.Empty));
        }

        private void RecivePrivateFile(Message message)
        {
            var fields = message.StringMessage.Split('$');
            if (fields.Length < 3)
            {
                SendResult(false);
                return;
            }
            SendResult(true);
            var fileName = CreatePrivateFileName(fields[0]);
            string fileHash;
            SHA1 hashCalc = SHA1.Create();
            if (!((fields[2] == true.ToString())
                    ? _sslChannel.ReciveEncryptedFile(fileName, fields[1])
                    : _sslChannel.ReciveFile(fileName)))
            {
                File.Delete(fileName);
                SendResult(false, "Ошибка получения файла");
                return;
            }
            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                fileHash = ByteToStringConverter(hashCalc.ComputeHash(fs));
            }
            string keyHash = ByteToStringConverter(hashCalc.ComputeHash(StringToByteConverter(fields[1])));
            string fileSize = new FileInfo(fileName).Length.ToString();
            if (!_dbController.SaveNewPrivateFile(fields[0], fileName, fileSize, fileHash, fields[2] == true.ToString(),
                keyHash))
            {
                File.Delete(fileName);
                SendResult(false, "Ошибка записи в базу данных");
                return;
            }
            SendResult(true);
        }

        private void SendPrivateFile(Message message)
        {
            var fileId = message.StringMessage.Split('$').First();
            var fileKey = message.StringMessage.Split('$').Last();
            SHA1 hashCalc = SHA1.Create();
            var keyHash = ByteToStringConverter(hashCalc.ComputeHash(StringToByteConverter(fileKey)));
            var filePath = _dbController.GetPrivateFilePath(fileId, keyHash);
            if (filePath == null)
            {
                SendResult(false, "Неверный ключ!");
                return;
            }
            SendResult(true);
            _sslChannel.SendEncryptedFile(filePath, fileKey);
            SendResult(true);
        }

        private void DeleteCommonFile(Message message)
        {
            var filePath = _dbController.GetCommonFilePath(message.StringMessage);
            if (filePath == null)
            {
                SendResult(false, "Файл не найден");
                return;
            }
            File.Delete(filePath);
            _dbController.DeleteCommonFile(message.StringMessage);
            SendResult(true, String.Empty);
        }

        private void DeletePrivateFile(Message message)
        {
            var filePath = _dbController.GetPrivateFilePath(message.StringMessage);
            if (filePath == null)
            {
                SendResult(false, "Файл не найден");
                return;
            }
            File.Delete(filePath);
            _dbController.DeletePrivateFile(message.StringMessage);
            SendResult(true, String.Empty);
        }

        /// <summary>
        /// Смена ключа шифрования файла
        /// Или его установка, если файл еще не зашифрован
        /// </summary>
        /// <param name="message"></param>
        private void ChangeFileKey(Message message)
        {
            var fields = message.StringMessage.Split('$');
            if (fields.Length < 3)
            {
                SendResult(false, "Неверные параметры");
            }
            SHA1 hash = SHA1.Create();
            var fileKey = ByteToStringConverter(hash.ComputeHash(StringToByteConverter(fields[1])));
            CryptoController crypto = new CryptoController();
            var filePath = _dbController.GetPrivateFilePath(fields.First(), fileKey);
            if (filePath == null)
            {
                SendResult(false, "Неверный ключ");
                return;
            }
            crypto.ChangeEncryptKey(filePath, fields[1], fields[2]);
            fileKey = ByteToStringConverter(hash.ComputeHash(StringToByteConverter(fields[2])));
            _dbController.UpdateFileKey(fields[0], fileKey);
            SendResult(true, String.Empty);
        }

        private void CheckFile(Message message)
        {
            var filePath = _dbController.GetPrivateFilePath(message.StringMessage);
            if (filePath == null)
            {
                SendResult(false, "Файл не найден");
                return;
            }
            SHA1 hash = SHA1.Create();
            string fileHash;
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                fileHash = ByteToStringConverter(hash.ComputeHash(fs));
            }
            var result = _dbController.CheckFileHash(message.StringMessage, fileHash);
            _dbController.UpdateDamageStatus(message.StringMessage, !result);
            SendResult(result);
        }

        private void RecalcHash(Message message)
        {
            var filePath = _dbController.GetPrivateFilePath(message.StringMessage);
            if (filePath == null)
            {
                SendResult(false, "Файл не найден");
                return;
            }
            SHA1 hash = SHA1.Create();
            string fileHash;
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                fileHash = ByteToStringConverter(hash.ComputeHash(fs));
            }
            _dbController.UpdateFileHash(message.StringMessage, fileHash);
            SendResult(true);
        }

        private void SetNewRsaKeys(Message message)
        {
            var fileKey = message.StringMessage;
            var privateFilePath = CreatePrivateKeyFileName();
            var publicFilePath = CreatePublicKeyFileName();
            SHA1 hash = SHA1.Create();
            CryptoController crypto = new CryptoController();
            try
            {
                crypto.CreateAndSaveRsaParameters(publicFilePath, privateFilePath, fileKey);
            }
            catch (Exception ex)
            {
                SendResult(false, ex.Message);
                return;
            }
            var keyHash = ByteToStringConverter(hash.ComputeHash(StringToByteConverter(fileKey)));
            if (_dbController.GetPublicKeyPath(_userId) == null)
            {
                _dbController.AddRsaKey(_userId, privateFilePath, publicFilePath, keyHash);
            }
            else
            {
                _dbController.UpdateRsaKey(_userId, privateFilePath, publicFilePath, keyHash);
            }
            SendResult(true);
        }

        private void SendNewFileList()
        {
            var commonFiles = _dbController.GetNewFileList();
            foreach (var file in commonFiles)
            {
                _sslChannel.SendMessage(new Message(Commands.GetNewFileList, file));
            }
            //Сообщение ERROR отправляется как флаг окончания списка
            _sslChannel.SendMessage(new Message(Commands.Error, String.Empty));
        }

        private void SendReportList()
        {
            var commonFiles = _dbController.GetReportList();
            foreach (var file in commonFiles)
            {
                _sslChannel.SendMessage(new Message(Commands.GetReportList, file));
            }
            //Сообщение ERROR отправляется как флаг окончания списка
            _sslChannel.SendMessage(new Message(Commands.Error, String.Empty));
        }

        private void ShareFile(Message message)
        {
            var fields = message.StringMessage.Split('$');
            if (fields.Length < 5)
            {
                SendResult(false, "Параметры неверны");
            }
            var userIdTo = fields[0];
            var fileId = fields[1];
            var fileName = fields[2];
            var fileKey = fields[3];
            var comment = fields[4];
            var sendFilePath = CreatePrivateFileName(fileName);
            SHA1 hash = SHA1.Create();
            var fileKeyHash = ByteToStringConverter(hash.ComputeHash(StringToByteConverter(fileKey)));
            var filePath = _dbController.GetPrivateFilePath(fileId, fileKeyHash);
            if (filePath == null)
            {
                SendResult(false, "Неверный ключ");
                return;
            }
            if (!File.Exists(filePath))
            {
                SendResult(false, "Файл не найден в файловой системе. Ошибка сервера");
                Console.WriteLine("Файл не найден в файловой системе: " + filePath);
                return;
            }
            try
            {
                File.Copy(filePath, sendFilePath);
            }
            catch (Exception)
            {
                SendResult(false, "Не удалось отправть файл");
                return;
            }
            var rsaPublicFile = _dbController.GetPublicKeyPath(userIdTo);
            if (String.IsNullOrEmpty(rsaPublicFile))
            {
                SendResult(false, "Пользователь не имеет открытого ключа");
                try
                {
                    File.Delete(sendFilePath);
                }
                catch (Exception)
                {
                    //
                }
                return;
            }
            var newFileKey = GenerateNewFileKey(10);
            CryptoController crypto = new CryptoController();
            crypto.ChangeEncryptKey(sendFilePath, fileKey, newFileKey);
            newFileKey = ByteToStringConverter(crypto.EncryptDataWithRsaFile(rsaPublicFile,
                    StringToByteConverter(newFileKey)));
            _dbController.ShareFile(sendFilePath, fileName, userIdTo, newFileKey, comment);
            SendResult(true);
        }

        private void ReciveNewFile(Message message)
        {
            var fields = message.StringMessage.Split('$');
            if (fields.Length < 3)
            {
                SendResult(false, "Параметры неверны");
            }
            var transferId = fields[0];
            var rsaParamKey = fields[1];
            var newFileKey = fields[2];
            var transferParams = _dbController.GetTransferParams(transferId);
            if (transferParams == null)
            {
                SendResult(false, "Файл не найден");
                return;
            }
            SHA1 hash = SHA1.Create();
            var rsaParamsKeyHash = ByteToStringConverter(hash.ComputeHash(StringToByteConverter(rsaParamKey)));
            var rsaFilePath = _dbController.GetPrivateKeyPath(_userId, rsaParamsKeyHash);
            if (rsaFilePath == null)
            {
                SendResult(false, "Неверный ключ обмена");
                return;
            }
            CryptoController crypto = new CryptoController();
            var oldFileKeyBytes = crypto.DecryptDataWithRsaFile(rsaFilePath, rsaParamKey,
                StringToByteConverter(transferParams[2]));
            if (oldFileKeyBytes == null)
            {
                SendResult(false, "Ошибка извлечения ключа файла");
                return;
            }
            var oldFileKey = ByteToStringConverter(oldFileKeyBytes);
            crypto.ChangeEncryptKey(transferParams[0], oldFileKey, newFileKey);
            string fileHash;
            string fileLength;
            using (FileStream fs = new FileStream(transferParams[0], FileMode.Open))
            {
                fileHash = ByteToStringConverter(hash.ComputeHash(fs));
                fileLength = fs.Length.ToString();
            }
            var newFileHash = ByteToStringConverter(hash.ComputeHash(StringToByteConverter(newFileKey)));
            _dbController.SaveNewPrivateFile(transferParams[1], transferParams[0],
                fileLength, fileHash, true, newFileHash);
            _dbController.UpdateTransferStatus(transferId);
            SendResult(true);
        }

        private void DeleteNewFile(Message message)
        {
            var transferParams = _dbController.GetTransferParams(message.StringMessage);
            if (transferParams.Length < 3)
            {
                SendResult(false);
                return;
            }
            try
            {
                File.Delete(transferParams[0]);
            }
            catch (Exception)
            {
                //
            }
            _dbController.UpdateTransferStatus(message.StringMessage);
            SendResult(true);
        }

        private void SendUserList()
        {
            var users = _dbController.GetUsersList();
            foreach (var file in users)
            {
                _sslChannel.SendMessage(new Message(Commands.GetUserList, file));
            }
            //Сообщение ERROR отправляется как флаг окончания списка
            _sslChannel.SendMessage(new Message(Commands.Error, String.Empty));
        }

        #endregion#

        #region Other logic

        /// <summary>
        /// Проверка пароля на соответствие заданной политики паролей
        /// </summary>
        /// <param name="pass"></param>
        /// <returns></returns>
        private bool CheckPassPolicy(string pass)
        {
            //TODO сделать проврку
            return true;
        }

        private static string CreateCommonFileName(string fileName)
        {
            if (!Directory.Exists("Common"))
            {
                Directory.CreateDirectory("Common");
            }
            string resultName = fileName == String.Empty ? "file" : fileName;
            resultName = resultName.Replace("\\", "");
            int i = 0;
            while (File.Exists("Common\\" + resultName))
            {
                resultName = fileName.Split('.').First() + "_" + i + "." + fileName.Split('.').Last();
                i++;
            }
            resultName = "Common\\" + resultName;
            return resultName;
        }

        private static string CreatePrivateFileName(string fileName)
        {
            if (!Directory.Exists("Private"))
            {
                Directory.CreateDirectory("Private");
            }
            string resultName = fileName == String.Empty ? "file" : fileName;
            resultName = resultName.Replace("\\", "");
            int i = 0;
            while (File.Exists("Private\\" + resultName))
            {
                resultName = fileName.Split('.').First() + "_" + i + "." + fileName.Split('.').Last();
                i++;
            }
            resultName = "Private\\" + resultName;
            return resultName;
        }

        private string CreatePrivateKeyFileName()
        {
            if (!Directory.Exists("PrivateKeys"))
            {
                Directory.CreateDirectory("PrivateKeys");
            }
            return "PrivateKeys\\" + _userId + ".prKey";
        }

        private string CreatePublicKeyFileName()
        {
            if (!Directory.Exists("PublicKeys"))
            {
                Directory.CreateDirectory("PublicKeys");
            }
            return "PublicKeys\\" + _userId + ".pbKey";
        }

        private static string GenerateNewFileKey(int length)
        {
            Random rnd = new Random();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                sb.Append(rnd.Next(32, 1000));
            }
            return sb.ToString();
        }

        private static string ByteToStringConverter(byte[] bytes)
        {
            StringBuilder str = new StringBuilder();
            foreach (var bt in bytes)
            {
                str.Append((char)((char)bt + 1));
            }
            return str.ToString();
        }

        private static byte[] StringToByteConverter(string source)
        {
            var res = new byte[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                res[i] = (byte)source[i];
                res[i] -= 1;
            }
            return res;
        }

        #endregion#

        #endregion
    }
}