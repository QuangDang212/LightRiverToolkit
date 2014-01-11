using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightRiver.Net.Sockets
{
    /*
     * this Connector logic in ConnectImplement() is based on my experience and my need.
     * So please modify if you have more simply or complex need.
     */
    public class TelegramSocketConnector : SocketConnector
    {
        private bool _isProxyConnected = false;

        public TelegramSocketConnector(ITelegramSocket socket)
            : base(socket)
        {
        }

        protected override void ConnectImplement()
        {
            if (HostProvider.Hosts.Count == 0)
                return;

            if (RetriedConnectTimes >= MaxTriedConnectTimes) {
                OnNeedReconnected();
                return;
            }

            var serverAddressPair = HostProvider.Current;
            if (serverAddressPair == null) {
                RetriedConnectTimes++;
                HostProvider.ResetIndex();
                return;
            }

            bool isSuccess = true;
            if (!IsEnableProxy) {
                isSuccess = ConnectDirect(serverAddressPair);
            }
            else {
                isSuccess = ConnectThroughProxy(serverAddressPair);
            }

            if (isSuccess) {
                ConnectionState = TelegramSocketConnectionState.Connected;
                RetriedConnectTimes = 0;
                return;
            }

            ConnectionState = TelegramSocketConnectionState.Disconnected;
            RetriedConnectTimes++;
            HostProvider.MoveNext();

            OnErrorOccured();
        }

        private bool ConnectDirect(TelegramExchangerHost serverAddressPair)
        {
            return ClientSocket.Connect(serverAddressPair.Host, serverAddressPair.Port);
        }

        private bool ConnectThroughProxy(TelegramExchangerHost serverAddressPair)
        {
            if (!ConnectProxy())
                return false;

            if (!SendProxyConnectProtocol(serverAddressPair))
                return false;

            if (!ReceiveProxyConnectResult())
                return false;

            return true;
        }

        private bool ConnectProxy()
        {
            if (_isProxyConnected)
                return true;

            if (Proxy == null)
                return false;

            _isProxyConnected = ClientSocket.Connect(Proxy.Host, Proxy.Port);
            return _isProxyConnected;
        }

        private bool SendProxyConnectProtocol(TelegramExchangerHost serverAddressPair)
        {
            // SOCKS 4 Protocol
            // +----+----+----+----+----+----+----+----+----+----+....+----+
            // | VN | CD | DSTPORT |      DSTIP        | USERID       |NULL|
            // +----+----+----+----+----+----+----+----+----+----+....+----+
            //    1    1      2              4           variable       1

            byte[] buffer = new byte[14];

            buffer[0] = 0x04; // VN
            buffer[1] = 0x01; // CD, 1:CONNECT request，2: BIND request

            // DSTPORT
            UInt16 port = Convert.ToUInt16(serverAddressPair.Port);
            byte[] portBytes = BitConverter.GetBytes(port);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(portBytes);
            Array.Copy(portBytes, 0, buffer, 2, 2);

            // DSTIP
            string[] ipAddresses = serverAddressPair.Host.Split('.');
            byte[] ipBytes = new byte[4];
            for (int i = 0; i < 4; i++) {
                ipBytes[i] = Byte.Parse(ipAddresses[i]);
            }
            Array.Copy(ipBytes, 0, buffer, 4, 4);

            // USERID : using default user name "guest"
            byte[] userIDBytes = Encoding.UTF8.GetBytes("guest");
            Array.Copy(userIDBytes, 0, buffer, 8, 5);

            // add c++ string terminated
            buffer[13] = 0x00;

            var sendResult = ClientSocket.Send(buffer);

            if (!sendResult)
                OnErrorOccured();

            return sendResult;
        }

        private bool ReceiveProxyConnectResult()
        {
            // Received data from server
            // Data has now been sent and received from the server. 
            // check result from sock4 proxy
            //
            // SOCKS 4 connect receive protocol
            // +----+----+----+----+----+----+----+----+
            // | VN | CD | DSTPORT |      DSTIP        |
            // +----+----+----+----+----+----+----+----+
            //    1    1      2              4          
            /*
                Socks Server CD(return code)
                90: request granted
                91: request rejected or failed
                92: request rejected becasue SOCKS server cannot connect to
                    identd on the client
                93: request rejected because the client program and identd
                    report different user-ids
            */

            const int bufferLength = 8;
            byte[] buffer = null;

            var receiveResult = ClientSocket.Receive(bufferLength, out buffer);
            if (!receiveResult) {
                OnErrorOccured();
                return false;
            }

            Debug.Assert(bufferLength == buffer.Length);
            
            return (buffer[1] == 90);
        }
    }
}