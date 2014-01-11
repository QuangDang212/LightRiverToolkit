using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightRiver.Net.Sockets
{
    /*
     * this Sender logic in SendImplement() is based on my experience and my need.
     * So please modify if you have more simply or complex need.
     */
    public class TelegramSocketSender : SocketSender
    {
        private object _queueLockObj = new object();

        protected Queue<Telegram> _sendQueue = new Queue<Telegram>();

        protected Dictionary<int, int> _telegramSendRecord = new Dictionary<int, int>();

        private DateTime _lastSendTime = DateTime.Now;

        public TelegramSocketSender(ITelegramSocket socket)
            : base(socket)
        {
            HeartBeatPeriod = 25000; // 25000ms
        }

        public override void Stop()
        {
            base.Stop();

            _sendQueue.Clear();
        }

        public override void Enqueue(Telegram telegram)
        {
            lock (_queueLockObj) {
                _sendQueue.Enqueue(telegram);
                _telegramSendRecord.Add(telegram.SerialNo, 0);
            }
        }

        protected override void SendImplement()
        {
            if (_sendQueue.Count == 0) {
                var timeFromLastSend = DateTime.Now - _lastSendTime;
                if (timeFromLastSend.TotalMilliseconds > HeartBeatPeriod) {
                    OnNeedSendEcho();
                }
                return;
            }

            Telegram telegram = _sendQueue.Peek();
            if (ClientSocket.Send(telegram.Buffer)) {
                _sendQueue.Dequeue();
                return;
            }

            _lastSendTime = DateTime.Now;

            var sentTimes = _telegramSendRecord[telegram.SerialNo];
            sentTimes++;
            
            if (sentTimes >= telegram.MaxResendTimes)
                _sendQueue.Dequeue();

            OnErrorOccured();
        }
    }
}
