using System;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Cryptography.X509Certificates;

namespace ExplorerClient.Core.Network
{
    public class SslChannel
    {
        private readonly SslStream _stream;
        private string _host;
        private int _port;
        private readonly TcpClient _client;
        private readonly BinaryFormatter _formatter = new BinaryFormatter();
        private const int CheckBufferSize = 32;
        
        public bool Connected => _client.Connected;

        public int BufferLength { get; set; } = 1024*8;

        // The following method is invoked by the RemoteCertificateValidationDelegate.
        public static bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            return certificate != null;
        }

        /// <summary>
        /// Подключние к серверу
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public SslChannel(string host, int port)
        {
            _host = host;
            _port = port;
            _formatter.Binder = new DeserializationBinder();
            try
            {
                _client = new TcpClient(host, port);
                _stream = new SslStream(_client.GetStream(), false,
                    ValidateServerCertificate, null);
                _stream.AuthenticateAsClient(host); 
            }
            catch (Exception)
            {
                throw new Exception("Ошибка подключения");
            }
        }

        /// <summary>
        /// Отправка сообщения с командой и неким текстом
        /// </summary>
        /// <param name="message">Отправляемое сообщение</param>
        public void SendMessage(Message message)
        {
            try
            {
                _formatter.Serialize(_stream, message);
            }
            catch (Exception)
            {
                if (!_client.Connected)
                {
                    Disconnected?.Invoke();
                }
            }
        }

        /// <summary>
        /// Ожиание и принятие от сервера сообщения с командой и неким текстовым сообщением
        /// </summary>
        /// <returns>Сообщение</returns>
        public Message ReciveMessage()
        {
            try
            {
                return (Message)_formatter.Deserialize(_stream);
            }
            catch (Exception)
            {
                if (!_client.Connected)
                {
                    Disconnected?.Invoke();
                }
            }
            return new Message(Commands.Error, String.Empty);
        }

        /// <summary>
        /// Отправка файла на сервер по защищенному каналу
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <returns>Результат отправки</returns>
        public bool SendFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return false;
            }
            var byteFormatter = new BinaryFormatter();
            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                while (fs.Position != fs.Length)
                {
                    var bytes =
                        new byte[fs.Length - fs.Position > BufferLength ? BufferLength : fs.Length - fs.Position];
                    var bytesCount = fs.Read(bytes, 0, bytes.Length);
                    byteFormatter.Serialize(_stream, bytes);
                }
            }
            var endFileBytes = new byte[CheckBufferSize];
            for (var i = 0; i < endFileBytes.Length; i++)
            {
                endFileBytes[i] = Byte.MaxValue;
            }
            byteFormatter.Serialize(_stream, endFileBytes);
            return true;
        }
        

        /// <summary>
        /// Ожидание и прияние файла с сервера
        /// Если файл с именем уже существует - он будет перезаписан
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        public bool ReciveFile(string fileName)
        {
            var byteFormatter = new BinaryFormatter();
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                do
                {
                    object recivedObj;
                    try
                    {
                        recivedObj = byteFormatter.Deserialize(_stream);
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                    var bytes = (byte[])recivedObj;
                    if (bytes.Length == CheckBufferSize)
                    {
                        if (bytes.Any(bt => bt == byte.MaxValue))
                        {
                            break;
                        }
                    }
                    //TODO шифровать
                    fs.Write(bytes, 0, bytes.Length);
                } while (true);

            }
            return true;
        }

        public delegate void DisconnectedDelegate();
        public event DisconnectedDelegate Disconnected;
    }
}