using Microsoft.VisualStudio.TestTools.UnitTesting;
using YyWsnCommunicatonLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace YyWsnCommunicatonLibrary.Tests
{
    [TestClass()]
    public class SerialPortHelperTests
    {
        [TestMethod()]
        public void UartCommTest()
        {
            SerialPortHelper serial = new SerialPortHelper();
            serial.SerialPortReceived += Serial_SerialPortReceived;
            serial.InitCOM("COM3");
            serial.OpenPort();

            serial.Send("00 CA 00 00");

            Thread.Sleep(2000);      
        }

        private void Serial_SerialPortReceived(object sender, SerialPortEventArgs e)
        {
           
        }
    }
}