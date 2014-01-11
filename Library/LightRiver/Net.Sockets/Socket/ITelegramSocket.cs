using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightRiver.Net.Sockets
{
    public class SocketErrorEventArgs : EventArgs
    {
        public enum SocketMethod
        {
            Connect,
            Send,
            Receive,
        };

        public SocketMethod Method { get; private set; }

        public string Message { get; private set; }

        public SocketErrorEventArgs(SocketMethod method, string errorMessage)
        {
            Method = method;
            Message = errorMessage;
        }
    }

    public interface ITelegramSocket : IDisposable
    {
        event EventHandler<SocketErrorEventArgs> ErrorOccured;

        bool Connect(string host, string port);

        bool Send(byte[] buffer);

        bool Receive(int length, out byte[] receiveBuffer);
    }
}
