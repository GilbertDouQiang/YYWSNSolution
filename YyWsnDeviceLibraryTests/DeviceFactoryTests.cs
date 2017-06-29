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

            string SourceBinary = "EA 18 01 51 00 06 61 23 45 67 0E 61 01 6B 63 17 64 0B EE 65 0A 5C 66 1E 5E 00 E1 3B AE 72";
            byte[] SourceByte = CommArithmetic.HexStringToByteArray(SourceBinary);

            Device device = DeviceFactory.CreateDevice(SourceByte);
            Assert.AreEqual(typeof(M1),device.GetType());
            //Assert.Fail();
        }
    }
}