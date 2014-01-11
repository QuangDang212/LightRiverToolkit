using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightRiver.Net.Sockets
{
    /*
     * this TelegramExchangeDispatcher logic is based on my experience and my need.
     * So please modify if you have more simply or complex need.
     */
    public sealed class TelegramExchangeDispatcher : ITelegramExchangeDispatcher, IDisposable
    {
        #region Field
        
        private object _historyMapLockObj = new object();

        private Dictionary<int, TelegramDispatcherCallback> _historyMap = new Dictionary<int, TelegramDispatcherCallback>();

        #endregion

        #region Property

        private TelegramSocketConnector _connector = null;
        public SocketConnector Connector
        {
            get { return _connector; }
        }

        private TelegramSocketSender _sender = null;
        public SocketSender Sender
        {
            get { return _sender; }
        }

        private TelegramSocketReceiver _receiver = null;
        public SocketReceiver Receiver
        {
            get { return _receiver; }
        }

        #endregion

        public TelegramExchangeDispatcher()
        {
        }

        public void Dispose()
        {
            _connector.Dispose();
            _sender.Dispose();
            _receiver.Dispose();
        }

        public void Enqueue(Telegram telegram, TelegramDispatcherCallback callback)
        {
            lock (_historyMapLockObj) {
                _historyMap.Add(telegram.SerialNo, callback);
            }

            _sender.Enqueue(telegram);
        }

        public void Start(SocketConnector connector, SocketSender sender, SocketReceiver receiver)
        {
            if (connector == null || sender == null || receiver == null)
                throw new ArgumentNullException();

            _connector = connector as TelegramSocketConnector;
            _sender = sender as TelegramSocketSender;
            _receiver = receiver as TelegramSocketReceiver;

            if (_connector == null || _sender == null || _receiver == null)
                throw new ArgumentException();

            _connector.ConnectionStateChanged += Connector_ConnectionStateChanged;
            _connector.ErrorOccured += Connector_ErrorOccured;
            _connector.NeedReconnected += Connector_NeedReconnected;
            _sender.ErrorOccured += Sender_ErrorOccured;
            _sender.NeedSendEcho += Sender_NeedSendEcho;
            _receiver.ErrorOccured += Receiver_ErrorOccured;
            _receiver.ReceiveFinished += Receiver_ReceiveFinished;

            // start Connector at first and start Sender/Receiver after receiver Connector.ConnectionStateChanged evet.
            _connector.Start();
        }

        public void Stop()
        {
            _connector.Stop();
            _sender.Stop();
            _receiver.Stop();
        }

        private void Connector_ConnectionStateChanged(object sender, TelegramSocketConnectionStateChangeEventArgs e)
        {
            if (e.State == TelegramSocketConnectionState.Connected) {
                _sender.Start();
                _receiver.Start();
            }
            else {
                _sender.Pause();
                _receiver.Pause();
            }
        }

        private void Connector_ErrorOccured(object sender, EventArgs e)
        {
        }

        private void Connector_NeedReconnected(object sender, EventArgs e)
        {
        }

        private void Sender_NeedSendEcho(object sender, EventArgs e)
        {
        }

        private void Sender_ErrorOccured(object sender, EventArgs e)
        {
        }

        private void Receiver_ErrorOccured(object sender, EventArgs e)
        {
        }

        private void Receiver_ReceiveFinished(object sender, TelegramSocketReceivedEventArgs e)
        {
            // get last telegramData
            Telegram telegram = e.Telegram;
            if (telegram == null)
                return;

            // get parser from historyMap
            if (!_historyMap.ContainsKey(telegram.SerialNo))
                return;

            var callback = _historyMap[telegram.SerialNo];
            _historyMap.Remove(telegram.SerialNo);
            // null means the telegramData doesn't belong to this controller or no need to be parsed.
            if (callback == null)
                return;

            // involk callback
            callback(telegram);
        }
    }
}
