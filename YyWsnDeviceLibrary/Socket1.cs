using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YyWsnDeviceLibrary
{

    /// <summary>
    /// Socket1 M4 智能插座
    /// </summary>
    public class Socket1 : Sensor
    {
        public Socket1()
        {

        }

        public Socket1(byte[] SourceData)
        {
            //上电自检数据
            if (SourceData.Length == 82)
            {
                Name = "Socket1";
                DeviceID = SourceData[4].ToString("X2");
                ProtocolVersion = SourceData[5];
                PrimaryMAC = CommArithmetic.DecodeMAC(SourceData, 6);
                DeviceMac = CommArithmetic.DecodeMAC(SourceData, 10);
                HardwareVersion = CommArithmetic.DecodeMAC(SourceData, 14);
                SoftwareVersion = CommArithmetic.DecodeClientID(SourceData, 18);
                ClientID = CommArithmetic.DecodeClientID(SourceData, 20);
                byte[] debugBytes = new byte[2];
                debugBytes[0] = SourceData[22];
                debugBytes[1] = SourceData[23];

                DebugString = CommArithmetic.DecodeClientID(SourceData, 22);

                Debug = debugBytes;
                Category = SourceData[24];
                Interval = SourceData[25] * 256 + SourceData[26];
                Calendar = CommArithmetic.DecodeDateTime(SourceData, 27);


                WorkFunction = SourceData[33];
                SymbolRate = SourceData[34];
                TXPower = SourceData[35];
                TXTimers = SourceData[36];
                Frequency = SourceData[37];



                ICTemperature = SourceData[58];
                Volt = Math.Round(Convert.ToDouble((SourceData[59] * 256 + SourceData[60])) / 1000, 2);

                MaxLength = SourceData[63];

              
                RSSI = SourceData[78] - 256;

                //Falsh
                FlashID = CommArithmetic.DecodeClientID(SourceData, 61);
                FlashFront = SourceData[64] * 256 * 256 + SourceData[65] * 256 + SourceData[66];
                FlashRear = SourceData[67] * 256 * 256 + SourceData[68] * 256 + SourceData[69];
                FlashQueueLength = SourceData[70] * 256 * 256 + SourceData[71] * 256 + SourceData[72];



            }


            //模式1 正常传输的数据，兼容原Z版本
            if (SourceData.Length == 31)
            {

                //将收到的数据填充到属性
                Name = "Socket1";
                DeviceID = SourceData[3].ToString("X2");
                DeviceMac = CommArithmetic.DecodeMAC(SourceData, 7);
                ClientID = CommArithmetic.DecodeClientID(SourceData, 5);
                WorkFunction = SourceData[2];
                ProtocolVersion = SourceData[4];

                SensorSN = SourceData[13] * 256 + SourceData[14];
                //传感器信息
                ICTemperature = SourceData[16]; //todo : 有符号整形数
                if (ICTemperature >= 128)
                    ICTemperature -= 256;

                Volt = Math.Round(Convert.ToDouble((SourceData[18] * 256 + SourceData[19])) / 1000, 2);
                int tempCalc = SourceData[21] * 256 + SourceData[22];
                if (tempCalc >= 0x8000)
                    tempCalc -= 65536;
                //Temperature = Math.Round((Convert.ToDouble(tempCalc) / 100), 2);


                //Humidity = Math.Round(Convert.ToDouble((SourceData[24] * 256 + SourceData[25])) / 100, 2);
                //广播模式，补充采集和传输时间
                SensorCollectTime = System.DateTime.Now;
                SensorTransforTime = System.DateTime.Now;
                //RSSI = SourceData[30] / 2 - 138; //1101 方案
                //可能收到没有RSSI的数据
                if (SourceData.Length == 31)
                {
                    RSSI = SourceData[30] - 256;
                }
                this.SourceData = CommArithmetic.ToHexString(SourceData);


            }



        }

        public byte[] UpdateFactory()
        {
            byte[] updateBytes = new byte[21];
            updateBytes[0] = 0xCE;
            updateBytes[1] = 0x10;
            updateBytes[2] = 0xA1;
            updateBytes[3] = 0x01;
            updateBytes[4] = 0x58;
            updateBytes[5] = 0x01;

            byte[] deviceMacBytes = CommArithmetic.HexStringToByteArray(DeviceMac);
            updateBytes[6] = deviceMacBytes[0];
            updateBytes[7] = deviceMacBytes[1];
            updateBytes[8] = deviceMacBytes[2];
            updateBytes[9] = deviceMacBytes[3];

            deviceMacBytes = CommArithmetic.HexStringToByteArray(DeviceNewMAC);
            updateBytes[10] = deviceMacBytes[0];
            updateBytes[11] = deviceMacBytes[1];
            updateBytes[12] = deviceMacBytes[2];
            updateBytes[13] = deviceMacBytes[3];

            deviceMacBytes = CommArithmetic.HexStringToByteArray(HardwareVersion);
            updateBytes[14] = deviceMacBytes[0];
            updateBytes[15] = deviceMacBytes[1];
            updateBytes[16] = deviceMacBytes[2];
            updateBytes[17] = deviceMacBytes[3];


            updateBytes[18] = 0x00;
            updateBytes[19] = 0x00;
            updateBytes[20] = 0xEC;

            //updateBytes[0] = 0xCE;




            return updateBytes;
        }

    }
}
