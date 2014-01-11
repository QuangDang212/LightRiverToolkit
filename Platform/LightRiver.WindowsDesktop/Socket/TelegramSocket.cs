using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LightRiver.Net.Sockets
{
    public class TelegramSocket : ITelegramSocket
    {
        private Socket _clientSocket = null;

        private object _recreateLock = new object();

        private DateTime _lastRecreateTime = DateTime.MinValue;

        private const int _recreatDuration = 1;

        public event EventHandler<SocketErrorEventArgs> ErrorOccured;
        private void OnErrorOccured(SocketErrorEventArgs e)
        {
            SafeRaise.Raise<SocketErrorEventArgs>(ErrorOccured, this, e);
        }

        public TelegramSocket()
        {
            CreateSocket();
        }

        public bool Connect(string host, string port)
        {
            try {
                _clientSocket.Connect(host, int.Parse(port));
                return true;
            }
            catch (SocketException ex) {
                OnErrorOccured(new SocketErrorEventArgs(SocketErrorEventArgs.SocketMethod.Connect, ex.Message));
                ReCreateSocket();
                return false;
            }
        }

        public bool Send(byte[] buffer)
        {
            try {
                _clientSocket.Send(buffer);
                return true;
            }
            catch (SocketException ex) {
                OnErrorOccured(new SocketErrorEventArgs(SocketErrorEventArgs.SocketMethod.Send, ex.Message));
                ReCreateSocket();
                return false;
            }
        }

        public bool Receive(int length, out byte[] receiveBuffer)
        {
            receiveBuffer = null;

            try {
                var buffer = new byte[length];
                int readLength = _clientSocket.Receive(buffer);
                receiveBuffer = new byte[readLength];
                Buffer.BlockCopy(buffer, 0, receiveBuffer, 0, readLength);
                return true;
            }
            catch (SocketException ex) {
                OnErrorOccured(new SocketErrorEventArgs(SocketErrorEventArgs.SocketMethod.Receive, ex.Message));
                ReCreateSocket();
                return false;
            }
        }

        public void Dispose()
        {
            if (_clientSocket != null)
                _clientSocket.Dispose();
        }

        private void CreateSocket()
        {
            _clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

        private void ReCreateSocket()
        {
            lock (_recreateLock) {
                if (DateTime.Now.Subtract(_lastRecreateTime) < TimeSpan.FromSeconds(_recreatDuration))
                    return;
            }

            _lastRecreateTime = DateTime.Now;
            Dispose();
            CreateSocket();
        }
    }
}
