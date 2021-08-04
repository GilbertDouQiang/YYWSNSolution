using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YyWsnDeviceLibrary
{
    public class M30O2 : Sensor
    {
        /// <summary>
        /// 气体浓度值，单位：
        /// </summary>
        public double GASValue { get; set; }

        /// <summary>
        /// 滤波/处理后的气体浓度值，单位：
        /// </summary>
        public double GASAdjustValue { get; set; }

        /// <summary>
        /// 气体浓度值预警上限，单位：%
        /// </summary>
        public double GASValueInfoHigh { get; set; }

        /// <summary>
        /// 气体浓度值预警下限，单位：%
        /// </summary>
        public double GASValueInfoLow { get; set; }

        /// <summary>
        /// 气体浓度值报警上限，单位：%
        /// </summary>
        public double GASValueWarnHigh { get; set; }

        /// <summary>
        /// 气体浓度值报警下限，单位：%
        /// </summary>
        public double GASValueWarnLow { get; set; }

        /// <summary>
        /// 气体浓度补偿值，单位：%
        /// </summary>
        public double GASCompensation { get; set; }

        public double Temperature { get; set; }

        public double Humidity { get; set; }

        /// <summary>
        /// 温度预警上限，单位：℃
        /// </summary>
        public double TemperatureInfoHigh { get; set; }
        /// <summary>
        /// 温度预警下限，单位：℃
        /// </summary>
        public double TemperatureInfoLow { get; set; }

        /// <summary>
        /// 温度报警上限，单位：℃
        /// </summary>
        public double TemperatureWarnHigh { get; set; }

        /// <summary>
        /// 温度报警下限，单位：℃
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
        /// 温度补偿，单位：℃
        /// </summary>
        public double TemperatureCompensation { get; set; }

        /// <summary>
        /// 湿度补偿，单位：%
        /// </summary>
        public double HumidityCompensation { get; set; }


        public M30O2()
        {

        }


        public M30O2(byte[] SourceData)
        {
            //上电自检数据, 协议版本为3 2019 0704 
            if (SourceData[0] == 0xEC && SourceData[2] == 0x01 && SourceData[4] == 0x7A && SourceData.Length >= 108)
            {
                SetDeviceName(SourceData[4]);
                ProtocolVersion = SourceData[5];
                SetDevicePrimaryMac(SourceData, 6);
                SetDeviceMac(SourceData, 10);
                SetHardwareRevision(SourceData, 14);
                SetSoftwareRevision(SourceData, 18);
                SetDeviceCustomer(SourceData, 20);
                SetDeviceDebug(SourceData, 22);

                Category = SourceData[24];
                Interval = (UInt16)(SourceData[25] * 256 + SourceData[26]);
                Calendar = CommArithmetic.DecodeDateTime(SourceData, 27);

                Pattern = SourceData[33];
                Bps = SourceData[34];
                SetTxPower(SourceData[35]);
                SampleSend = SourceData[36];
                Channel = SourceData[37];

                TemperatureInfoHigh = CommArithmetic.DecodeTemperature(SourceData, 38);
                TemperatureInfoLow = CommArithmetic.DecodeTemperature(SourceData, 40);

                HumidityInfoHigh = CommArithmetic.DecodeHumidity(SourceData, 42);
                HumidityInfoLow = CommArithmetic.DecodeHumidity(SourceData, 44);

                UInt32 gasValue = (UInt32)(SourceData[46] * 256 * 256 + SourceData[47] * 256 + SourceData[48]);
                GASValueInfoHigh = (double)gasValue / 10000.0f;

                gasValue = (UInt32)(SourceData[49] * 256 * 256 + SourceData[50] * 256 + SourceData[51]);
                GASValueInfoLow = (double)gasValue / 10000.0f;

                TemperatureWarnHigh = CommArithmetic.DecodeTemperature(SourceData, 52);
                TemperatureWarnLow = CommArithmetic.DecodeTemperature(SourceData, 54);

                HumidityWarnHigh = CommArithmetic.DecodeHumidity(SourceData, 56);
                HumidityWarnLow = CommArithmetic.DecodeHumidity(SourceData, 58);

                gasValue = (UInt32)(SourceData[60] * 256 * 256 + SourceData[61] * 256 + SourceData[62]);
                GASValueWarnHigh = (double)gasValue / 10000.0f;

                gasValue = (UInt32)(SourceData[63] * 256 * 256 + SourceData[64] * 256 + SourceData[65]);
                GASValueWarnLow = (double)gasValue / 10000.0f;

                // 温度补偿
                TemperatureCompensation = CommArithmetic.DecodeTemperature(SourceData, 66);

                // 湿度补偿
                HumidityCompensation = CommArithmetic.DecodeTemperature(SourceData, 68);

                // O2补偿
                UInt32 Value = (UInt32)(SourceData[70] * 256 * 256 + SourceData[71] * 256 + SourceData[72]);
                Int32 CompValueOfAO2 = 0;
                if (Value >= 0x00800000)
                {
                    CompValueOfAO2 = (Int32)(Value - 0x01000000);
                }
                else
                {
                    CompValueOfAO2 = (Int32)Value;
                }

                GASCompensation = Math.Round((double)(CompValueOfAO2 / 10000.0f), 4);

                ICTemperature = SourceData[73];

                volt = (UInt16)(SourceData[74] * 256 + SourceData[75]);
                voltF = (double)(volt / 1000.0f);

                //Falsh
                FlashID = CommArithmetic.DecodeClientID(SourceData, 76);
                MaxLength = SourceData[78];
                FlashFront = (UInt32)(SourceData[79] * 256 * 256 + SourceData[80] * 256 + SourceData[81]);
                FlashRear = (UInt32)(SourceData[82] * 256 * 256 + SourceData[83] * 256 + SourceData[84]);
                FlashQueueLength = (UInt32)(SourceData[85] * 256 * 256 + SourceData[86] * 256 + SourceData[87]);

                Temperature = CommArithmetic.DecodeTemperature(SourceData, 88);
                Humidity = CommArithmetic.DecodeHumidity(SourceData, 90);

                gasValue = (UInt32)(SourceData[92] * 256 * 256 + SourceData[93] * 256 + SourceData[94]);
                GASValue = (double)gasValue / 10000.0f;

                NormalInterval = (UInt16)(SourceData[95] * 256 + SourceData[96]);
                WarnInterval = (UInt16)(SourceData[97] * 256 + SourceData[98]);
                AlertInterval = (UInt16)(SourceData[99] * 256 + SourceData[100]);

                RSSI = SourceData[104] - 256;
            }
        }

    }
}
