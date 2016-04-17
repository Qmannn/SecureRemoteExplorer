using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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