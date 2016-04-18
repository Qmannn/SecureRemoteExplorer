using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ExplorerServer.Core;
using ExplorerServer.Core.Cryptography;

namespace ExplorerServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //Server server = new Server(3000, "SSLServer.pfx");

            //server.Start();

            BinaryFormatter formatter = new BinaryFormatter();
            RSAParameters rsaParams = new RSAParameters();
            using (FileStream fs = new FileStream("rsa.key", FileMode.Open))
            {
                rsaParams = (RSAParameters) formatter.Deserialize(fs);
                var lol = fs.Length;
            }
            try
            {
                //Create a new RSACryptoServiceProvider object.
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {


                    //Export the key information to an RSAParameters object.
                    //Pass false to export the public key information or pass
                    //true to export public and private key information.
                    rsaParams = rsa.ExportParameters(false);
                }


            }
            catch (CryptographicException e)
            {
                //Catch this exception in case the encryption did
                //not succeed.
                Console.WriteLine(e.Message);

            }
            

            //CryptoController controller = new CryptoController();
            //controller.GostEncryptString("lolaaaaa", "SecretKey");
            //controller.EncrypeStreamToFile(new StreamReader("test.txt"), "lol", "outfile");
        }
    }
}
