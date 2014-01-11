using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightRiver.Net.Sockets
{
    public delegate void TelegramDispatcherCallback(Telegram telegram);

    public interface ITelegramExchangeDispatcher
    {
        SocketConnector Connector { get; }

        SocketSender Sender { get; }

        SocketReceiver Receiver { get; }

        void Enqueue(Telegram telegram, TelegramDispatcherCallback callback);

        void Start(SocketConnector connector, SocketSender sender, SocketReceiver receiver);
    }
}
