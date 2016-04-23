using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using ExplorerServer.Config;
using Npgsql;

namespace ExplorerServer.Core
{
    public class Server
    {
        #region members#
        
        private X509Certificate _certificate;
        private TcpListener _listener;
        private readonly List<Client> _clients = new List<Client>();
        private NpgsqlConnection _dbConnection;
        private bool _working = true;
        private Thread _listenThread;
        private readonly ServerData _config;
        #endregion members#

        #region PublicMethod#

        public Server(ServerData data)
        {
            _config = data;
        }

        /// <summary>
        /// Запуск сервера, подклчение к базе данных
        /// </summary>
        public void Start()
        {
            _working = true;
            //Подключение к базе данных
            _dbConnection = new NpgsqlConnection("Server="
                + _config.DataBaseServer +
                ";Port="
                + _config.DataBasePort +
                ";User="
                + _config.DataBaseLogin +
                ";Password="
                + _config.DataBasePass +
                ";Database="
                + _config.DataBaseName +
                ";");
            try
            {
                _dbConnection.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Не удалось подключиться к базе данных");
                Console.WriteLine("Original error:" + ex.Message);
                return;
            }
            try
            {
                //Открытие файла сертификата
                _certificate = new X509Certificate2(_config.CertificateName, _config.CertificatePass);
                _listener = new TcpListener(IPAddress.Any, _config.Port);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка запуска сервера: " + ex.Message);
                return;
            }
            Console.WriteLine("Сервер запущен.");
            _listenThread = new Thread(AcceptClients);
            _listenThread.Start();
        }

        /// <summary>
        /// Остановка сервера
        /// </summary>
        public void Stop()
        {
            _listener.Stop();
            _listenThread.Abort();
            foreach (var client in _clients)
            {
                client.Stop();
            }
            Console.WriteLine("Сервер останвлен");
        }

        #endregion PublicMethods

        #region PrivateMethods

        private void AcceptClients()
        {
            _listener.Start();
            while (_working)
            {
                try
                {
                    Client client = new Client(_listener.AcceptTcpClient(), _certificate, _dbConnection);
                    _clients.Add(client);
                    client.Start();
                }
                catch (Exception)
                {
                    Console.WriteLine("Сервер был принудительно остановлен.");
                    _working = false;
                }
            }
        }

        #endregion PrivateMethods

    }
}