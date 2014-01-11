using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightRiver.Net.Sockets
{
    public enum TelegramSocketConnectionState
    {
        Connected,
        Disconnected,
    };

    public abstract class SocketConnector : IDisposable
    {
        #region Fields
        
        private const int _connectWaitPeriod = 500; // 500ms

        private Task _connectTask = null;

        private ManualResetEvent _connectSleepEvent = null;

        private CancellationTokenSource _connectCanelToken = null;

        private bool _threadStopFlag = false;

        #endregion

        #region Event

        public event EventHandler<TelegramSocketConnectionStateChangeEventArgs> ConnectionStateChanged;
        private void OnConnectionStateChanged(TelegramSocketConnectionStateChangeEventArgs e)
        {
            SafeRaise.Raise<TelegramSocketConnectionStateChangeEventArgs>(ConnectionStateChanged, this, e);
        }

        public event EventHandler NeedReconnected;
        protected void OnNeedReconnected()
        {
            SafeRaise.Raise(NeedReconnected, this);
        }

        public event EventHandler ErrorOccured;
        protected void OnErrorOccured()
        {
            SafeRaise.Raise(ErrorOccured, this);
        }

        #endregion

        #region Property

        public ITelegramSocket ClientSocket { get; private set; }

        public TelegramExchangerHostProvider HostProvider { get; protected set; }

        public TelegramExchangerHost Proxy { get; set; }

        public bool IsEnableProxy { get; set; }

        public bool IsStarted { get; protected set; }

        public int MaxTriedConnectTimes { get; set; }

        public int RetriedConnectTimes { get; protected set; }

        private TelegramSocketConnectionState _connectionState = TelegramSocketConnectionState.Disconnected;
        public virtual TelegramSocketConnectionState ConnectionState
        {
            get { return _connectionState; }
            protected set
            {
                if (_connectionState == value)
                    return;

                _connectionState = value;
                OnConnectionStateChanged(new TelegramSocketConnectionStateChangeEventArgs(_connectionState));
            }
        }

        #endregion

        public SocketConnector(ITelegramSocket socket)
        {
            if (socket == null)
                throw new ArgumentNullException();

            ClientSocket = socket;
            HostProvider = new TelegramExchangerHostProvider();
            MaxTriedConnectTimes = 3;
            RetriedConnectTimes = 0;
            IsEnableProxy = false;
            IsStarted = false;
        }

        public virtual void Start()
        {
            if (IsStarted)
                return;

            _connectSleepEvent = new ManualResetEvent(false);
            _connectCanelToken = new CancellationTokenSource();
            _connectTask = Task.Factory.StartNew(() => ConnectTaskWorkMethod(), _connectCanelToken.Token);
            IsStarted = true;
        }

        public virtual void Stop()
        {
            if (!IsStarted)
                return;

            _connectCanelToken.Cancel();
            _connectTask.Wait(_connectCanelToken.Token);
            _connectCanelToken.Dispose();

            _connectSleepEvent.Dispose();

            ConnectionState = TelegramSocketConnectionState.Disconnected;
        }

        public void Dispose()
        {
            Stop();
        }

        private void ConnectTaskWorkMethod()
        {
            while (!_threadStopFlag) {
                if (_connectCanelToken.IsCancellationRequested)
                    break;

                if (ClientSocket == null) {
                    _connectSleepEvent.WaitOne(_connectWaitPeriod);
                    continue;
                }

                if (ConnectionState == TelegramSocketConnectionState.Connected) {
                    _connectSleepEvent.WaitOne(_connectWaitPeriod);
                    continue;
                }

                ConnectImplement();

                _connectSleepEvent.WaitOne(_connectWaitPeriod);
            }
        }

        protected abstract void ConnectImplement();

        public void ResetMaxTriedConnectTimes()
        {
            MaxTriedConnectTimes = 0;
        }
    }
}
