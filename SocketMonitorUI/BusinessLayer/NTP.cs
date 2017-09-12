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
    /// 服务器想网关发送授时反馈
    /// </summary>
    public class NTP : CommandBase<HyperWSNSession, BinaryRequestInfo>
    {
        public override string Name
        {
            get
            {
                return "91";
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
            if (ServiceStatus.ResponsePing == true && requestInfo.Body.Length == 22)
            {

                byte[] response = new byte[21];
                response[0] = 0xEB;  //开始位
                response[1] = 0xEB;  //开始位
                response[2] = 0x0E;  //总长度
                response[3] = 0x91;  //命令位
                response[4] = requestInfo.Body[4]; //协议版本
                response[5] = requestInfo.Body[5]; //序列号
                response[6] = requestInfo.Body[6]; //序列号
                response[7] = requestInfo.Body[7]; //MAC
                response[8] = requestInfo.Body[8]; //MAC
                response[9] = requestInfo.Body[9]; //MAC
                response[10] = requestInfo.Body[10]; //MAC

                byte[] timeNow = CommArithmetic.EncodeDateTime(DateTime.Now);
                response[11] = timeNow[0];
                response[12] = timeNow[1];
                response[13] = timeNow[2];
                response[14] = timeNow[3];
                response[15] = timeNow[4];
                response[16] = timeNow[5];

                response[17] = 0x00;
                response[18] = 0x00;
                response[19] = 0xBE;
                response[20] = 0xBE;



                session.Send(response, 0, response.Length); ;
                Logger.AddLog(DateTime.Now.ToString("HH:mm:ss.fff") + " :SendData:" + session.RemoteEndPoint.Address.ToString() + " :\t"
                   + CommArithmetic.ToHexString(response) + " ");

            }



        }

    }
}
