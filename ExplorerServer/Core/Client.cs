using System;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using ExplorerServer.Core.Network;

namespace ExplorerServer.Core
{
    public class Client
    {
        private readonly TcpClient _client;
        private readonly X509Certificate _certificate;
        private readonly SslChannel _sslChannel;
        private Thread _clientThread;

        #region Public#

        public Client(TcpClient client, X509Certificate certificate)
        {
            _client = client;
            _certificate = certificate;
            _sslChannel = new SslChannel(_client, _certificate);
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

        #endregion#

        #region Private

        private bool MessageHandler(Message message)
        {
            switch (message.Command)
            {
                case Commands.Hello:
                    Console.WriteLine(message.StringMessage);
                    _sslChannel.SendMessage(new Message(Commands.Bye, "Hello my dear client!"));
                    break;
                case Commands.Bye:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return true;
        }

        #endregion
    }
}