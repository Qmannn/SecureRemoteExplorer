using System;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ExplorerClient.Core.Network;

namespace ExplorerClient.Core
{
    public static class Client
    {

        #region members

        private static SslChannel _sslChannel = null;

        public static string Host { get; private set; }
        public static int Port { get; private set; }
        public static bool Connected = false;
        
        #endregion

        #region Public

        public static void Connect(string host, int port)
        {
            _sslChannel = new SslChannel(host, port);
        }

        //TODO delete this method
        public static string SayHello(string message)
        {
            _sslChannel.SendMessage(new Message(Commands.Hello, message));
            return _sslChannel.ReciveMessage().StringMessage;
        }

        #endregion#
    }
}