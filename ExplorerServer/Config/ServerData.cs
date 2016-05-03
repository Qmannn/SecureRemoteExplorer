using System.IO;

namespace ExplorerServer.Config
{
    public class ServerData
    {
        public const string ConfigFileName = "serverconf.cfg";
        public string CertificateName { get; set; }
        public string CertificatePass { get; set; }
        public int Port { get; set; }
        public string DataBaseServer { get; set; }
        public int DataBasePort { get; set; }
        public string DataBaseLogin { get; set; }
        public string DataBasePass { get; set; }
        public string DataBaseName { get; set; }
        public int MinPassLength { get; set; }
        public int PassValidateRegex { get; set; }

        public bool ReadCongigFile()
        {
            if (!File.Exists(ConfigFileName))
            {
                return false;
            }
            StreamReader sReader = File.OpenText(ConfigFileName);
            sReader.ReadLine();
            CertificateName = sReader.ReadLine();
            sReader.ReadLine();
            CertificatePass = sReader.ReadLine();
            sReader.ReadLine();
            var varString = sReader.ReadLine();
            int varInt;
            if (!int.TryParse(varString, out varInt))
            {
                sReader.Close();
                return false;
            }
            if (varInt < 1)
            {
                sReader.Close();
                return false;
            }
            Port = varInt;
            sReader.ReadLine();
            DataBaseServer = sReader.ReadLine();
            sReader.ReadLine();
            varString = sReader.ReadLine();
            if (!int.TryParse(varString, out varInt))
            {
                sReader.Close();
                return false;
            }
            DataBasePort = varInt;
            if (varInt < 1)
            {
                sReader.Close();
                return false;
            }
            sReader.ReadLine();
            DataBaseLogin = sReader.ReadLine();
            sReader.ReadLine();
            DataBasePass = sReader.ReadLine();
            sReader.ReadLine();
            DataBaseName = sReader.ReadLine();
            sReader.ReadLine();
            varString = sReader.ReadLine();
            if (!int.TryParse(varString, out varInt))
            {
                sReader.Close();
                return false;
            }
            MinPassLength = varInt;
            sReader.ReadLine();
            varString = sReader.ReadLine();
            if (!int.TryParse(varString, out varInt))
            {
                sReader.Close();
                return false;
            }
            PassValidateRegex = varInt;
            sReader.Close();
            return true;
        }

        public void WriteConfigFile()
        {
            StreamWriter sWriter = File.CreateText(ConfigFileName);
            sWriter.WriteLine("Имя сертификата:");
            sWriter.WriteLine(CertificateName);
            sWriter.WriteLine("Пароль сертификата:");
            sWriter.WriteLine(CertificatePass);
            sWriter.WriteLine("Порт сервера:");
            sWriter.WriteLine(Port);
            sWriter.WriteLine("IP адрес базы данных:");
            sWriter.WriteLine(DataBaseServer);
            sWriter.WriteLine("Порт базы данных:");
            sWriter.WriteLine(DataBasePort);
            sWriter.WriteLine("Логин для базы данных:");
            sWriter.WriteLine(DataBaseLogin);
            sWriter.WriteLine("Пароль для базы данных:");
            sWriter.WriteLine(DataBasePass);
            sWriter.WriteLine("Имя базы данных:");
            sWriter.WriteLine(DataBaseName);
            sWriter.WriteLine("Минимальная длина пароля пользователя:");
            sWriter.WriteLine(MinPassLength);
            sWriter.WriteLine("Сложноть пароля пользователей (1-3):");
            sWriter.WriteLine(PassValidateRegex);
            sWriter.Close();

        }

        public void CreateTemplateCfg()
        {
            StreamWriter sWriter = File.CreateText(ConfigFileName);
            sWriter.WriteLine("Имя сертификата:");
            sWriter.WriteLine();
            sWriter.WriteLine("Пароль сертификата:");
            sWriter.WriteLine();
            sWriter.WriteLine("Порт сервера:");
            sWriter.WriteLine();
            sWriter.WriteLine("IP адрес базы данных:");
            sWriter.WriteLine();
            sWriter.WriteLine("Порт базы данных:");
            sWriter.WriteLine();
            sWriter.WriteLine("Логин для базы данных:");
            sWriter.WriteLine();
            sWriter.WriteLine("Пароль для базы данных:");
            sWriter.WriteLine();
            sWriter.WriteLine("Имя базы данных:");
            sWriter.WriteLine();
            sWriter.WriteLine("Минимальная длина пароля пользователя:");
            sWriter.WriteLine();
            sWriter.WriteLine("Сложноть пароля пользователей (1-3):");
            sWriter.WriteLine("");
            sWriter.Close();
        }

    }
}
