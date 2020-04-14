using Microsoft.VisualStudio.TestTools.UnitTesting;
using YyWsnDeviceLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using YyWsnCommunicatonLibrary;

namespace YyWsnDeviceLibrary.Tests
{
    [TestClass()]
    public class UartGatewayCommandTests
    {
        [TestMethod()]
        public void ReadInfoTest()
        {

            SerialPortHelper Serial = new SerialPortHelper();

            Serial.InitCOM("COM5");
            Serial.OpenPort();

            byte[] TxBuf = { 1, 2, 3, 4 };

            byte[] RxBuf = Serial.SendReceive(TxBuf, 0, (UInt16)TxBuf.Length, 500);

            Serial.ClosePort();

            Assert.AreEqual("F1 76 D7 DB", RxBuf);
        }
    }
}