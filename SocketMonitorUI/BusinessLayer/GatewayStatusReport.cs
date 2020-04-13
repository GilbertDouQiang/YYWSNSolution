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

            //识别出不同的网关
            Device gateway = DeviceFactory.CreateDevice(requestInfo.Body);
            if (gateway != null && gateway.GetType() == typeof(AlarmGateway1))
            {
                if (ServiceStatus.ResponseGatewayReport == true)
                {
                    byte[] TxBuf = null;

                    if (gateway.DeviceMacV == ServiceStatus.MacOfDesGateway && ServiceStatus.ExeResult == 0 && ServiceStatus.ExeCmd == 0xA3)
                    {
                        TxBuf = new byte[17];
                        
                        // 起始位
                        TxBuf[0] = 0xEB;
                        TxBuf[1] = 0xEB;

                        // 长度位
                        TxBuf[2] = 0x0A;

                        // 命令位
                        TxBuf[3] = ServiceStatus.ExeCmd;

                        // 协议版本
                        TxBuf[4] = requestInfo.Body[4];

                        // 序列号
                        TxBuf[5] = requestInfo.Body[5];
                        TxBuf[6] = requestInfo.Body[6];

                        ServiceStatus.Serial = (UInt16)(((UInt16)requestInfo.Body[5] << 8) | ((UInt16)requestInfo.Body[6] << 0));

                        // GW ID
                        TxBuf[7] = requestInfo.Body[8];
                        TxBuf[8] = requestInfo.Body[9];
                        TxBuf[9] = requestInfo.Body[10];
                        TxBuf[10] = requestInfo.Body[11];

                        // 协议保留位
                        TxBuf[11] = 0x00;
                        TxBuf[12] = 0x00;

                        // CRC16
                        UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 3, TxBuf[2]);
                        TxBuf[13] = (byte)((crc & 0xFF00) >> 8);
                        TxBuf[14] = (byte)((crc & 0x00FF) >> 0);

                        // 结束位
                        TxBuf[15] = 0xBE;
                        TxBuf[16] = 0xBE;

                        session.Send(TxBuf, 0, TxBuf.Length);

                        ServiceStatus.ExeResult = 1;
                    }
                    else
                    {
                        //发现一体机
                        //发送默认反馈
                        TxBuf = new byte[11];

                        // 起始位
                        TxBuf[0] = 0xEB;
                        TxBuf[1] = 0xEB;

                        // 长度位
                        TxBuf[2] = 0x04;

                        // 命令位
                        TxBuf[3] = 0x92;

                        // 协议版本
                        TxBuf[4] = requestInfo.Body[4];

                        // 序列号
                        TxBuf[5] = requestInfo.Body[5];
                        TxBuf[6] = requestInfo.Body[6];

                        // CRC16
                        UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 3, TxBuf[2]);
                        TxBuf[7] = (byte)((crc & 0xFF00) >> 8);
                        TxBuf[8] = (byte)((crc & 0x00FF) >> 0);

                        // 结束位
                        TxBuf[9] = 0xBE;
                        TxBuf[10] = 0xBE;

                        session.Send(TxBuf, 0, TxBuf.Length);
                    }

                    //Save to database
                    try
                    {
                        if (session.SQLStatic == true)
                        {
                            //授时响应
                            SqlCommand command = new SqlCommand();
                            command.Connection = session.SQLConn;
                            command.CommandText = "INSERT INTO [dbo].[GatewayStatus]([DeviceMacS],[ProtocolVersion],[SerialNo],[DeviceTypeS],[GatewayTransDateTime]" +
                                ",[GatewayVoltage],[SwRevisionS] ,[CustomerS],[RamCount],[RomCount],[GSMSignal],[BindingNumber],[TransforNumber],[SimNumber]" +
                                ",[LastSuccessNumber],[LastStatus],[TransStrategy],[ACPower],[SourceData],[SendData]) VALUES(@DeviceMacS,@ProtocolVersion,@SerialNo,@DeviceTypeS,@GatewayTransDateTime, " +
                                "@GatewayVoltage,@SwRevisionS,@CustomerS,@RamCount,@RomCount,@GSMSignal,@BindingNumber,@TransforNumber,@SimNumber," +
                                "@LastSuccessNumber,@LastStatus,@TransStrategy,@ACPower,@SourceData,@SendData)";

                            command.Parameters.Add("@DeviceMAC", SqlDbType.NVarChar);
                            command.Parameters.Add("@ProtocolVersion", SqlDbType.NVarChar);
                            command.Parameters.Add("@SerialNo", SqlDbType.Int);
                            command.Parameters.Add("@DeviceTypeS", SqlDbType.NVarChar);
                            command.Parameters.Add("@GatewayTransDateTime", SqlDbType.DateTime);
                            command.Parameters.Add("@GatewayVoltage", SqlDbType.Decimal);
                            command.Parameters.Add("@SwRevisionS", SqlDbType.NVarChar);
                            command.Parameters.Add("@CustomerS", SqlDbType.NVarChar);
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
                            command.Parameters["@DeviceTypeS"].Value = requestInfo.Body[7].ToString("X2");
                            command.Parameters["@GatewayTransDateTime"].Value = CommArithmetic.DecodeDateTime(requestInfo.Body, 12);
                            command.Parameters["@GatewayVoltage"].Value = CommArithmetic.DecodeVoltage(requestInfo.Body, 18);
                            command.Parameters["@SwRevisionS"].Value = CommArithmetic.DecodeClientID(requestInfo.Body, 20);
                            command.Parameters["@CustomerS"].Value = CommArithmetic.DecodeClientID(requestInfo.Body, 22);
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
                            command.Parameters["@SendData"].Value = CommArithmetic.ToHexString(TxBuf);

                            try
                            {
                                command.ExecuteNonQuery();
                            }
                            catch (Exception)
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
                       + CommArithmetic.ToHexString(TxBuf) + " ");
                }
            }
        }
    }
}
