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
using System.Data;
using System.Data.SqlClient;


namespace SocketMonitorUI.BusinessLayer
{
    /// <summary>
    /// 服务器想网关发送授时反馈</br>
    /// BE BE 0F 91 01 00 18 51 00 10 00 17 09 15 01 38 46 01 95 89 EB EB
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
            if (ServiceStatus.ResponseNTP == true && requestInfo.Body.Length == 22)
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

                //Save to Database
                //授时响应
                if (session.SQLStatic == true)
                {
                    SqlCommand command = new SqlCommand();
                    command.Connection = session.SQLConn;
                    command.CommandText = "INSERT INTO [dbo].[NTPStatus]([DeviceMAC],[SerialNo],[ProtocolVersion],[NTPStatus],[RequestDateTime],[SendDateTime],[SourceData],[SendData]) VALUES (" +
                        " @DeviceMAC,@SerialNo,@ProtocolVersion,@NTPStatus,@RequestDateTime,@SendDateTime,@SourceData,@SendData)";
                    command.Parameters.Add("@DeviceMAC", SqlDbType.VarChar);
                    command.Parameters.Add("@SerialNo", SqlDbType.Int);
                    command.Parameters.Add("@ProtocolVersion", SqlDbType.VarChar);
                    command.Parameters.Add("@NTPStatus", SqlDbType.Int);
                    command.Parameters.Add("@RequestDateTime", SqlDbType.DateTime);
                    command.Parameters.Add("@SendDateTime", SqlDbType.DateTime);
                    command.Parameters.Add("@SourceData", SqlDbType.VarChar);
                    command.Parameters.Add("@SendData", SqlDbType.VarChar);



                    command.Parameters["@DeviceMAC"].Value = CommArithmetic.DecodeMAC(requestInfo.Body, 7);//协议起始位置-1
                    command.Parameters["@SerialNo"].Value = CommArithmetic.Byte2Int(requestInfo.Body,5,2);
                    command.Parameters["@ProtocolVersion"].Value = requestInfo.Body[4].ToString("X2");
                    command.Parameters["@NTPStatus"].Value = requestInfo.Body[17];
                    command.Parameters["@RequestDateTime"].Value = CommArithmetic.DecodeDateTime(requestInfo.Body, 11);
                    command.Parameters["@SendDateTime"].Value = System.DateTime.Now;
                    command.Parameters["@SourceData"].Value = CommArithmetic.ToHexString(requestInfo.Body);
                    command.Parameters["@SendData"].Value = CommArithmetic.ToHexString(response);

                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception)
                    {
                    }
                }

               

                //Save to Queue
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

                //save to database

                
                Logger.AddLog(DateTime.Now.ToString("HH:mm:ss.fff") + " :SendData:" + session.RemoteEndPoint.Address.ToString() + " :\t"
                   + CommArithmetic.ToHexString(response) + " ");

            }



        }

    }
}
