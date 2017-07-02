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
            
            SerialPortHelper serial = new SerialPortHelper();
            serial.InitCOM("COM10");
            serial.OpenPort();

            UartGatewayCommand command = new UartGatewayCommand();
            byte[] resultBytes= serial.SendCommand(command.ReadInfo(),500);

            UartGateway device = (UartGateway)DeviceFactory.CreateDevice(resultBytes);

            Assert.AreEqual("F1 76 D7 DB ", device.DeviceMac);
            serial.ClosePort();


            
        }
    }
}