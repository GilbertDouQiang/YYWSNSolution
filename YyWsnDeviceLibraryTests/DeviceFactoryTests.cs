using Microsoft.VisualStudio.TestTools.UnitTesting;
using YyWsnDeviceLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace YyWsnDeviceLibrary.Tests
{
    [TestClass()]
    public class DeviceFactoryTests
    {
        [TestMethod()]
        public void CreateDeviceTest()
        {

            string SourceBinary = "EA 19 01 51 01 00 06 61 23 45 67 0E 61 00 00 63 1A 64 0C 19 65 0A FA 66 1E 2E 00 D6 1B AE DD ";
            byte[] SourceByte = CommArithmetic.HexStringToByteArray(SourceBinary, 0);

            Device device = DeviceFactory.CreateDevice(SourceByte);
            Assert.AreEqual(typeof(M1), device.GetType());

        }


        [TestMethod()]
        public void CreateDeviceUartGateway()
        {

            string SourceBinary = "AC AC 24 01 F1 76 D7 DB FF FF FF FF 53 03 00 00 00 00 00 00 1E 16 04 01 23 59 59 01 01 00 00 00 00 00 00 00 01 00 00 BF 79 CA CA";
            byte[] SourceByte = CommArithmetic.HexStringToByteArray(SourceBinary, 0);

            Device device = DeviceFactory.CreateDevice(SourceByte);
            Assert.AreEqual(typeof(UartGateway), device.GetType());

        }

        [TestMethod()]
        public void CreateDevicesTest()
        {
            string SourceBinary = "01 01 02 03 EA 19 01 51 01 00 06 20 17 07 07 0E 61 01 06 63 17 64 0B EE 65 0A 32 66 1C 7C 00 3F 5F AE D3 EA 19 01 51 01 00 06 20 17 07 07 0E 61 01 07 63 17 64 0B EE 65 0A 30 66 1C 80 00 68 13 AE D5";
            byte[] SourceByte = CommArithmetic.HexStringToByteArray(SourceBinary, 0);

            ObservableCollection<Device>  devices = DeviceFactory.CreateDevices(SourceByte);
        }
    }
}