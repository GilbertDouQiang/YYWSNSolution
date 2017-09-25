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


                                 //EA 19 01 53 01 D3 9A 11 11 11 11 0E 61 00 D9 63 18 64 0B A3 65 09 48 66 1A 9C 00 98 EE AE D9 
            
            //模式1 发出的数据，兼容Z协议
            string SourceBinary = "EA 19 01 53 01 D3 9A 11 11 11 11 0E 61 00 D9 63 18 64 0B A3 65 09 48 66 1A 9C 00 98 EE AE D9 ";
            byte[] SourceByte = CommArithmetic.HexStringToByteArray(SourceBinary, 0);

            Device device = DeviceFactory.CreateDevice(SourceByte);
            Assert.AreEqual(typeof(M1), device.GetType());
            Assert.AreEqual(((M1)device).DeviceMac , "11 11 11 11 ");


            //上电自检发出的数据
            SourceBinary = "EC 4D 01 01 51 02 11 76 B8 4A F1 76 B8 4A FF FF FF FF A1 05 D3 9A 00 02 00 00 0A 17 04 01 23 59 59 01 00 0D 01 00 17 70 F0 60 23 28 03 E8 1B 58 EC 78 25 1C 01 F4 00 00 00 00 1A 0B CE 00 00 04 00 00 00 00 00 00 00 00 00 0A CA 1C 51 00 E1 47 E7 CE";
            SourceByte = CommArithmetic.HexStringToByteArray(SourceBinary, 0);
            device = DeviceFactory.CreateDevice(SourceByte);
            Assert.AreEqual(typeof(M1), device.GetType());

        }



        [TestMethod()]
        public void CreateDeviceTest2()
        {


            //EA 18 01 02 00 52 00 27 70 10 0F 02 00 8D 0B 02 6C 15 0C 02 84 18 09 02 03 04 AE DB
            //EA 19 01 53 01 D3 9A 11 11 11 11 0E 61 00 D9 63 18 64 0B A3 65 09 48 66 1A 9C 00 98 EE AE D9

            //模式1 发出的数据，兼容Z协议
            string SourceBinary = "EA 18 01 02 00 52 00 27 70 10 0F 02 00 8D 0B 02 6C 15 0C 02 84 18 09 02 03 04 AE DB ";
            byte[] SourceByte = CommArithmetic.HexStringToByteArray(SourceBinary, 0);

            Device device = DeviceFactory.CreateDevice(SourceByte);
            Assert.AreEqual(typeof(M1), device.GetType());
            Assert.AreEqual(((M1)device).DeviceMac, "11 11 11 11 ");


            //上电自检发出的数据
            SourceBinary = "EC 4D 01 01 51 02 11 76 B8 4A F1 76 B8 4A FF FF FF FF A1 05 D3 9A 00 02 00 00 0A 17 04 01 23 59 59 01 00 0D 01 00 17 70 F0 60 23 28 03 E8 1B 58 EC 78 25 1C 01 F4 00 00 00 00 1A 0B CE 00 00 04 00 00 00 00 00 00 00 00 00 0A CA 1C 51 00 E1 47 E7 CE";
            SourceByte = CommArithmetic.HexStringToByteArray(SourceBinary, 0);
            device = DeviceFactory.CreateDevice(SourceByte);
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