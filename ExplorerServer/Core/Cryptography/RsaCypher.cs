using System;
using System.Security;
using System.Security.Cryptography;

namespace ExplorerServer.Core.Cryptography
{
    public class RsaCypher
    {
        public static byte[] RsaEncrypt(byte[] dataToEncrypt, RSAParameters rsaKeyInfo, bool doOaepPadding)
        {
            try
            {
                byte[] encryptedData;
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportParameters(rsaKeyInfo);
                    encryptedData = rsa.Encrypt(dataToEncrypt, doOaepPadding);
                }
                return encryptedData;
            }
            catch (CryptographicException e)
            {
                return null;
            }

        }

        public static byte[] RsaDecrypt(byte[] dataToDecrypt, RSAParameters rsaKeyInfo, bool doOaepPadding)
        {
            try
            {
                byte[] decryptedData;
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportParameters(rsaKeyInfo);
                    decryptedData = rsa.Decrypt(dataToDecrypt, doOaepPadding);
                }
                return decryptedData;
            }
            catch (CryptographicException e)
            {
                return null;
            }
        }

        public static RSAParameters GenerateRsaParameters()
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                return rsa.ExportParameters(true);
            }
        }

    }
}