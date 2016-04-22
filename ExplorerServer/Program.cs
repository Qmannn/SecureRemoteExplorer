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
            Server server = new Server(3000, "SSLServer.pfx");

            server.Start(); 
        }

        
    }
}
