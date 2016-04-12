using System;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ExplorerClient.Core.Network;
using ExplorerClient.Gui.View;

namespace ExplorerClient.Core
{
    public static class Client
    {

        #region members

        private static SslChannel _sslChannel = null;

        public static string Host { get; private set; }
        public static int Port { get; private set; }

        public static string Name { get; set; }
        public static string Login { get; set; }
        public static string Password { get; set; }

        public static bool Connected => _sslChannel?.Connected ?? false;
        public static bool IsAuthorized = false;
        public static string LastError { get; set; }

        #endregion

        #region Public

        public static void Connect(string host, int port)
        {
            _sslChannel = new SslChannel(host, port);
        }

        /// <summary>
        /// Асинхронное подключение к серверу
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static Task ConnectAsync(string host, int port)
        {
            return Task.Run(() =>
            {
                try
                {
                    Connect(host, port);
                }
                catch (Exception ex)
                {
                    LastError = ex.Message;
                    //ignored
                }
            });
        }

        public static void Authorization(string login, string pass)
        {
            string command = login + "$" + pass;
            _sslChannel.SendMessage(new Message(Commands.Login, command));
            var recivedCommand = _sslChannel.ReciveMessage();
            Name = recivedCommand.StringMessage;
            IsAuthorized = recivedCommand.Command == Commands.Ok;
        }

        /// <summary>
        /// Асинхронная авторизация
        /// </summary>
        /// <param name="login"></param>
        /// <param name="pass"></param>
        /// <returns></returns>
        public static Task AuthorizationAsync(string login, string pass)
        {
            return Task.Run(() =>
            {
                Authorization(login, pass);
            });
        }

        public static string[] GetUserState()
        {
            Message recivedMessage = null;
            try
            {
                _sslChannel.SendMessage(new Message(Commands.GetUserState, String.Empty));
                recivedMessage = _sslChannel.ReciveMessage();
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return null;
            }
            return recivedMessage?.StringMessage.Split('$');
;        }

        /// <summary>
        /// Асинхронное получение статистики пользователя
        /// </summary>
        /// <returns></returns>
        public static Task<string[]> GetUserStateAsync()
        {
            return Task.Run(() => GetUserState());
        }

        #endregion#
    }
}