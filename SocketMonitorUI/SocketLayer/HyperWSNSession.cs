using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using YyWsnCommunicatonLibrary;

using System.Data; // State variables 
using System.Data.SqlClient; // Database 

using System.Configuration;


namespace HyperWSN.Socket
{
    public class HyperWSNSession : AppSession<HyperWSNSession, BinaryRequestInfo>
    {
        ActiveMQHelper mymq;// = new ActiveMQHelper(isLocalMachine: true, remoteAddress: "");
        public SqlConnection SQLConn;

        public new HyperWSNSocketServer AppServer
        {
            get
            {
                return (HyperWSNSocketServer)base.AppServer;
            }
        }

        public void ConnectQueue(string QueueName, string ClientID)
        {
            try
            {
                mymq = new ActiveMQHelper(isLocalMachine: true, remoteAddress: "");
                mymq.ClientID = "HyperWSNTopicClient"; ;
                mymq.InitQueueOrTopic(topic: true, name: QueueName, selector: false);
                QueueStatic = true;
            }
            catch (Exception)
            {

                mymq = null;
            }


        }

        public void DisconnectQueue()
        {
            if (mymq != null)
            {
                mymq.ShutDown();
            }
        }

        public void SaveToQueue(byte[] StringData)
        {
            try
            {
                if (mymq != null)
                {
                    //save data to queue
                    mymq.SendMessage(StringData);
                }
            }
            catch (Exception)
            {


            }
        }

        public void ConnectSQLServer()
        {
            try
            {
                string conn = ConfigurationManager.AppSettings["ConnectionString"];
                SQLConn = new SqlConnection(conn);
                SQLConn.Open();
                SQLStatic = true;

            }
            catch (Exception ex)
            {

                SQLStatic = false;
                throw ex;
            }
        }


        public void DisConnectSQLServer()
        {
            try
            {
                if (SQLConn != null)
                {
                    if (SQLConn.State == ConnectionState.Open)
                    {
                        SQLConn.Close();
                    }
                    SQLConn.Dispose();
                    SQLConn = null;
                }

                SQLStatic = false;

            }
            catch (Exception)
            {

                SQLStatic = false;
            }
        }

        public bool QueueStatic { get; set; }

        public bool SQLStatic { get; set; }

    }
}
