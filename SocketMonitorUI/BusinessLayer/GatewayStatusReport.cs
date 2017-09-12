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
    ///  Hyper WSN Internet Protocol 1.0 </br>
    ///  Gateway Update Information
    /// </summary>
    public class GatewayStatusReport : CommandBase<HyperWSNSession, BinaryRequestInfo>
    {
        public override string Name
        {
            get
            {
                return "92";
            }
        }

        /// <summary>
        /// 这里有多种业务处理逻辑</br>
        /// 01. 仅发送反馈
        /// 02. 提示下发数据
        /// 03. 发送下发数据
        /// 
        /// </summary>
        /// <param name="session"></param>
        /// <param name="requestInfo"></param>
        public override void ExecuteCommand(HyperWSNSession session, BinaryRequestInfo requestInfo)
        {
            //记录到日志,收到数据
            Logger.AddLog(DateTime.Now.ToString("HH:mm:ss.fff") + " :Received:" + session.RemoteEndPoint.Address.ToString() + " :\t"
                + CommArithmetic.ToHexString(requestInfo.Body) + " ");
            //The logic of saving GPS position data
            //var response = session.AppServer.DefaultResponse;
            //byte[] response = new byte[] { 0xEB, 0xEB, 0x01, 0x03, 0x00, 0x00, 0xBE, 0xBE };

            
            //识别出不同的网关
            Device gateway = DeviceFactory.CreateDevice(requestInfo.Body);
            if (gateway.GetType()==typeof(AlarmGateway1))
            {
                if (ServiceStatus.ResponseGatewayReport == true)
                {
                    //发现一体机
                    //发送默认反馈
                    byte[] response = new byte[17];
                    response[0] = 0xEB;
                    response[1] = 0xEB;
                    response[2] = 0x10;
                    response[3] = 0x92;
                    response[4] = requestInfo.Body[4];
                    response[5] = requestInfo.Body[5];
                    response[6] = requestInfo.Body[6];

                    response[7] = requestInfo.Body[8];
                    response[8] = requestInfo.Body[9];
                    response[9] = requestInfo.Body[10];
                    response[10] = requestInfo.Body[11];


                    response[11] = 0x00;
                    response[12] = 0x00;
                    response[13] = 0x00;
                    response[14] = 0x00;
                    response[15] = 0xBE;
                    response[16] = 0xBE;

                    session.Send(response, 0, response.Length); ;
                    Logger.AddLog(DateTime.Now.ToString("HH:mm:ss.fff") + " :SendData:" + session.RemoteEndPoint.Address.ToString() + " :\t"
                       + CommArithmetic.ToHexString(response) + " ");
                }
                


            }
                

           

              

            



        }

    }
}
