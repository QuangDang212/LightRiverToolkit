using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace LightRiver.Net.Sockets
{
    public class TelegramSocket : ITelegramSocket
    {
        private StreamSocket _clientSocket = null;

        private IAsyncAction _connectOperation = null;

        private DataWriter _dataWriter = null;

        private DataWriterStoreOperation _dataWriterOperation = null;

        private DataReader _dataReader = null;

        private DataReaderLoadOperation _dataReaderOperation = null;

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
                _connectOperation = _clientSocket.ConnectAsync(new HostName(host), port);
                _connectOperation.AsTask().Wait();
                if (_connectOperation.ErrorCode != null) {
                    OnErrorOccured(new SocketErrorEventArgs(SocketErrorEventArgs.SocketMethod.Connect, SocketError.GetStatus(_connectOperation.ErrorCode.HResult).ToString()));
                    _connectOperation = null;
                    return false;
                }
                _connectOperation = null;
                return true;
            }
            catch (TaskCanceledException) {
                return false;
            }
            catch (Exception ex) {
                OnErrorOccured(new SocketErrorEventArgs(SocketErrorEventArgs.SocketMethod.Connect, SocketError.GetStatus(ex.HResult).ToString()));
                ReCreateSocket();
                return false;
            }
        }

        public bool Send(byte[] buffer)
        {
            try {
                _dataWriter.WriteBytes(buffer);
                _dataWriterOperation = _dataWriter.StoreAsync();
                _dataWriterOperation.AsTask().Wait();
                if (_dataWriterOperation.ErrorCode != null) {
                    OnErrorOccured(new SocketErrorEventArgs(SocketErrorEventArgs.SocketMethod.Connect, SocketError.GetStatus(_dataWriterOperation.ErrorCode.HResult).ToString()));
                    _dataWriterOperation = null;
                    return false;
                }
                _dataWriterOperation = null;
                return true;
            }
            catch (TaskCanceledException) {
                return false;
            }
            catch (Exception ex) {
                OnErrorOccured(new SocketErrorEventArgs(SocketErrorEventArgs.SocketMethod.Send, SocketError.GetStatus(ex.HResult).ToString()));
                ReCreateSocket();
                return false;
            }
        }

        public bool Receive(int length, out byte[] receiveBuffer)
        {
            receiveBuffer = null;
            uint lengthUnsigned = Convert.ToUInt32(length);
            try {
                _dataReaderOperation = _dataReader.LoadAsync(lengthUnsigned);
                _dataReaderOperation.AsTask().Wait();
                if (_dataReaderOperation.ErrorCode != null) {
                    OnErrorOccured(new SocketErrorEventArgs(SocketErrorEventArgs.SocketMethod.Connect, SocketError.GetStatus(_dataReaderOperation.ErrorCode.HResult).ToString()));
                    _dataReaderOperation = null;
                    return false;
                }
                
                _dataReaderOperation = null;

                if (_dataReader.UnconsumedBufferLength <= 0)
                    return true;

                receiveBuffer = new byte[_dataReader.UnconsumedBufferLength];
                _dataReader.ReadBytes(receiveBuffer);
                return true;
            }
            catch (Exception ex) {
                OnErrorOccured(new SocketErrorEventArgs(SocketErrorEventArgs.SocketMethod.Receive, SocketError.GetStatus(ex.HResult).ToString()));
                ReCreateSocket();
                return false;
            }
        }

        public void Dispose()
        {
            if (_dataWriterOperation != null)
                _dataWriterOperation.Cancel();

            if (_dataReaderOperation != null)
                _dataReaderOperation.Cancel();

            // put connectOperaion at the last one. or _dataReader.DetachStream will raise exception when dispose
            if (_connectOperation != null)
                _connectOperation.Cancel();

            DisposeDataReader();

            DisposeDataWriter();

            if (_clientSocket != null) {
                _clientSocket.Dispose();
                _clientSocket = null;
            }
        }

        private void DisposeDataReader()
        {
            if (_dataReader == null)
                return;

            bool isError = false;
            try {
                _dataReader.DetachBuffer();
                _dataReader.DetachStream();
                _dataReader.Dispose();
            }
            catch {
                isError = true;
            }
            finally {
                if (!isError && _dataReader != null)
                    _dataReader.Dispose();

                _dataReader = null;
            }
        }

        private void DisposeDataWriter()
        {
            if (_dataWriter == null)
                return;

            bool isError = false;
            try {
                _dataWriter.DetachBuffer();
                _dataWriter.DetachStream();
                _dataWriter.Dispose();
            }
            catch {
                isError = true;
            }
            finally {
                if (!isError && _dataWriter != null)
                    _dataWriter.Dispose();

                _dataWriter = null;
            }
        }

        private void CreateSocket()
        {
            _clientSocket = new StreamSocket();
            _dataReader = new DataReader(_clientSocket.InputStream);
            _dataReader.InputStreamOptions = InputStreamOptions.Partial;
            _dataWriter = new DataWriter(_clientSocket.OutputStream);
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
