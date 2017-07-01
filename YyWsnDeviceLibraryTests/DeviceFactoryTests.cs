using Microsoft.VisualStudio.TestTools.UnitTesting;
using YyWsnDeviceLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace YyWsnDeviceLibrary.Tests
{
    [TestClass()]
    public class DeviceFactoryTests
    {
        [TestMethod()]
        public void CreateDeviceTest()
        {

            string SourceBinary = "【2017-06-30 09:38:26:698】EA 18 01 51 00 06 61 23 45 67 0E 61 00 00 63 1A 64 0C 19 65 0A FA 66 1E 2E 00 D6 1B AE DD ";
            byte[] SourceByte = CommArithmetic.HexStringToByteArray(SourceBinary,25);

            Device device = DeviceFactory.CreateDevice(SourceByte);
            Assert.AreEqual(typeof(M1),device.GetType());
            
        }
    }
}