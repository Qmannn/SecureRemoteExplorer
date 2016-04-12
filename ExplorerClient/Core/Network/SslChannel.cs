using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace ExplorerClient.Core.Network
{
    public class SslChannel
    {
        private SslStream _stream;
        private string _host;
        private int _port;
        private TcpClient _client;
        private readonly BinaryFormatter _formatter = new BinaryFormatter();

        public bool Connected => _client.Connected;

        // The following method is invoked by the RemoteCertificateValidationDelegate.
        public static bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            return true;
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;
            
            throw new SecurityException("Server certificate error. PolicyError: " + sslPolicyErrors);
        }

        public SslChannel(string host, int port)
        {
            _host = host;
            _port = port;
            _formatter.Binder = new DeserializationBinder();
            try
            {
                _client = new TcpClient(host, port);
                _stream = new SslStream(_client.GetStream(), false,
                    new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
                _stream.AuthenticateAsClient(host); 
            }
            catch (Exception)
            {
                throw new Exception("Ошибка подключения");
            }
        }

        public void SendMessage(Message message)
        {
            _formatter.Serialize(_stream, message);
        }

        public Message ReciveMessage()
        {
            return (Message)_formatter.Deserialize(_stream);
        }
    }
}