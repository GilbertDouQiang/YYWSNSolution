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
            if (gateway!=null && gateway.GetType()==typeof(AlarmGateway1))
            {
                if (ServiceStatus.ResponseGatewayReport == true)
                {
                    //发现一体机
                    //发送默认反馈
                    byte[] response = new byte[11];
                    response[0] = 0xEB;
                    response[1] = 0xEB;
                    response[2] = 0x04;
                    response[3] = 0x92;

                    response[4] = requestInfo.Body[4];
                    response[5] = requestInfo.Body[5];
                    response[6] = requestInfo.Body[6];


                    response[7] = 0x00;
                    response[8] = 0x00;
                    response[9] = 0xBE;
                    response[10] = 0xBE;

                    session.Send(response, 0, response.Length); ;

                    //Save to database
                    try
                    {
                        if (session.SQLStatic == true)
                        {
                            //授时响应
                            SqlCommand command = new SqlCommand();
                            command.Connection = session.SQLConn;
                            command.CommandText = "INSERT INTO [dbo].[GatewayStatus]([DeviceMac],[ProtocolVersion],[SerialNo],[DeviceType],[GatewayTransDateTime]" +
                                ",[GatewayVoltage],[SoftwareVersion] ,[ClientID],[RamCount],[RomCount],[GSMSignal],[BindingNumber],[TransforNumber],[SimNumber]" +
                                ",[LastSuccessNumber],[LastStatus],[TransStrategy],[ACPower],[SourceData],[SendData]) VALUES(@DeviceMac,@ProtocolVersion,@SerialNo,@DeviceType,@GatewayTransDateTime, " +
                                "@GatewayVoltage,@SoftwareVersion,@ClientID,@RamCount,@RomCount,@GSMSignal,@BindingNumber,@TransforNumber,@SimNumber," +
                                "@LastSuccessNumber,@LastStatus,@TransStrategy,@ACPower,@SourceData,@SendData)";

                            command.Parameters.Add("@DeviceMAC", SqlDbType.NVarChar);
                            command.Parameters.Add("@ProtocolVersion", SqlDbType.NVarChar);
                            command.Parameters.Add("@SerialNo", SqlDbType.Int);
                            command.Parameters.Add("@DeviceType", SqlDbType.NVarChar);
                            command.Parameters.Add("@GatewayTransDateTime", SqlDbType.DateTime);
                            command.Parameters.Add("@GatewayVoltage", SqlDbType.Decimal);
                            command.Parameters.Add("@SoftwareVersion", SqlDbType.NVarChar);
                            command.Parameters.Add("@ClientID", SqlDbType.NVarChar);
                            command.Parameters.Add("@RamCount", SqlDbType.Int);
                            command.Parameters.Add("@RomCount", SqlDbType.Int);
                            command.Parameters.Add("@GSMSignal", SqlDbType.Int);
                            command.Parameters.Add("@BindingNumber", SqlDbType.Int);
                            command.Parameters.Add("@TransforNumber", SqlDbType.Int);
                            command.Parameters.Add("@SimNumber", SqlDbType.NVarChar);
                            command.Parameters.Add("@LastSuccessNumber", SqlDbType.Int);
                            command.Parameters.Add("@LastStatus", SqlDbType.Int);
                            command.Parameters.Add("@TransStrategy", SqlDbType.Int);
                            command.Parameters.Add("@ACPower", SqlDbType.Int);
                            command.Parameters.Add("@SourceData", SqlDbType.VarChar);
                            command.Parameters.Add("@SendData", SqlDbType.VarChar);


                            command.Parameters["@DeviceMAC"].Value = CommArithmetic.DecodeMAC(requestInfo.Body, 8); //协议起始位置-1
                            command.Parameters["@ProtocolVersion"].Value = requestInfo.Body[4].ToString("X2");
                            command.Parameters["@SerialNo"].Value = CommArithmetic.Byte2Int(requestInfo.Body, 5, 2);
                            command.Parameters["@DeviceType"].Value = requestInfo.Body[7].ToString("X2");
                            command.Parameters["@GatewayTransDateTime"].Value = CommArithmetic.DecodeDateTime(requestInfo.Body, 12);
                            command.Parameters["@GatewayVoltage"].Value = CommArithmetic.DecodeVoltage(requestInfo.Body, 18);
                            command.Parameters["@SoftwareVersion"].Value = CommArithmetic.DecodeClientID(requestInfo.Body, 20);
                            command.Parameters["@ClientID"].Value = CommArithmetic.DecodeClientID(requestInfo.Body, 22);
                            //新增：TransStrategy
                            command.Parameters["@TransStrategy"].Value = requestInfo.Body[24];

                            command.Parameters["@RamCount"].Value = requestInfo.Body[25];
                            command.Parameters["@RomCount"].Value = CommArithmetic.Byte2Int(requestInfo.Body, 26, 3);
                            command.Parameters["@GSMSignal"].Value = requestInfo.Body[31];
                            command.Parameters["@BindingNumber"].Value = requestInfo.Body[33];
                            command.Parameters["@TransforNumber"].Value = requestInfo.Body[34];
                            command.Parameters["@SimNumber"].Value = CommArithmetic.DecodeMAC(requestInfo.Body, 36);
                            command.Parameters["@LastSuccessNumber"].Value = CommArithmetic.Byte2Int(requestInfo.Body, 41, 3);
                            command.Parameters["@LastStatus"].Value = CommArithmetic.Byte2Int(requestInfo.Body, 45, 2);
                            command.Parameters["@ACPower"].Value = CommArithmetic.DecodeACPower(requestInfo.Body[18]);
                            command.Parameters["@SourceData"].Value = CommArithmetic.ToHexString(requestInfo.Body);
                            command.Parameters["@SendData"].Value = CommArithmetic.ToHexString(response);


                            try
                            {
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {


                            }
                        }

                    }
                    catch (Exception)
                    {

                        throw;
                    }


                    try
                    {
                        if (session.QueueStatic == true)
                        {
                            //session.SaveToQueue(requestInfo.Body);
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
