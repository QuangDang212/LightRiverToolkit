using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightRiver.Net.Sockets
{
    public class TelegramExchangerHost
    {
        /// <summary>
        /// server address (IP address)
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// connection port (between 0 ~ 65535)
        /// </summary>
        public string Port { get; set; }

        public TelegramExchangerHost(string host, string port)
        {
            Host = host;
            Port = port;
        }
    }
}
