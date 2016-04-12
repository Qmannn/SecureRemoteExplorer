using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using Npgsql;

namespace ExplorerServer.Core
{
    public class Server
    {
        #region members#

        private readonly int _port;
        private string _certificatePath;
        private X509Certificate _certificate;
        private TcpListener _listener;
        private List<Client> _clients = new List<Client>();
        private NpgsqlConnection _dbConnection = null;

        #endregion members#

        #region PublicMethod#
        public Server(int port, string certificatePath)
        {
            _port = port;
            _certificatePath = certificatePath;
        }

        /// <summary>
        /// Запуск сервера, подклчение к базе данных
        /// </summary>
        public void Start()
        {
            _dbConnection = new NpgsqlConnection("Server="
                + "localhost" +
                ";Port="
                + "5432" +
                ";User="
                + "postgres" +
                ";Password="
                + "1111" +
                ";Database="
                + "ExplorerDataBase" +
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
                _certificate = new X509Certificate2(_certificatePath, "1111");
                _listener = new TcpListener(IPAddress.Any, _port);
            }
            catch (Exception ex)
            {

                Console.WriteLine("Ошибка запуска сервера: " + ex.Message);
            }
            //TODO Асинхронный метод AcceptClients
            
            Console.WriteLine("Server was started");
            AcceptClients();
        }

        #endregion PublicMethods

        #region PrivateMethods

        private void AcceptClients()
        {
            _listener.Start();
            while (true)
            {
                Client client = new Client(_listener.AcceptTcpClient(), _certificate, _dbConnection);
                _clients.Add(client);
                client.Start();
            }
        }

        #endregion PrivateMethods

    }
}