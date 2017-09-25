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
    public class SensorData : CommandBase<HyperWSNSession, BinaryRequestInfo>
    {
        public override string Name
        {
            get
            {
                return "94";
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
            if (ServiceStatus.ResponseSensorData == true)
            {
                if ( requestInfo.Body.Length == 63 || requestInfo.Body.Length == 60)
                {
                    byte[] response = new byte[12];
                    response[0] = 0xEB;  //开始位
                    response[1] = 0xEB;  //开始位
                    response[2] = 0x05;  //总长度
                    response[3] = 0x94;  //命令位
                    response[4] = requestInfo.Body[4]; //协议版本
                    response[5] = requestInfo.Body[5]; //序列号
                    response[6] = requestInfo.Body[6]; //序列号
                    response[7] = 0x00;


                    response[8] = 0x00;
                    response[9] = 0x00;
                    response[10] = 0xBE;
                    response[11] = 0xBE;
                    session.Send(response, 0, response.Length);

                    try
                    {
                        if (session.QueueStatic == true)
                        {
                            session.SaveToQueue(requestInfo.Body);
                        }

                    }
                    catch (Exception)
                    {


                    }

                    Logger.AddLog(DateTime.Now.ToString("HH:mm:ss.fff") + " :SendData:" + session.RemoteEndPoint.Address.ToString() + " :\t"
                       + CommArithmetic.ToHexString(response) + " ");
                }

               



               

            }



        }
    }
}
