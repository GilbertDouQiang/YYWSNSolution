using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YyWsnDeviceLibrary
{
    public class M2 : Sensor
    {
        public double Temperature { get; set; }

        public M2()
        {

        }

        public M2(byte[] SourceData)
        {


            //2017版协议 v3.5
            //判断第三位 ，01 代表从传感器发出的正常数据，长度不是固定值



            //上电自检数据
            if (SourceData.Length == 0x46)
            {
                if (SourceData[4] == 0x57)
                {
                    Name = "M2";
                }
               
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

                TemperatureInfoHigh = CommArithmetic.DecodeTemperature(SourceData, 38);
                TemperatureInfoLow = CommArithmetic.DecodeTemperature(SourceData, 40);

                //HumidityInfoHigh = CommArithmetic.DecodeHumidity(SourceData, 42);
                //HumidityInfoLow = CommArithmetic.DecodeHumidity(SourceData, 44);

                TemperatureWarnHigh = CommArithmetic.DecodeTemperature(SourceData, 42);
                TemperatureWarnLow = CommArithmetic.DecodeTemperature(SourceData, 44);

                //HumidityWarnHigh = CommArithmetic.DecodeHumidity(SourceData, 50);
                //HumidityWarnLow = CommArithmetic.DecodeHumidity(SourceData, 52);

                TemperatureCompensation = CommArithmetic.DecodeTemperature(SourceData, 46);
                //HumidityCompensation = CommArithmetic.DecodeHumidity(SourceData, 56);

                ICTemperature = SourceData[48];
                Volt = Math.Round(Convert.ToDouble((SourceData[49] * 256 + SourceData[50])) / 1000, 2);

                MaxLength = SourceData[53];

                //Temperature = CommArithmetic.DecodeTemperature(SourceData, 73);
                //Humidity = CommArithmetic.DecodeHumidity(SourceData, 75);
                RSSI = SourceData[66] - 256;

                //Falsh
                FlashID = CommArithmetic.DecodeClientID(SourceData, 51);
                FlashFront = SourceData[54] * 256 * 256 + SourceData[55] * 256 + SourceData[56];
                FlashRear = SourceData[57] * 256 * 256 + SourceData[58] * 256 + SourceData[59];
                FlashQueueLength = SourceData[60] * 256 * 256 + SourceData[61] * 256 + SourceData[62];


            }


            //兼容模式，兼容Z模式
            if (SourceData.Length == 29)
            {
                Name = "M2";
                DeviceID = "57";
                DeviceMac = CommArithmetic.DecodeMAC(SourceData, 5);
                ClientID = CommArithmetic.DecodeClientID(SourceData, 3);
                WorkFunction = SourceData[2];
                ProtocolVersion = 0x00;

                SensorSN = SourceData[12] * 256 + SourceData[13];
                //传感器信息
                ICTemperature = 0; //老协议中没有IC 温度


                Volt = CommArithmetic.SHT20Voltage(SourceData[24], SourceData[25]);


                //Temperature = CommArithmetic.SHT20Temperature(SourceData[16], SourceData[17]);


                //Humidity = CommArithmetic.SHT20Humidity(SourceData[20], SourceData[21]);
                //广播模式，补充采集和传输时间
                SensorCollectTime = System.DateTime.Now;
                SensorTransforTime = System.DateTime.Now;
                //RSSI = SourceData[30] / 2 - 138; //1101 方案
                //可能收到没有RSSI的数据
                if (SourceData.Length == 28)
                {
                    RSSI = SourceData[27] - 256;
                }
                this.SourceData = CommArithmetic.ToHexString(SourceData);


            }


            //模式1 正常传输的数据，
            if (SourceData.Length == 28)
            {

                //将收到的数据填充到属性
                Name = "M2";
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
                Temperature = Math.Round((Convert.ToDouble(tempCalc) / 100), 2);


                //Humidity = Math.Round(Convert.ToDouble((SourceData[24] * 256 + SourceData[25])) / 100, 2);
                //广播模式，补充采集和传输时间
                SensorCollectTime = System.DateTime.Now;
                SensorTransforTime = System.DateTime.Now;
                //RSSI = SourceData[30] / 2 - 138; //1101 方案
                //可能收到没有RSSI的数据
                if (SourceData.Length == 28)
                {
                    RSSI = SourceData[27] - 256;
                }
                this.SourceData = CommArithmetic.ToHexString(SourceData);


            }




        }
        /// <summary>
        /// 温度预警上限
        /// </summary>
        public double TemperatureInfoHigh { get; set; }
        /// <summary>
        /// 温度预警下限
        /// </summary>
        public double TemperatureInfoLow { get; set; }

        /// <summary>
        /// 温度报警上限
        /// </summary>
        public double TemperatureWarnHigh { get; set; }

        /// <summary>
        /// 温度报警下限
        /// </summary>
        public double TemperatureWarnLow { get; set; }

        /// <summary>
        /// 湿度预警上限
        /// </summary>
        public double HumidityInfoHigh { get; set; }
        /// <summary>
        /// 湿度预警下限
        /// </summary>
        public double HumidityInfoLow { get; set; }

        /// <summary>
        /// 湿度报警上限
        /// </summary>
        public double HumidityWarnHigh { get; set; }

        /// <summary>
        /// 湿度报警下限
        /// </summary>
        public double HumidityWarnLow { get; set; }

        /// <summary>
        /// 温度补偿
        /// </summary>
        public double TemperatureCompensation { get; set; }

        /// <summary>
        /// 湿度补偿
        /// </summary>
        public double HumidityCompensation { get; set; }

    }
}
