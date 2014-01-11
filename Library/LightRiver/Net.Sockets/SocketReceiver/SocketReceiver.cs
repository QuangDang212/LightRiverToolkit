using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightRiver.Net.Sockets
{
    public abstract class SocketReceiver : IDisposable
    {
        #region Field

        private const int _transferSleepPeriod = 100; // 100ms

        private Task _receiveTask = null;

        private ManualResetEvent _receiveSleepEvent = new ManualResetEvent(false);

        private CancellationTokenSource _receiveCancelToken = null;

        private bool _threadStopFlag = false;

        #endregion

        #region Property

        public ITelegramSocket ClientSocket { get; private set; }

        public bool IsStarted { get; protected set; }

        private bool IsPauseed { get; set; }

        #endregion

        #region Event

        public event EventHandler<TelegramSocketReceivedEventArgs> ReceiveFinished;
        protected void OnReceiveFinished(TelegramSocketReceivedEventArgs e)
        {
            SafeRaise.Raise<TelegramSocketReceivedEventArgs>(ReceiveFinished, this, e);
        }

        public event EventHandler ErrorOccured;
        protected void OnErrorOccured()
        {
            SafeRaise.Raise(ErrorOccured, this);
        }

        #endregion

        public SocketReceiver(ITelegramSocket socket, bool isPaused = true)
        {
            if (socket == null)
                throw new ArgumentNullException();

            ClientSocket = socket;
            IsPauseed = isPaused;
            IsStarted = false;
        }

        public virtual void Start()
        {
            if (IsPauseed)
                IsPauseed = false;

            if (IsStarted)
                return;

            _receiveSleepEvent = new ManualResetEvent(false);
            _receiveCancelToken = new CancellationTokenSource();
            _receiveTask = Task.Factory.StartNew(() => SendTaskWorkMehtod(), _receiveCancelToken.Token);
            IsStarted = true;
        }

        public virtual void Stop()
        {
            if (!IsStarted)
                return;

            _receiveCancelToken.Cancel();
            _receiveTask.Wait(_receiveCancelToken.Token);
            _receiveCancelToken.Dispose();

            _receiveSleepEvent.Dispose();
        }

        public void Pause()
        {
            IsPauseed = true;
        }

        public void Dispose()
        {
            Stop();
        }

        private void SendTaskWorkMehtod()
        {
            while (!_threadStopFlag) {
                if (IsPauseed) {
                    _receiveSleepEvent.WaitOne(_transferSleepPeriod);
                    continue;
                }

                if (_receiveCancelToken.IsCancellationRequested)
                    break;

                ReceiveImplement();

                _receiveSleepEvent.WaitOne(_transferSleepPeriod);
            }
        }

        protected abstract void ReceiveImplement();
    }
}
