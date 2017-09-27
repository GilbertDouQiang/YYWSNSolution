using Microsoft.VisualStudio.TestTools.UnitTesting;
using YyWsnCommunicatonLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YyWsnCommunicatonLibrary.Tests
{
    [TestClass()]
    public class ActiveMQHelperTests
    {
        [TestMethod()]
        public void SendMessageTest()
        {
            ActiveMQHelper mymq = new ActiveMQHelper(isLocalMachine: true, remoteAddress: "");

            mymq.InitQueueOrTopic(topic: true, name: "HyperWSNQueue", selector: false);
            //mymq.SendMessage("BE BE 01 01 02 EB EB");
            //mymq.SendMessage("hello26");

            System.Threading.Thread.Sleep(500);
            mymq.ShutDown();
        }



        [TestMethod()]
        public void ReceiveTest()
        {
            ActiveMQHelper mymq = new ActiveMQHelper(isLocalMachine: true, remoteAddress: "");
            mymq.ClientID = "HyperWSNTopicClient";
            mymq.InitQueueOrTopic(topic: true, name: "HyperWSNQueue", selector: false);
            //mymq.SendMessage("hello5");
            //mymq.SendMessage("hello26");
            byte[] receiveBytes = mymq.GetMessage();



            System.Threading.Thread.Sleep(500);
            mymq.ShutDown();
        }


    }
}