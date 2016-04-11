using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

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

        #endregion members#

        #region PublicMethod#
        public Server(int port, string certificatePath)
        {
            _port = port;
            _certificatePath = certificatePath;
        }

        public void Start()
        {
            try
            {
                _certificate = new X509Certificate2(_certificatePath, "1111");
                _listener = new TcpListener(IPAddress.Any, _port);
            }
            catch (Exception ex)
            {
                
                throw new Exception("Error server start");
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
                Client client = new Client(_listener.AcceptTcpClient(), _certificate);
                _clients.Add(client);
                client.Start();
            }
        }

        #endregion PrivateMethods

    }
}