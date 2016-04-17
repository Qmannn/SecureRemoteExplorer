using System;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using ExplorerServer.Core.Cryptography;

namespace ExplorerServer.Core.Network
{
    public class SslChannel
    {
        private readonly SslStream _stream;
        private readonly BinaryFormatter _formatter = new BinaryFormatter();
        public int BufferLength { get; } = 1024 * 8;
        private const int CheckBufferSize = 32;
        private readonly CryptoController _cryptoController = new CryptoController();

        public SslChannel(TcpClient client,  X509Certificate certificate)
        {
            _stream = new SslStream(client.GetStream(), false);
            _stream.AuthenticateAsServer(certificate, false, SslProtocols.Tls, false);
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
#if DEBUG
                Console.WriteLine("Не удалоь отправить сообщение: " + ex.Message);
#endif
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
#if DEBUG
                Console.WriteLine("Не удалоь отправить сообщение: " + ex.Message);
#endif
            }

            return message ?? new Message(Commands.Error, String.Empty);
        }

        /// <summary>
        /// Ожидание и прияние файла с клиента
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
                    fs.Write(bytes, 0, bytes.Length);
                } while (true);

            }
            return true;
        }

        /// <summary>
        /// Принимает файл и сразу шифрует принимаемый поток
        /// Необходимо для исключения нахождения на сервре незашифрованного файла
        /// </summary>
        /// <param name="fileName">Имя создаваемого файлв</param>
        /// <param name="key">Пользовательский ключ шифрования</param>
        /// <returns>Результат</returns>
        public bool ReciveEncryptedFile(string fileName, string key)
        {
            _cryptoController.Key = key;
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
                    if (bytes.Length%8 != 0)
                    {
                        var subBytes = new byte[(8 - bytes.Length%8) + 8];
                        subBytes[subBytes.Length - 1] = (byte) (8 - bytes.Length % 8);
                        bytes = bytes.Concat(subBytes).ToArray();
                    }
                    //шифрование принятого массива байт
                    bytes = _cryptoController.GostEncryptBytes(bytes);
                    fs.Write(bytes, 0, bytes.Length);
                } while (true);
            }
            return true;
        }

        /// <summary>
        /// Отправляет клиенту файл, предварительно расшифровывая его
        /// </summary>
        /// <param name="fileName">Путь до файла</param>
        /// <param name="key">Ключ шифрования</param>
        /// <returns></returns>
        public bool SendEncryptedFile(string fileName, string key)
        {
            if (!File.Exists(fileName))
            {
                return false;
            }
            _cryptoController.Key = key;
            var byteFormatter = new BinaryFormatter();
            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                while (fs.Position != fs.Length)
                {
                    var bytes =
                        new byte[fs.Length - fs.Position > BufferLength ? BufferLength : fs.Length - fs.Position];
                    var bytesCount = fs.Read(bytes, 0, bytes.Length);

                    //Криптографические преобразования над массивом байт
                    bytes = _cryptoController.GostDecryptBytes(bytes);
                    if (fs.Position == fs.Length)
                    {
                        var subBytesCount = bytes.Last() + 8;
                        var lastBytes = new byte[bytes.Length - subBytesCount];
                        Array.Copy(bytes, lastBytes, bytes.Length - subBytesCount);
                        bytes = lastBytes;
                    }
                    //

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
        /// Отправка файла клиенту
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        /// <returns></returns>
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
    }
}