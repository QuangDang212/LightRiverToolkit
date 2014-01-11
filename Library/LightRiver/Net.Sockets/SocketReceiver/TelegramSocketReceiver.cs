using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightRiver.Net.Sockets
{
    /*
     * Receive logic is mainly relative what you need so please remember to implement ReceiveImplement() yourself
     */
    public class TelegramSocketReceiver : SocketReceiver
    {
        /// <summary>
        /// 接收資料的最大長度
        /// </summary>
        private const int MAX_BUFFER_SIZE = 2048;

        public TelegramSocketReceiver(ITelegramSocket socket)
            : base(socket)
        {
        }

        protected override void ReceiveImplement()
        {
            byte[] buffer = null;
            if (ClientSocket.Receive(MAX_BUFFER_SIZE, out buffer)) {
                if (buffer == null)
                    return;

                // TODO: do your socket receive implement logic
                return;
            }

            OnErrorOccured();
        }
    }
}
