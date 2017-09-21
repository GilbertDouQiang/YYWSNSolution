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
        public void StartService()
        {
            //连接到数据库

            //启动MQ
            saveTimer = new System.Timers.Timer();
            saveTimer.Interval = SaveInterval;
            saveTimer.Elapsed += SaveTimer_Elapsed;
            saveTimer.Enabled = true;

            //定时将MQ数据录入数据库


        }

        private void SaveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            saveTimer.Enabled = false;
            //连接MQ , 如果不能连接，等待下一次连接
            try
            {
                mq = new ActiveMQHelper(isLocalMachine: true, remoteAddress: "", clientID: ClientID);
                mq.InitQueueOrTopic(topic: false, name: QueueName, selector: false);

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
                saveTimer.Enabled = true;
                return;

            }
               

            //开始接收数据


            byte[] result =mq.GetMessage();
            while(result.Length>0)
            {
                //先判断收到数据的属性（命令）
                if (result.Length<=6)
                {

                    result = mq.GetMessage();
                    continue;


                }

                if (result[3] ==0x91)
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


                    
                    command.Parameters["@DeviceMAC"].Value = CommArithmetic.DecodeMAC(result, 7);
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

                //收到数据，分析数据，写入数据库
                Device device = DeviceFactory.CreateDevice(result);
                if (device==null)
                {
                    result = mq.GetMessage();
                    continue;

                }

                

                if (device.GetType()==typeof(M1))
                {

                }
                



                Thread.Sleep(10);
                result = mq.GetMessage();

            }





            saveTimer.Enabled = true;
        }

        public void StopService()
        {
            //关闭数据连接，停止服务
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
