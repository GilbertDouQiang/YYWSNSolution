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
        /// <summary>
        /// 市电电压，单位：V；
        /// </summary>
        public UInt16 SupplyVoltage { get; set; }

        /// <summary>
        /// 负载功率，单位：W；
        /// </summary>
        public UInt16 LoadPower { get; set; }

        /// <summary>
        /// 负载功率补偿，单位：W；
        /// </summary>
        public Int16 PowerCompensation { get; set; }

        /// <summary>
        /// 市电电压补偿，单位：V；
        /// </summary>
        public Int16 VoltageCompensation { get; set; }

        /// <summary>
        /// 市电电压预警上限，单位：V；
        /// </summary>
        public UInt16 VoltageWarnHigh { get; set; }
        /// <summary>
        /// 市电电压预警下限，单位：V；
        /// </summary>
        public UInt16 VoltageWarnLow { get; set; }

        /// <summary>
        /// 市电电压报警上限，单位：V；
        /// </summary>
        public UInt16 VoltageAlertHigh { get; set; }

        /// <summary>
        /// 市电电压报警下限，单位：V；
        /// </summary>
        public UInt16 VoltageAlertLow { get; set; }

        /// <summary>
        /// 负载功率预警上限，单位：W；
        /// </summary>
        public UInt16 PowerWarnHigh { get; set; }

        /// <summary>
        /// 负载功率预警下限，单位：W；
        /// </summary>
        public UInt16 PowerWarnLow { get; set; }

        /// <summary>
        /// 负载功率报警上限，单位：W；
        /// </summary>
        public UInt16 PowerAlertHigh { get; set; }

        /// <summary>
        /// 负载功率报警下限，单位：W；
        /// </summary>
        public UInt16 PowerAlertLow { get; set; }


        public Socket1()
        {

        }

        public Socket1(byte[] SourceData)
        {
            //上电自检数据
            if (SourceData.Length >= 82)
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

                PowerWarnHigh = (UInt16)(SourceData[38] * 256 + SourceData[39]);
                PowerWarnLow = (UInt16)(SourceData[40] * 256 + SourceData[41]);

                VoltageWarnHigh = (UInt16)(SourceData[42] * 256 + SourceData[43]);
                VoltageWarnLow = (UInt16)(SourceData[44] * 256 + SourceData[45]);

                PowerAlertHigh = (UInt16)(SourceData[46] * 256 + SourceData[47]);
                PowerAlertLow = (UInt16)(SourceData[48] * 256 + SourceData[49]);

                VoltageAlertHigh = (UInt16)(SourceData[50] * 256 + SourceData[51]);
                VoltageAlertLow = (UInt16)(SourceData[52] * 256 + SourceData[53]);

                PowerCompensation = (Int16)(SourceData[54] * 256 + SourceData[55]);
                VoltageCompensation = (Int16)(SourceData[56] * 256 + SourceData[57]);

                ICTemperature = SourceData[58];
                voltF = Math.Round(Convert.ToDouble((SourceData[59] * 256 + SourceData[60])) / 1000, 2);

                MaxLength = SourceData[63];

                LoadPower = (UInt16)(SourceData[73] * 256 + SourceData[74]);
                SupplyVoltage = (UInt16)(SourceData[75] * 256 + SourceData[76]);

                RSSI = SourceData[78] - 256;

                //Falsh
                FlashID = CommArithmetic.DecodeClientID(SourceData, 61);
                FlashFront = (UInt32)(SourceData[64] * 256 * 256 + SourceData[65] * 256 + SourceData[66]);
                FlashRear = (UInt32)(SourceData[67] * 256 * 256 + SourceData[68] * 256 + SourceData[69]);
                FlashQueueLength = (UInt32)(SourceData[70] * 256 * 256 + SourceData[71] * 256 + SourceData[72]);
            }

            //模式1 正常传输的数据，兼容原Z版本
            if (SourceData.Length == 31)
            {
                //将收到的数据填充到属性
                Name = "Socket1";
                DeviceTypeS = SourceData[3].ToString("X2");
                DeviceMacS = CommArithmetic.DecodeMAC(SourceData, 7);
                CustomerS = CommArithmetic.DecodeClientID(SourceData, 5);
                Pattern = SourceData[2];
                ProtocolVersion = SourceData[4];

                SensorSN = SourceData[13] * 256 + SourceData[14];
                //传感器信息
                ICTemperature = SourceData[16]; //todo : 有符号整形数
                if (ICTemperature >= 128)
                    ICTemperature -= 256;

                voltF = Math.Round(Convert.ToDouble((SourceData[18] * 256 + SourceData[19])) / 1000, 2);
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
    }
}
