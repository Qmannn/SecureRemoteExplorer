using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using ExplorerServer.Core.DataBase;
using ExplorerServer.Core.Network;
using Npgsql;
using NpgsqlTypes;

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
                    Console.WriteLine("Получено сообщение ошибки. Соединение разорвано. Логин: " + _login);
                    return false;
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

            pass = Encoding.UTF8.GetString(sha1.ComputeHash(Encoding.UTF8.GetBytes(pass)));
            try
            {
                _name = _dbController.Authorization(login, pass);
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

        #endregion
    }
}