using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//user define namaspace
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using HyperWSN.Socket;
using YyWsnCommunicatonLibrary;
using YyWsnDeviceLibrary;

namespace SocketMonitorUI.BusinessLayer
{
    /// <summary>
    /// Hyper WSN Internet Protocol 1.0 </br>
    /// Ping 
    /// </summary>
    public class GetwayPing : CommandBase<HyperWSNSession, BinaryRequestInfo>
    {
        public override string Name
        {
            get
            {
                return "90";
            }
        }

        public override void ExecuteCommand(HyperWSNSession session, BinaryRequestInfo requestInfo)
        {
            //记录到日志,收到数据
            Logger.AddLog(DateTime.Now.ToString("HH:mm:ss.fff") + " :Received:" + session.RemoteEndPoint.Address.ToString() + " :\t"
                + CommArithmetic.ToHexString(requestInfo.Body) + " ");
            //The logic of saving GPS position data
            //var response = session.AppServer.DefaultResponse;
            //byte[] response = new byte[] { 0xEB, 0xEB, 0x01, 0x03, 0x00, 0x00, 0xBE, 0xBE };
            if (ServiceStatus.ResponsePing == true && requestInfo.Body.Length == 11)
            {
                byte[] response = new byte[11];
                response[0] = 0xEB;
                response[1] = 0xEB;
                response[2] = 0x04;
                response[3] = 0x90;
                response[4] = requestInfo.Body[4];
                response[5] = requestInfo.Body[5];
                response[6] = requestInfo.Body[6];
                response[7] = 0x00;
                response[8] = 0x00;
                response[9] = 0xBE;
                response[10] = 0xBE;

                session.Send(response, 0, response.Length);

                Logger.AddLog(DateTime.Now.ToString("HH:mm:ss.fff") + " :SendData:" + session.RemoteEndPoint.Address.ToString() + " :\t"
                   + CommArithmetic.ToHexString(response) + " ");
            }
        }
    }
}
