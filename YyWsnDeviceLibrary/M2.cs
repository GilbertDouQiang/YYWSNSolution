using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YyWsnDeviceLibrary
{
    public class M2: Sensor
    {
        public double Temperature { get; set; }

        public M2()
        {

        }

        public M2(byte[] SrcData)
        {        
            //2017版协议 v3.5
            //判断第三位 ，01 代表从传感器发出的正常数据，长度不是固定值

            //上电自检数据
            if (SrcData.Length == 0x46)
            {
                SetDeviceName(SrcData[4]);
                ProtocolVersion = SrcData[5];
                SetDevicePrimaryMac(SrcData, 6);
                SetDeviceMac(SrcData, 10);
                HwRevisionS = CommArithmetic.DecodeMAC(SrcData, 14);
                SwRevisionS = CommArithmetic.DecodeClientID(SrcData, 18);
                CustomerS = CommArithmetic.DecodeClientID(SrcData, 20);
                DebugV = (UInt16)(SrcData[22] * 256 + SrcData[23]);
                Category = SrcData[24];
                Interval = (UInt16)(SrcData[25] * 256 + SrcData[26]);
                Calendar = CommArithmetic.DecodeDateTime(SrcData, 27);

                Pattern = SrcData[33];
                Bps = SrcData[34];
                SetTxPower(SrcData[35]);
                SampleSend = SrcData[36];
                Channel = SrcData[37];

                TemperatureInfoHigh = CommArithmetic.DecodeTemperature(SrcData, 38);
                TemperatureInfoLow = CommArithmetic.DecodeTemperature(SrcData, 40);

                //HumidityInfoHigh = CommArithmetic.DecodeHumidity(SrcData, 42);
                //HumidityInfoLow = CommArithmetic.DecodeHumidity(SrcData, 44);

                TemperatureWarnHigh = CommArithmetic.DecodeTemperature(SrcData, 42);
                TemperatureWarnLow = CommArithmetic.DecodeTemperature(SrcData, 44);

                //HumidityWarnHigh = CommArithmetic.DecodeHumidity(SrcData, 50);
                //HumidityWarnLow = CommArithmetic.DecodeHumidity(SrcData, 52);

                TemperatureCompensation = CommArithmetic.DecodeTemperature(SrcData, 46);
                //HumidityCompensation = CommArithmetic.DecodeHumidity(SrcData, 56);

                ICTemperature = SrcData[48];
                voltF = Math.Round(Convert.ToDouble((SrcData[49] * 256 + SrcData[50])) / 1000, 2);

                MaxLength = SrcData[53];

                //Temperature = CommArithmetic.DecodeTemperature(SrcData, 73);
                //Humidity = CommArithmetic.DecodeHumidity(SrcData, 75);
                RSSI = SrcData[66] - 256;

                //Falsh
                FlashID = CommArithmetic.DecodeClientID(SrcData, 51);
                FlashFront = SrcData[54] * 256 * 256 + SrcData[55] * 256 + SrcData[56];
                FlashRear = SrcData[57] * 256 * 256 + SrcData[58] * 256 + SrcData[59];
                FlashQueueLength = SrcData[60] * 256 * 256 + SrcData[61] * 256 + SrcData[62];
            }


            //兼容模式，兼容Z模式
            if (SrcData.Length == 29)
            {
                SetDeviceName(0x57);
                SetDeviceMac(SrcData, 5);
                CustomerS = CommArithmetic.DecodeClientID(SrcData, 3);
                Pattern = SrcData[2];
                ProtocolVersion = 0x00;

                SensorSN = SrcData[12] * 256 + SrcData[13];

                //传感器信息
                ICTemperature = 0; //老协议中没有IC 温度

                voltF = CommArithmetic.SHT20Voltage(SrcData[24], SrcData[25]);

                //Temperature = CommArithmetic.SHT20Temperature(SrcData[16], SrcData[17]);

                //Humidity = CommArithmetic.SHT20Humidity(SrcData[20], SrcData[21]);
                //广播模式，补充采集和传输时间
                SensorCollectTime = System.DateTime.Now;
                SensorTransforTime = System.DateTime.Now;
                //RSSI = SrcData[30] / 2 - 138; //1101 方案
                //可能收到没有RSSI的数据
                if (SrcData.Length == 28)
                {
                    RSSI = SrcData[27] - 256;
                }
                this.SourceData = CommArithmetic.ToHexString(SrcData);
            }


            //模式1 正常传输的数据，
            if (SrcData.Length == 28)
            {
                //将收到的数据填充到属性
                SetDeviceName(SrcData[3]);
                SetDeviceMac(SrcData, 7);
                CustomerS = CommArithmetic.DecodeClientID(SrcData, 5);
                Pattern = SrcData[2];
                ProtocolVersion = SrcData[4];

                SensorSN = SrcData[13] * 256 + SrcData[14];
                //传感器信息
                ICTemperature = SrcData[16]; //todo : 有符号整形数
                if (ICTemperature >= 128)
                    ICTemperature -= 256;

                voltF = Math.Round(Convert.ToDouble((SrcData[18] * 256 + SrcData[19])) / 1000, 2);
                int tempCalc = SrcData[21] * 256 + SrcData[22];
                if (tempCalc >= 0x8000)
                    tempCalc -= 65536;
                Temperature = Math.Round((Convert.ToDouble(tempCalc) / 100), 2);


                //Humidity = Math.Round(Convert.ToDouble((SrcData[24] * 256 + SrcData[25])) / 100, 2);
                //广播模式，补充采集和传输时间
                SensorCollectTime = System.DateTime.Now;
                SensorTransforTime = System.DateTime.Now;
                //RSSI = SrcData[30] / 2 - 138; //1101 方案
                //可能收到没有RSSI的数据
                if (SrcData.Length == 28)
                {
                    RSSI = SrcData[27] - 256;
                }
                this.SourceData = CommArithmetic.ToHexString(SrcData);
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
