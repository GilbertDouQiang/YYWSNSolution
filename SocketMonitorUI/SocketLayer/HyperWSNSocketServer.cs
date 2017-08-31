using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace HyperWSN.Socket
{
    public class HyperWSNSocketServer : AppServer<HyperWSNSession, BinaryRequestInfo>
    {
        public HyperWSNSocketServer()
            : base(new DefaultReceiveFilterFactory<HyperWSNReceiveFilter, BinaryRequestInfo>())
        {

            DefaultResponse = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 };
        }

        internal byte[] DefaultResponse { get; private set; }
    }
}
