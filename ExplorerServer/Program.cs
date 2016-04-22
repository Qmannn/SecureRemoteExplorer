﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ExplorerServer.Config;
using ExplorerServer.Core;
using ExplorerServer.Core.Cryptography;

namespace ExplorerServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerData config = new ServerData();
            if (!config.ReadCongigFile())
            {
                Console.WriteLine(@"Файл конфгурации не найден");
                Console.WriteLine(@"Создан шаблон файла конфигурации.");
                Console.WriteLine(@"Имя: " + ServerData.ConfigFileName);
                config.CreateTemplateCfg();
                Console.ReadKey();
                return;
            }
            Server server = new Server(config);

            server.Start();

            //управление сервером
            bool exit = false;
            do
            {
                var command = Console.ReadLine();
                switch (command?.ToLower())
                {
                    case "stop":
                        server.Stop();
                        break;
                    case "restart":
                        server.Stop();
                        server.Start();
                        break;
                    case "start":
                        server.Start();
                        break;
                    case "exit":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Неизвестная команда...");
                        break;
                }
            } while (!exit);
        }

        
    }
}
