using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExplorerServer.Core;

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
