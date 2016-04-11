using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace ExplorerServer.Core.Network
{
    public class SslChannel
    {
        private readonly SslStream _stream;
        private readonly X509Certificate _certificate;
        private readonly BinaryFormatter _formatter = new BinaryFormatter();

        public SslChannel(TcpClient client,  X509Certificate certificate)
        {
            _certificate = certificate;
            _stream = new SslStream(client.GetStream(), false);
            _stream.AuthenticateAsServer(_certificate, false, SslProtocols.Tls, false);
            _formatter.Binder = new DeserializationBinder();
        }

        public void SendMessage(Message message)
        {
            try
            {
                _formatter.Serialize(_stream, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Не удалоь отправить сообщение: " + ex.Message);
                //TODO добавить запись в лог!
            }
        }

        public Message ReciveMessage()
        {
            Message message = null;
            try
            {
                message = (Message)_formatter.Deserialize(_stream);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Не удалоь получить сообщение: " + ex.Message);
                //TODO добавить запись в лог!
            }

            return message ?? new Message(Commands.Bye, String.Empty);
        }
    }
}