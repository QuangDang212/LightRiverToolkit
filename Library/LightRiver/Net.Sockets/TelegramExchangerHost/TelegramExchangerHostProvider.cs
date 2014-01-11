using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightRiver.Net.Sockets
{
    public class TelegramExchangerHostProvider
    {
        /// <summary>
        /// Current Server Index
        /// </summary>
        private int _currentIndex = 0;

        private List<TelegramExchangerHost> _hostList = new List<TelegramExchangerHost>();
        public List<TelegramExchangerHost> Hosts
        {
            get { return _hostList; }
        }

        public TelegramExchangerHost Current
        {
            get
            {
                return this[_currentIndex];
            }
        }

        public TelegramExchangerHost this[int index]
        {
            get
            {
                if (_hostList.Count == 0)
                    return null;

                if (index < 0 || index > _hostList.Count - 1)
                    return null;

                return _hostList[index];
            }
        }

        public void MoveNext()
        {
            _currentIndex++;
        }

        public void ResetIndex()
        {
            _currentIndex = 0;
        }
    }
}
