﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using YyWsnCommunicatonLibrary;

namespace HyperWSN.Socket
{
    public class HyperWSNSession : AppSession<HyperWSNSession, BinaryRequestInfo>
    {
        ActiveMQHelper mymq;// = new ActiveMQHelper(isLocalMachine: true, remoteAddress: "");

        public new HyperWSNSocketServer AppServer
        {
            get
            {
                return (HyperWSNSocketServer)base.AppServer;
            }
        }

        public void ConnectQueue(string QueueName)
        {
            try
            {
                mymq = new ActiveMQHelper(isLocalMachine: true, remoteAddress: "");
                mymq.InitQueueOrTopic(topic: false, name: QueueName, selector: false);
                QueueStatic = true;
            }
            catch (Exception)
            {

                mymq = null;
            }
           

        }

        public void DisconnectQueue()
        {
            if (mymq!=null)
            {
                mymq.ShutDown();
            }
        }

        public void SaveToQueue(string StringData)
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

        public bool QueueStatic { get; set; }

    }
}
