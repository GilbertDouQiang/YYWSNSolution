using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using HyperWSN.Socket;

namespace SuperSocket.QuickStart.GPSSocketServer.Command
{
    public class Position : CommandBase<HyperWSNSession, BinaryRequestInfo>
    {
        public override string Name
        {
            get
            {
                return "01";
            }
        }

        public override void ExecuteCommand(HyperWSNSession session, BinaryRequestInfo requestInfo)
        {
            //The logic of saving GPS position data
            var response = session.AppServer.DefaultResponse;
            session.Send(response, 0, response.Length); ;
        }
    }
}
