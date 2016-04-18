using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using ExplorerServer.Core.Cryptography;

namespace ExplorerServer.Core.Cryptography
{
    public class CryptoController
    {

        #region Members

        private byte[] _keyBytes;
        private readonly GostCipher _gost = new GostCipher();
        private const int BufferLength = 1024;
        #endregion

        #region Public

        public string Key
        {
            set { _keyBytes = GetKeyBytes(value); }
        }

        public byte[] GostEncryptBytes(byte[] bytes)
        {
            return _gost.Encode(bytes, _keyBytes, false);
        }

        public byte[] GostDecryptBytes(byte[] bytes)
        {
            return _gost.Decode(bytes, _keyBytes, false);
        }

        public void ChangeEncryptKey(string file, string oldKey, string newKey)
        {
            var oldKeyBytes = GetKeyBytes(oldKey);
            var newKeyBytes = GetKeyBytes(newKey);
            using (FileStream fs = new FileStream(file, FileMode.Open))
            {
                while (fs.Position != fs.Length)
                {
                    var bytes = new byte[fs.Length - fs.Position < 1024 ? fs.Length - fs.Position : 1024];
                    fs.Read(bytes, 0, bytes.Length);
                    bytes = _gost.Decode(bytes, oldKeyBytes);
                    bytes = _gost.Encode(bytes, newKeyBytes);
                    fs.Position -= bytes.Length;
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
        }

        public void EncryptFile(string file, string key)
        {
            var keyBytes = GetKeyBytes(key);
            using (FileStream fs = new FileStream(file, FileMode.Open))
            {
                while (fs.Position != fs.Length)
                {
                    var bytes = new byte[fs.Length - fs.Position < 1024 ? fs.Length - fs.Position : 1024];
                    fs.Read(bytes, 0, bytes.Length);
                    if (fs.Position == fs.Length)
                    {
                        var subBytes = new byte[(8 - bytes.Length % 8) + 8];
                        subBytes[subBytes.Length - 1] = (byte)(8 - bytes.Length % 8);
                        bytes = bytes.Concat(subBytes).ToArray();
                    }
                    bytes = _gost.Encode(bytes, keyBytes);
                    fs.Position = 0;
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
        }

        public void DecrypFile(string file, string key)
        {
            using (var fs = new FileStream(file, FileMode.Open))
            {
                while (fs.Position != fs.Length)
                {
                    var bytes =
                        new byte[fs.Length - fs.Position > BufferLength ? BufferLength : fs.Length - fs.Position];
                    var bytesCount = fs.Read(bytes, 0, bytes.Length);
                    bytes = GostDecryptBytes(bytes);
                    if (fs.Position == fs.Length)
                    {
                        var subBytesCount = bytes.Last() + 8;
                        var lastBytes = new byte[bytes.Length - subBytesCount];
                        Array.Copy(bytes, lastBytes, bytes.Length - subBytesCount);
                        bytes = lastBytes;
                    }
                    fs.Position -= bytes.Length;
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
        }

        public byte[] EncryptDataWithRsaFile(string rsaPublicParamsFilePath, byte[] encryptData)
        {
            if (!File.Exists(rsaPublicParamsFilePath))
            {
                return null;
            }
            RSAParameters rsaParameters;
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                using (FileStream fs = new FileStream(rsaPublicParamsFilePath, FileMode.Open))
                {
                    rsaParameters = (RSAParameters) formatter.Deserialize(fs);
                }
            }
            catch (Exception)
            {
                return null;
            }
            return RsaCypher.RsaEncrypt(encryptData, rsaParameters, false);
        }

        public byte[] DecryptDataWithRsaFile(string rsaPrivateParamsFilePath, string fileKey, byte[] encryptData)
        {
            if (!File.Exists(rsaPrivateParamsFilePath))
            {
                return null;
            }
            RSAParameters rsaParameters;
            Key = fileKey;
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                DecrypFile(rsaPrivateParamsFilePath, fileKey);
                using (FileStream fs = new FileStream(rsaPrivateParamsFilePath, FileMode.Open))
                {
                    rsaParameters = (RSAParameters) formatter.Deserialize(fs);
                }
                EncryptFile(rsaPrivateParamsFilePath, fileKey);
            }
            catch (Exception)
            {
                return null;
            }
            return RsaCypher.RsaDecrypt(encryptData, rsaParameters, false);
        }

        public void CreateAndSaveRsaParameters(string rsaPublicParamsFilePath, string rsaPrivateParamsFilePath, string privateFileKey)
        {
            RSAParameters privateParameters;
            RSAParameters publicParameters;
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                privateParameters = rsa.ExportParameters(true);
                publicParameters = rsa.ExportParameters(false);
            }
            var formatter = new BinaryFormatter();
            using (FileStream fs = new FileStream(rsaPrivateParamsFilePath, FileMode.Create))
            {
                formatter.Serialize(fs, privateParameters);
            }
            EncryptFile(rsaPrivateParamsFilePath, privateFileKey);
            using (FileStream fs = new FileStream(rsaPublicParamsFilePath, FileMode.Create))
            {
                formatter.Serialize(fs, publicParameters);
            }
        }

        #endregion#

        #region Private

        private byte[] GetKeyBytes(string key)
        {
            var hash = MD5.Create();
            var keyBytesFirst = hash.ComputeHash(Encoding.UTF8.GetBytes(key));
            var keyBytesSecond = hash.ComputeHash(Encoding.UTF8.GetBytes(MoveKey(key)));
            return keyBytesFirst.Concat(keyBytesSecond).ToArray();
        }

        private static string MoveKey(string key)
        {
            StringBuilder builder = new StringBuilder(key);
            char tmpChar = builder[0];
            builder[0] = builder[key.Length - 1];
            builder[key.Length - 1] = tmpChar;
            return builder.ToString();
        }
        #endregion #
    }

}