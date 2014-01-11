using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightRiver.Net.Sockets
{
    /// <summary>
    /// the base telegram data exchange use class
    /// </summary>
    public class Telegram
    {
        public int SerialNo { get; private set; }

        public byte[] Buffer { get; private set; }

        /// <summary>
        /// 最大重送次數
        /// </summary>
        public int MaxResendTimes { get; private set; }

        public Telegram(byte[] rawData)
            : this(0, rawData)
        {
        }

        public Telegram(int serialNo, byte[] rawData, int maxResendTimes = 2)
        {
            SerialNo = serialNo;
            Buffer = rawData;
            MaxResendTimes = maxResendTimes;
        }
    }
}
