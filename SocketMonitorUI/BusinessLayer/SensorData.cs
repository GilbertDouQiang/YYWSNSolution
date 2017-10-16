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
                if ( requestInfo.Body.Length == 63 || requestInfo.Body.Length == 60 || requestInfo.Body.Length == 68 || requestInfo.Body.Length == 65)
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
                            //session.SaveToQueue(requestInfo.Body);
                        }

                    }
                    catch (Exception)
                    {



                    }

                    //保存到数据库
                    try
                    {
                        if (session.SQLStatic == true)
                        {

                            SqlCommand command = new SqlCommand();
                            command.Connection = session.SQLConn;

                            if (requestInfo.Body[26] == 0x51 || requestInfo.Body[26] == 0x53)
                            {


                                //授时响应

                                //TODO 是否需要RSSI

                                command.CommandText = "INSERT INTO[dbo].[M1Data] ([DeviceMac],[DeviceSN],[DeviceTransDate],[DeviceVolt],[SensorMac] ,[SensorStatic] ,[SensorFuntction] ,[SensorType]" +
                                    ",[ProtocolVersion],[ICTemperature],[SensorSN],[SensorVolt],[SensorCollectDatetime] ,[SensorTransforDatetime] ,[SensorRSSI]" +
                                    ",[SensorTemperature] ,[SensorHumidity] ,[SourceData]  ,[SendData],[SensorInstant],[SensorRAMCount],[SensorROMCount],[DeviceCSQ]) VALUES(@DeviceMac,@DeviceSN,@DeviceTransDate,@DeviceVolt,@SensorMac ,@SensorStatic ,@SensorFuntction ,@SensorType" +
                                    ",@ProtocolVersion,@ICTemperature,@SensorSN,@SensorVolt,@SensorCollectDatetime ,@SensorTransforDatetime ,@SensorRSSI" +
                                    ",@SensorTemperature ,@SensorHumidity ,@SourceData  ,@SendData,@SensorInstant,@SensorRAMCount,@SensorROMCount,@DeviceCSQ)";

                                command.Parameters.Add("@DeviceMAC", SqlDbType.NVarChar);
                                command.Parameters.Add("@DeviceSN", SqlDbType.Int);
                                command.Parameters.Add("@DeviceTransDate", SqlDbType.DateTime);
                                command.Parameters.Add("@DeviceVolt", SqlDbType.Decimal);

                                command.Parameters.Add("@SensorMac", SqlDbType.NVarChar);
                                command.Parameters.Add("@SensorStatic", SqlDbType.Int);
                                command.Parameters.Add("@SensorFuntction", SqlDbType.Int);
                                command.Parameters.Add("@SensorType", SqlDbType.NVarChar);
                                //line2
                                command.Parameters.Add("@ProtocolVersion", SqlDbType.Int);
                                command.Parameters.Add("@ICTemperature", SqlDbType.Decimal);
                                command.Parameters.Add("@SensorSN", SqlDbType.Int);
                                command.Parameters.Add("@SensorVolt", SqlDbType.Decimal);
                                command.Parameters.Add("@SensorCollectDatetime", SqlDbType.DateTime);
                                command.Parameters.Add("@SensorTransforDatetime", SqlDbType.DateTime);
                                command.Parameters.Add("@SensorRSSI", SqlDbType.Int);

                                //line 3
                                command.Parameters.Add("@SensorTemperature", SqlDbType.Decimal);
                                command.Parameters.Add("@SensorHumidity", SqlDbType.Decimal);
                                command.Parameters.Add("@SourceData", SqlDbType.NVarChar);
                                command.Parameters.Add("@SendData", SqlDbType.NVarChar);

                                command.Parameters.Add("@SensorInstant", SqlDbType.Int);
                                command.Parameters.Add("@SensorRAMCount", SqlDbType.Int);
                                command.Parameters.Add("@SensorROMCount", SqlDbType.Int);
                                command.Parameters.Add("@DeviceCSQ", SqlDbType.Int);



                                //Line1
                                command.Parameters["@DeviceMAC"].Value = CommArithmetic.DecodeMAC(requestInfo.Body, 8); //协议起始位置-1
                                command.Parameters["@DeviceSN"].Value = CommArithmetic.Byte2Int(requestInfo.Body, 5, 2);
                                command.Parameters["@DeviceTransDate"].Value = CommArithmetic.DecodeDateTime(requestInfo.Body, 12);
                                command.Parameters["@DeviceVolt"].Value = CommArithmetic.DecodeVoltage(requestInfo.Body, 18); //协议起始位置-1

                                command.Parameters["@SensorMac"].Value = CommArithmetic.DecodeMAC(requestInfo.Body, 20); //协议起始位置-1
                                command.Parameters["@SensorStatic"].Value = requestInfo.Body[24];
                                command.Parameters["@SensorFuntction"].Value = requestInfo.Body[25];
                                command.Parameters["@SensorType"].Value = requestInfo.Body[26].ToString("X2"); //ok


                                //line 2
                                command.Parameters["@ProtocolVersion"].Value = requestInfo.Body[27].ToString("X2"); //ok
                                command.Parameters["@ICTemperature"].Value = requestInfo.Body[33]; //todo
                                command.Parameters["@SensorSN"].Value = CommArithmetic.Byte2Int(requestInfo.Body, 30, 2);
                                command.Parameters["@SensorVolt"].Value = CommArithmetic.DecodeSensorVoltage(requestInfo.Body, 35);

                                command.Parameters["@SensorCollectDatetime"].Value = CommArithmetic.DecodeDateTime(requestInfo.Body, 38);
                                command.Parameters["@SensorTransforDatetime"].Value = CommArithmetic.DecodeDateTime(requestInfo.Body, 45);
                                command.Parameters["@SensorRSSI"].Value = requestInfo.Body[52] - 256;


                                //line 3
                                command.Parameters["@SensorTemperature"].Value = CommArithmetic.DecodeTemperature(requestInfo.Body, 54);
                                command.Parameters["@SensorHumidity"].Value = CommArithmetic.DecodeHumidity(requestInfo.Body, 57);
                                command.Parameters["@SourceData"].Value = CommArithmetic.ToHexString(requestInfo.Body);
                                command.Parameters["@SendData"].Value = CommArithmetic.ToHexString(response);

                                command.Parameters["@SensorInstant"].Value = requestInfo.Body[59];
                                command.Parameters["@SensorRAMCount"].Value = requestInfo.Body[60];
                                command.Parameters["@SensorROMCount"].Value = CommArithmetic.Byte2Int(requestInfo.Body, 61, 2);
                                command.Parameters["@DeviceCSQ"].Value = requestInfo.Body[63];



                                try
                                {
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {


                                }
                            }

                            if (requestInfo.Body[26] == 0x57)
                            {


                                //授时响应

                                //TODO 是否需要RSSI

                                command.CommandText = "INSERT INTO[dbo].[M2Data] ([DeviceMac],[DeviceSN],[DeviceTransDate],[DeviceVolt],[SensorMac] ,[SensorStatic] ,[SensorFuntction] ,[SensorType]" +
                                    ",[ProtocolVersion],[ICTemperature],[SensorSN],[SensorVolt],[SensorCollectDatetime] ,[SensorTransforDatetime] ,[SensorRSSI]" +
                                    ",[SensorTemperature] ,[SourceData]  ,[SendData],[SensorInstant],[SensorRAMCount],[SensorROMCount],[DeviceCSQ]) VALUES(@DeviceMac,@DeviceSN,@DeviceTransDate,@DeviceVolt,@SensorMac ,@SensorStatic ,@SensorFuntction ,@SensorType" +
                                    ",@ProtocolVersion,@ICTemperature,@SensorSN,@SensorVolt,@SensorCollectDatetime ,@SensorTransforDatetime ,@SensorRSSI" +
                                    ",@SensorTemperature  ,@SourceData  ,@SendData,@SensorInstant,@SensorRAMCount,@SensorROMCount,@DeviceCSQ)";

                                command.Parameters.Add("@DeviceMAC", SqlDbType.NVarChar);
                                command.Parameters.Add("@DeviceSN", SqlDbType.Int);
                                command.Parameters.Add("@DeviceTransDate", SqlDbType.DateTime);
                                command.Parameters.Add("@DeviceVolt", SqlDbType.Decimal);

                                command.Parameters.Add("@SensorMac", SqlDbType.NVarChar);
                                command.Parameters.Add("@SensorStatic", SqlDbType.Int);
                                command.Parameters.Add("@SensorFuntction", SqlDbType.Int);
                                command.Parameters.Add("@SensorType", SqlDbType.NVarChar);
                                //line2
                                command.Parameters.Add("@ProtocolVersion", SqlDbType.Int);
                                command.Parameters.Add("@ICTemperature", SqlDbType.Decimal);
                                command.Parameters.Add("@SensorSN", SqlDbType.Int);
                                command.Parameters.Add("@SensorVolt", SqlDbType.Decimal);
                                command.Parameters.Add("@SensorCollectDatetime", SqlDbType.DateTime);
                                command.Parameters.Add("@SensorTransforDatetime", SqlDbType.DateTime);
                                command.Parameters.Add("@SensorRSSI", SqlDbType.Int);

                                //line 3
                                command.Parameters.Add("@SensorTemperature", SqlDbType.Decimal);
                                
                                command.Parameters.Add("@SourceData", SqlDbType.NVarChar);
                                command.Parameters.Add("@SendData", SqlDbType.NVarChar);

                                command.Parameters.Add("@SensorInstant", SqlDbType.Int);
                                command.Parameters.Add("@SensorRAMCount", SqlDbType.Int);
                                command.Parameters.Add("@SensorROMCount", SqlDbType.Int);
                                command.Parameters.Add("@DeviceCSQ", SqlDbType.Int);



                                //Line1
                                command.Parameters["@DeviceMAC"].Value = CommArithmetic.DecodeMAC(requestInfo.Body, 8); //协议起始位置-1
                                command.Parameters["@DeviceSN"].Value = CommArithmetic.Byte2Int(requestInfo.Body, 5, 2);
                                command.Parameters["@DeviceTransDate"].Value = CommArithmetic.DecodeDateTime(requestInfo.Body, 12);
                                command.Parameters["@DeviceVolt"].Value = CommArithmetic.DecodeVoltage(requestInfo.Body, 18); //协议起始位置-1

                                command.Parameters["@SensorMac"].Value = CommArithmetic.DecodeMAC(requestInfo.Body, 20); //协议起始位置-1
                                command.Parameters["@SensorStatic"].Value = requestInfo.Body[24];
                                command.Parameters["@SensorFuntction"].Value = requestInfo.Body[25];
                                command.Parameters["@SensorType"].Value = requestInfo.Body[26].ToString("X2"); //ok


                                //line 2
                                command.Parameters["@ProtocolVersion"].Value = requestInfo.Body[27].ToString("X2"); //ok
                                command.Parameters["@ICTemperature"].Value = requestInfo.Body[33]; //todo
                                command.Parameters["@SensorSN"].Value = CommArithmetic.Byte2Int(requestInfo.Body, 30, 2);
                                command.Parameters["@SensorVolt"].Value = CommArithmetic.DecodeSensorVoltage(requestInfo.Body, 35);

                                command.Parameters["@SensorCollectDatetime"].Value = CommArithmetic.DecodeDateTime(requestInfo.Body, 38);
                                command.Parameters["@SensorTransforDatetime"].Value = CommArithmetic.DecodeDateTime(requestInfo.Body, 45);
                                command.Parameters["@SensorRSSI"].Value = requestInfo.Body[52] - 256;


                                //line 3
                                command.Parameters["@SensorTemperature"].Value = CommArithmetic.DecodeTemperature(requestInfo.Body, 54);
                                //command.Parameters["@SensorHumidity"].Value = CommArithmetic.DecodeHumidity(requestInfo.Body, 57);
                                command.Parameters["@SourceData"].Value = CommArithmetic.ToHexString(requestInfo.Body);
                                command.Parameters["@SendData"].Value = CommArithmetic.ToHexString(response);

                                command.Parameters["@SensorInstant"].Value = requestInfo.Body[56];
                                command.Parameters["@SensorRAMCount"].Value = requestInfo.Body[57];
                                command.Parameters["@SensorROMCount"].Value = CommArithmetic.Byte2Int(requestInfo.Body, 58, 2);
                                command.Parameters["@DeviceCSQ"].Value = requestInfo.Body[60];



                                try
                                {
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {


                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {

                        
                    }


                    Logger.AddLog(DateTime.Now.ToString("HH:mm:ss.fff") + " :SendData:" + session.RemoteEndPoint.Address.ToString() + " :\t"
                       + CommArithmetic.ToHexString(response) + " ");
                }

               



               

            }



        }
    }
}
