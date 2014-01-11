using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightRiver.Net.Sockets
{
    public class TelegramSocketReceivedEventArgs : EventArgs
    {
        public Telegram Telegram { get; private set; }

        public TelegramSocketReceivedEventArgs(Telegram telegramData)
        {
            Telegram = telegramData;
        }
    }

    public class TelegramSocketConnectionStateChangeEventArgs : EventArgs
    {
        public TelegramSocketConnectionState State { get; private set; }

        public TelegramSocketConnectionStateChangeEventArgs(TelegramSocketConnectionState state)
        {
            State = state;
        }
    }
}
