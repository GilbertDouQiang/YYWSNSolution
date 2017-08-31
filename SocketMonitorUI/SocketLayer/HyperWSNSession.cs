using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;


namespace HyperWSN.Socket
{
    public class HyperWSNSession : AppSession<HyperWSNSession, BinaryRequestInfo>
    {
        public new HyperWSNSocketServer AppServer
        {
            get
            {
                return (HyperWSNSocketServer)base.AppServer;
            }
        }

    }
}
