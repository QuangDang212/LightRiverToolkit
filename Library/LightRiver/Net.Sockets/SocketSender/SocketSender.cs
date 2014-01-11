using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightRiver.Net.Sockets
{
    public abstract class SocketSender : IDisposable
    {
        #region Field

        private const int _transferSleepPeriod = 100; // 100ms

        private Task _sendTask = null;

        private ManualResetEvent _sendSleepEvent = null;

        private CancellationTokenSource _sendCancelToken = null;

        private bool _threadStopFlag = false;

        #endregion

        #region Event

        public event EventHandler NeedSendEcho;
        protected void OnNeedSendEcho()
        {
            SafeRaise.Raise(NeedSendEcho, this);
        }

        public event EventHandler ErrorOccured;
        protected void OnErrorOccured()
        {
            SafeRaise.Raise(ErrorOccured, this);
        }

        #endregion

        #region Property

        public ITelegramSocket ClientSocket { get; private set; }

        public bool IsStarted { get; protected set; }

        private bool IsPauseed { get; set; }

        public int HeartBeatPeriod { get; set; }

        #endregion

        public SocketSender(ITelegramSocket socket, bool isPaused = true)
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

            _sendSleepEvent = new ManualResetEvent(false);
            _sendCancelToken = new CancellationTokenSource();
            _sendTask = Task.Factory.StartNew(() => SendTaskWorkMehtod(), _sendCancelToken.Token);
            IsStarted = true;
        }

        public virtual void Stop()
        {
            if (!IsStarted)
                return;

            _sendCancelToken.Cancel();
            _sendTask.Wait(_sendCancelToken.Token);
            _sendCancelToken.Dispose();

            _sendSleepEvent.Dispose();
        }

        public void Pause()
        {
            IsPauseed = true;
        }

        public void Dispose()
        {
            Stop();
        }

        public abstract void Enqueue(Telegram telegram);

        private void SendTaskWorkMehtod()
        {
            while (!_threadStopFlag) {
                if (IsPauseed) {
                    _sendSleepEvent.WaitOne(_transferSleepPeriod);
                    continue;
                }

                if (_sendCancelToken.IsCancellationRequested)
                    break;

                SendImplement();

                _sendSleepEvent.WaitOne(_transferSleepPeriod);
            }
        }

        protected abstract void SendImplement();
    }
}
