using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data; // State variables 
using System.Data.SqlClient; // Database 
using System.Globalization; // Date 

using System.Timers;

using YyWsnCommunicatonLibrary;
using YyWsnDeviceLibrary;

using System.Threading;

namespace DeviceSQLDatabaseLibrary
{
    public class DatabaseService
    {

        private System.Timers.Timer saveTimer;
        private ActiveMQHelper mq;
        private SqlConnection myConn;
        private bool serviceStatic;
        public void StartService()
        {
            

            //启动MQ
            saveTimer = new System.Timers.Timer();
            saveTimer.Interval = SaveInterval;
            saveTimer.Elapsed += SaveTimer_Elapsed;
            saveTimer.Enabled = true;
            serviceStatic = true;

            //定时将MQ数据录入数据库


        }

        private void SaveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            saveTimer.Enabled = false;
            //连接MQ , 如果不能连接，等待下一次连接

            //问题描述：
            //猜测可能因为ClientID重复，导致无法多次连接，用随机数及解决
            Random ra = new Random();
            int id =  ra.Next(1, 10000000);
            ClientID = "Receive" + id.ToString();


            try
            {
                mq = new ActiveMQHelper(isLocalMachine: true, remoteAddress: "");
                mq.ClientID = "HyperWSNTopicClient";
                mq.InitQueueOrTopic(topic: true, name: "HyperWSNQueue", selector: false);

            }
            catch (Exception)
            {
                saveTimer.Enabled = true;
                return;
            }

            //连接数据库，如果无法连接，等待下一次连接
            myConn = new SqlConnection(ConnectionString);

            try
            {
                myConn.Open();
            }
            catch (Exception)
            {
                mq.ShutDown();
                saveTimer.Enabled = true;
                return;

            }


            //开始接收数据
            byte[] result;
            if (serviceStatic == true)
            {
                result = mq.GetMessage();

            }
            else
            {
                saveTimer.Enabled = true;
                return;
            }


            while (serviceStatic && result != null)
            {
                //先判断收到数据的属性（命令）
                if (result.Length <= 6)
                {

                    result = mq.GetMessage();
                    continue;


                }

                if (result[3] == 0x91)
                {
                    //授时响应
                    SqlCommand command = new SqlCommand();
                    command.Connection = myConn;
                    command.CommandText = "INSERT INTO [dbo].[NTPStatus]([DeviceMAC],[ProtocolVersion],[NTPStatus],[Sysdatetime],[SourceData]) VALUES (" +
                        " @DeviceMAC,@ProtocolVersion,@NTPStatus,@Sysdatetime,@SourceData)";
                    command.Parameters.Add("@DeviceMAC", SqlDbType.VarChar);
                    command.Parameters.Add("@ProtocolVersion", SqlDbType.VarChar);
                    command.Parameters.Add("@NTPStatus", SqlDbType.Int);
                    command.Parameters.Add("@Sysdatetime", SqlDbType.DateTime);
                    command.Parameters.Add("@SourceData", SqlDbType.VarChar);



                    command.Parameters["@DeviceMAC"].Value = CommArithmetic.DecodeMAC(result, 7);//协议起始位置-1
                    command.Parameters["@ProtocolVersion"].Value = result[4].ToString("X2");
                    command.Parameters["@NTPStatus"].Value = result[17];
                    command.Parameters["@Sysdatetime"].Value = System.DateTime.Now;
                    command.Parameters["@SourceData"].Value = CommArithmetic.ToHexString(result);

                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception)
                    {



                    }

                }


                //0x92 Gateway Status
                if (result[3] == 0x92)
                {
                    //授时响应
                    SqlCommand command = new SqlCommand();
                    command.Connection = myConn;
                    command.CommandText = "INSERT INTO [dbo].[GatewayStatus]([DeviceMac],[ProtocolVersion],[SerialNo],[DeviceType],[GatewayTransDateTime]" +
                        ",[GatewayVoltage],[SoftwareVersion] ,[ClientID],[RamCount],[RomCount],[GSMSignal],[BindingNumber],[TransforNumber],[SimNumber]" +
                        ",[LastSuccessNumber],[LastStatus]) VALUES(@DeviceMac,@ProtocolVersion,@SerialNo,@DeviceType,@GatewayTransDateTime, " +
                        "@GatewayVoltage,@SoftwareVersion,@ClientID,@RamCount,@RomCount,@GSMSignal,@BindingNumber,@TransforNumber,@SimNumber," +
                        "@LastSuccessNumber,@LastStatus)";

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


                    command.Parameters["@DeviceMAC"].Value = CommArithmetic.DecodeMAC(result, 8); //协议起始位置-1
                    command.Parameters["@ProtocolVersion"].Value = result[4].ToString("X2");
                    command.Parameters["@SerialNo"].Value = CommArithmetic.Byte2Int(result, 5, 2);
                    command.Parameters["@DeviceType"].Value = result[7].ToString("X2");
                    command.Parameters["@GatewayTransDateTime"].Value = CommArithmetic.DecodeDateTime(result, 12);
                    command.Parameters["@GatewayVoltage"].Value = CommArithmetic.DecodeVoltage(result, 18);
                    command.Parameters["@SoftwareVersion"].Value = CommArithmetic.DecodeClientID(result, 20);
                    command.Parameters["@ClientID"].Value = CommArithmetic.DecodeClientID(result, 22);
                    command.Parameters["@RamCount"].Value = result[24];
                    command.Parameters["@RomCount"].Value = CommArithmetic.Byte2Int(result, 25, 3);
                    command.Parameters["@GSMSignal"].Value = result[30];
                    command.Parameters["@BindingNumber"].Value = result[32];
                    command.Parameters["@TransforNumber"].Value = result[33];
                    command.Parameters["@SimNumber"].Value = CommArithmetic.DecodeMAC(result, 35);
                    command.Parameters["@LastSuccessNumber"].Value = CommArithmetic.Byte2Int(result, 40, 3);
                    command.Parameters["@LastStatus"].Value = result[40];


                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception)
                    {
                    }
                }

                Thread.Sleep(10);
                result = mq.GetMessage();
            }

            mq.ShutDown();
            saveTimer.Enabled = true;
        }

        public void StopService()
        {
            //关闭数据连接，停止服务
            serviceStatic = false;
            Thread.Sleep(250);
            saveTimer.Enabled = false;
            saveTimer.Dispose();
            if (mq!=null)
            {
                mq = null;
            }
            if (myConn!=null)
            {
                myConn = null;
            }


        }

        public string ConnectionString { get; set; }
        public int SaveInterval { get; set; }

        public string ClientID { get; set; }
        public string QueueName { get; set; }


    }

}
