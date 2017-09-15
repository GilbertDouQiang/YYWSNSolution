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
            ActiveMQHelper mymq = new ActiveMQHelper(isLocalMachine: true, remoteAddress: "",clientID: "clientid");

            mymq.InitQueueOrTopic(topic: false, name: "SensorData", selector: false);
            mymq.SendMessage("BE BE 01 01 02 EB EB");
            mymq.SendMessage("hello26");

            System.Threading.Thread.Sleep(500);
            mymq.ShutDown();
        }
    }
}