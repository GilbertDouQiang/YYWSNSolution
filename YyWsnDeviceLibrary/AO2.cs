using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace YyWsnDeviceLibrary
{
    public class AO2 : Sensor
    {
        /**************************************
         * 属性
         * ************************************/

        /// <summary>
        /// 数据包的类型
        /// </summary>
        private DataPktType dataPktType { get; set; }       // 外部只读

        /// <summary>
        /// 网关的设备类型
        /// </summary>
        public byte deviceTypeOfGw { get; set; }

        /// <summary>
        /// 网关的传输序列号
        /// </summary>
        public UInt16 SerialOfGw { get; set; }

        /// <summary>
        /// 网关的电压，单位：0.001V
        /// </summary>
        public UInt16 GwVolt { get; set; }

        /// <summary>
        /// 网关的电压，单位：V
        /// </summary>
        public double GwVoltF { get; set; }

        /// <summary>
        /// 保留位
        /// </summary>
        public byte Reserved { get; set; }

        /// <summary>
        /// 温度，单位：0.01℃
        /// </summary>
        public Int16 temp { get; set; }

        /// <summary>
        /// 温度，单位：℃
        /// </summary>
        public double tempF { get; set; }

        /// <summary>
        /// 湿度，单位：0.01%
        /// </summary>
        public UInt16 hum { get; set; }

        /// <summary>
        /// 湿度，单位：%
        /// </summary>
        public double humF { get; set; }

        /// <summary>
        /// 滤波后的O2浓度，单位：ppm
        /// </summary>
        public Int32 O2 { get; set; }

        /// <summary>
        /// 滤波后的O2浓度，保留四位小数，单位：%
        /// </summary>
        public double O2F { get; set; }

        /// <summary>
        /// 滤波前的O2浓度，单位：ppm
        /// </summary>
        public Int32 uO2 { get; set; }

        /// <summary>
        /// 滤波前的O2浓度，保留四位小数，单位：%
        /// </summary>
        public double uO2F { get; set; }

        /// <summary>
        /// 温度预警上限，单位：℃
        /// </summary>
        public double TempWarnHigh { get; set; }

        /// <summary>
        /// 温度预警下限，单位：℃
        /// </summary>
        public double TempWarnLow { get; set; }

        /// <summary>
        /// 温度报警上限，单位：℃
        /// </summary>
        public double TempAlertHigh { get; set; }

        /// <summary>
        /// 温度报警下限，单位：℃
        /// </summary>
        public double TempAlertLow { get; set; }

        /// <summary>
        /// 湿度预警上限，单位：%
        /// </summary>
        public double HumWarnHigh { get; set; }
        /// <summary>
        /// 湿度预警下限，单位：%
        /// </summary>
        public double HumWarnLow { get; set; }

        /// <summary>
        /// 湿度报警上限，单位：%
        /// </summary>
        public double HumAlertHigh { get; set; }

        /// <summary>
        /// 湿度报警下限，单位：%
        /// </summary>
        public double HumAlertLow { get; set; }

        /// <summary>
        /// O2预警上限，保留四位小数，单位：%
        /// </summary>
        public double O2WarnHigh { get; set; }
        /// <summary>
        /// O2预警下限，保留四位小数，单位：%
        /// </summary>
        public double O2WarnLow { get; set; }

        /// <summary>
        /// O2报警上限，保留四位小数，单位：%
        /// </summary>
        public double O2AlertHigh { get; set; }

        /// <summary>
        /// O2报警下限，保留四位小数，单位：%
        /// </summary>
        public double O2AlertLow { get; set; }

        /// <summary>
        /// 温度补偿
        /// </summary>
        public double TempCompensation { get; set; }

        /// <summary>
        /// 湿度补偿
        /// </summary>
        public double HumCompensation { get; set; }

        /// <summary>
        /// O2补偿，保留四位小数，单位：%
        /// </summary>
        public double CompValueOfAO2 { get; set; }

        /**************************************
         * 方法
         * ************************************/

        /// <summary>
        /// 获取设备类型编码
        /// </summary>
        /// <returns></returns>
        static public byte GetDeviceType()
        {
            return (byte)DeviceType.AO2;
        }

        /// <summary>
        /// 判断输入的设备类型是否与本设备类型一致
        /// </summary>
        /// <param name="iDeviceType"></param>
        /// <returns></returns>
        static public bool isDeviceType(byte iDeviceType)
        {
            if (iDeviceType == GetDeviceType())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 获取数据包的类型
        /// </summary>
        /// <returns></returns>
        public DataPktType GetDataPktType()
        {
            return dataPktType;
        }

        public AO2() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="SrcData"></param>
        public AO2(byte[] SrcData, UInt16 IndexOfStart)
        {
            if (isSensorDataV3(SrcData, IndexOfStart, true) >= 0)
            {
                ReadSensorDataV3(SrcData, IndexOfStart, true);
            }

            return;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="SrcData"></param>
        public AO2(byte[] SrcData, UInt16 IndexOfStart, Device.DataPktType pktType, Device.DeviceType deviceType)
        {
            if (isDeviceType((byte)deviceType) == false)
            {
                return;
            }

            UInt16 iCnt = 0;

            if (pktType == Device.DataPktType.SensorFromSsToGw)
            {
                if (isSensorDataV3(SrcData, IndexOfStart, true) >= 0)
                {
                    ReadSensorDataV3(SrcData, IndexOfStart, true);
                }
            }
            else if (pktType == Device.DataPktType.SensorDataFromGmToPc)
            {
                iCnt = (UInt16)(IndexOfStart + 9);

                // 数据包类型
                dataPktType = DataPktType.SensorFromSsToGw;

                // 网关的设备类型
                deviceTypeOfGw = SrcData[iCnt];
                iCnt += 1;

                // 网关的传输序列号
                SerialOfGw = (UInt16)(SrcData[iCnt] * 256 + SrcData[iCnt + 1]);
                iCnt += 2;

                // 网关的传输时间
                GWTime = CommArithmetic.DecodeDateTime(SrcData, iCnt);
                iCnt += 6;

                // 网关的电压
                GwVolt = (UInt16)(SrcData[iCnt] * 256 + SrcData[iCnt + 1]);
                GwVoltF = (double)(GwVolt / 1000.0f);
                iCnt += 2;

                // Sensor ID
                SetDeviceMac(SrcData, iCnt);
                iCnt += 4;

                //状态state
                State = SrcData[iCnt];
                iCnt += 1;

                //报警项
                AlarmItem = SrcData[iCnt];
                iCnt += 1;

                // Cmd
                Pattern = SrcData[iCnt];
                iCnt += 1;

                // Device Type
                SetDeviceName(SrcData[iCnt]);
                iCnt += 1;

                // protocol
                ProtocolVersion = SrcData[iCnt];
                iCnt += 1;

                iCnt += 2;

                // Serial
                SensorSN = SrcData[iCnt] * 256 + SrcData[iCnt + 1];
                iCnt += 2;

                iCnt += 1;

                // IC temp
                ICTemperature = SrcData[iCnt];
                if (ICTemperature >= 128)
                {
                    ICTemperature -= 256;
                }
                iCnt += 1;

                iCnt += 1;

                // voltage
                volt = (UInt16)(SrcData[iCnt] * 256 + SrcData[iCnt + 1]);
                voltF = (double)(volt / 1000.0f);
                iCnt += 2;

                iCnt += 1;

                // Sample Calendar
                SensorCollectTime = CommArithmetic.DecodeDateTime(SrcData, iCnt);
                iCnt += 6;

                iCnt += 1;

                // Transfer Calendar
                SensorTransforTime = CommArithmetic.DecodeDateTime(SrcData, iCnt);
                iCnt += 6;

                iCnt += 1;

                // RSSI
                byte rssi = SrcData[iCnt];
                if (rssi >= 0x80)
                {
                    RSSI = (double)(rssi - 0x100);
                }
                else
                {
                    RSSI = (double)rssi;
                }
                iCnt += 1;

                iCnt += 1;

                // 温度
                temp = (Int16)(SrcData[iCnt] * 256 + SrcData[iCnt + 1]);
                tempF = (double)(temp / 100.0f);
                iCnt += 2;

                iCnt += 1;

                // 湿度
                hum = (UInt16)(SrcData[iCnt] * 256 + SrcData[iCnt + 1]);
                humF = (double)(hum / 100.0f);
                iCnt += 2;

                iCnt += 1;

                // 滤波后的O2浓度
                Int32 Value = (Int32)(SrcData[iCnt] * 256 * 256 + SrcData[iCnt + 1] * 256 + SrcData[iCnt + 2]);
                if (Value >= 0x00800000)
                {
                    O2 = (Int32)(Value - 0x01000000);
                }
                else
                {
                    O2 = (Int32)Value;
                }
                O2F = Math.Round((double)(O2 / 10000.0f), 4);
                iCnt += 3;

                iCnt += 1;

                // 滤波前的O2浓度
                Value = (Int32)(SrcData[iCnt] * 256 * 256 + SrcData[iCnt + 1] * 256 + SrcData[iCnt + 2]);
                if (Value >= 0x00800000)
                {
                    uO2 = (Int32)(Value - 0x01000000);
                }
                else
                {
                    uO2 = (Int32)Value;
                }
                uO2F = Math.Round((double)(uO2 / 10000.0f), 4);
                iCnt += 3;

                // Last/History
                LastHistory = SrcData[iCnt];
                iCnt += 1;

                // To Send Ram
                ToSendRam = SrcData[iCnt];
                iCnt += 1;

                // To Send Flash
                ToSendFlash = (UInt16)(SrcData[iCnt] * 256 + SrcData[iCnt + 1]);
                iCnt += 2;

                // 保留
                Reserved = SrcData[iCnt];
                iCnt += 1;

                // 系统时间            
                SystemTime = System.DateTime.Now;

                // 源数据
                this.SourceData = CommArithmetic.ToHexString(SrcData);
            }
            else if (pktType == Device.DataPktType.SelfTestFromUsbToPc)
            {
                if (SrcData[IndexOfStart + 5] != 0x03)
                {
                    return;
                }

                SetDeviceName(SrcData[IndexOfStart + 4]);
                ProtocolVersion = SrcData[IndexOfStart + 5];
                SetDevicePrimaryMac(SrcData, (UInt16)(IndexOfStart + 6));
                SetDeviceMac(SrcData, (UInt16)(IndexOfStart + 10));
                SetHardwareRevision(SrcData, (UInt16)(IndexOfStart + 14));
                SetSoftwareRevision(SrcData, (UInt16)(IndexOfStart + 18));

                SetDeviceCustomer(SrcData, (UInt16)(IndexOfStart + 20));
                SetDeviceDebug(SrcData, (UInt16)(IndexOfStart + 22));

                Category = SrcData[IndexOfStart + 24];
                Interval = (UInt16)(SrcData[IndexOfStart + 25] * 256 + SrcData[IndexOfStart + 26]);
                Calendar = CommArithmetic.DecodeDateTime(SrcData, (UInt16)(IndexOfStart + 27));

                Pattern = SrcData[IndexOfStart + 33];
                Bps = SrcData[IndexOfStart + 34];
                SetTxPower(SrcData[IndexOfStart + 35]);
                SampleSend = SrcData[IndexOfStart + 36];
                Channel = SrcData[IndexOfStart + 37];

                TempWarnHigh = (double)(Int16)(SrcData[IndexOfStart + 38] * 256 + SrcData[IndexOfStart + 39]) / 100.0f;
                TempWarnLow = (double)(Int16)(SrcData[IndexOfStart + 40] * 256 + SrcData[IndexOfStart + 41]) / 100.0f;

                HumWarnHigh = (double)(SrcData[IndexOfStart + 42] * 256 + SrcData[IndexOfStart + 43]) / 100.0f;
                HumWarnLow = (double)(SrcData[IndexOfStart + 44] * 256 + SrcData[IndexOfStart + 45]) / 100.0f;

                UInt32 gasValue = (UInt32)(SrcData[IndexOfStart + 46] * 256 * 256 + SrcData[IndexOfStart + 47] * 256 + SrcData[IndexOfStart + 48]);
                O2WarnHigh = Math.Round((double)(gasValue / 10000.0f), 4);

                gasValue = (UInt32)(SrcData[IndexOfStart + 49] * 256 * 256 + SrcData[IndexOfStart + 50] * 256 + SrcData[IndexOfStart + 51]);
                O2WarnLow = Math.Round((double)(gasValue / 10000.0f), 4);

                TempAlertHigh = (double)(Int16)(SrcData[IndexOfStart + 52] * 256 + SrcData[IndexOfStart + 53]) / 100.0f;
                TempAlertLow = (double)(Int16)(SrcData[IndexOfStart + 54] * 256 + SrcData[IndexOfStart + 55]) / 100.0f;

                HumAlertHigh = (double)(SrcData[IndexOfStart + 56] * 256 + SrcData[IndexOfStart + 57]) / 100.0f;
                HumAlertLow = (double)(SrcData[IndexOfStart + 58] * 256 + SrcData[IndexOfStart + 59]) / 100.0f;

                gasValue = (UInt32)(SrcData[IndexOfStart + 60] * 256 * 256 + SrcData[IndexOfStart + 61] * 256 + SrcData[IndexOfStart + 62]);
                O2AlertHigh = Math.Round((double)(gasValue / 10000.0f), 4);

                gasValue = (UInt32)(SrcData[IndexOfStart + 63] * 256 * 256 + SrcData[IndexOfStart + 64] * 256 + SrcData[IndexOfStart + 65]);
                O2AlertLow = Math.Round((double)(gasValue / 10000.0f), 4);

                TempCompensation = (double)(Int16)(SrcData[IndexOfStart + 66] * 256 + SrcData[IndexOfStart + 67]) / 100.0f;
                HumCompensation = (double)(Int16)(SrcData[IndexOfStart + 68] * 256 + SrcData[IndexOfStart + 69]) / 100.0f;

                UInt32 Value = (UInt32)(SrcData[IndexOfStart + 70] * 256 * 256 + SrcData[IndexOfStart + 71] * 256 + SrcData[IndexOfStart + 72]);
                Int32 iValue = 0;
                if (Value >= 0x00800000)
                {
                    iValue = (Int32)(Value - 0x01000000);
                }
                else
                {
                    iValue = (Int32)Value;
                }
                CompValueOfAO2 = Math.Round((double)(iValue / 10000.0f), 4);

                ICTemperature = SrcData[IndexOfStart + 73];

                volt = (UInt16)(SrcData[IndexOfStart + 74] * 256 + SrcData[IndexOfStart + 75]);
                voltF = (double)(volt / 1000.0f);

                //Falsh
                FlashID = CommArithmetic.DecodeClientID(SrcData, IndexOfStart + 76);
                MaxLength = SrcData[IndexOfStart + 78];

                FlashFront = (UInt32)(SrcData[IndexOfStart + 79] * 256 * 256 + SrcData[IndexOfStart + 80] * 256 + SrcData[IndexOfStart + 81]);
                FlashRear = (UInt32)(SrcData[IndexOfStart + 82] * 256 * 256 + SrcData[IndexOfStart + 83] * 256 + SrcData[IndexOfStart + 84]);
                FlashQueueLength = (UInt32)(SrcData[IndexOfStart + 85] * 256 * 256 + SrcData[IndexOfStart + 86] * 256 + SrcData[IndexOfStart + 87]);

                temp = (Int16)(SrcData[IndexOfStart + 88] * 256 + SrcData[IndexOfStart + 89]);
                tempF = (double)(temp / 100.0f);

                hum = (UInt16)(SrcData[IndexOfStart + 90] * 256 + SrcData[IndexOfStart + 91]);
                humF = (double)(hum / 100.0f);

                Value = (UInt32)(SrcData[92] * 256 * 256 + SrcData[93] * 256 + SrcData[94]);
                if (Value >= 0x00800000)
                {
                    O2 = (Int32)(Value - 0x01000000);
                }
                else
                {
                    O2 = (Int32)Value;
                }
                O2F = Math.Round((double)(O2 / 10000.0f), 4);

                uO2 = O2;
                uO2F = O2F;

                NormalInterval = (UInt16)(SrcData[IndexOfStart + 95] * 256 + SrcData[IndexOfStart + 96]);
                WarnInterval = (UInt16)(SrcData[IndexOfStart + 97] * 256 + SrcData[IndexOfStart + 98]);
                AlertInterval = (UInt16)(SrcData[IndexOfStart + 99] * 256 + SrcData[IndexOfStart + 100]);

                byte rssi = SrcData[IndexOfStart + 104];
                if (rssi >= 0x80)
                {
                    RSSI = (double)(rssi - 0x100);
                }
                else
                {
                    RSSI = (double)rssi;
                }

                // 源数据
                this.SourceData = CommArithmetic.ToHexString(SrcData);
            }
            else
            {

            }
        }

        /// <summary>
        /// 判断是不是监测工具监测到的AO2发出的传感器数据包（V3版本）
        /// </summary>
        /// <param name="SrcBuf"></param>
        /// <param name="ExistRssi">true = 包含RSSI； false = 不包含RSSI；</param>
        /// <returns></returns>
        static public Int16 isSensorDataV3(byte[] SrcData, UInt16 IndexOfStart, bool ExistRssi)
        {
            UInt16 AppendLen = 0;
            if (ExistRssi == true)
            {
                AppendLen = 1;
            }

            // 数据包的总长度
            UInt16 SrcLen = (UInt16)(SrcData.Length - IndexOfStart);
            if (SrcLen < 47 + AppendLen)
            {
                return -1;
            }

            // 起始位
            if (SrcData[IndexOfStart + 0] != 0xEA)
            {
                return -2;
            }

            // 长度位
            byte pktLen = SrcData[IndexOfStart + 1];
            if (pktLen + 5 + AppendLen > SrcLen)
            {
                return -3;
            }

            if (SrcData[IndexOfStart + 2 + pktLen + 2] != 0xAE)
            {
                return -4;
            }

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, SrcData, (UInt16)(IndexOfStart + 2), pktLen);
            UInt16 crc_chk = (UInt16)(SrcData[IndexOfStart + 2 + pktLen + 0] * 256 + SrcData[IndexOfStart + 2 + pktLen + 1]);
            if (crc_chk != crc && crc_chk != 0)
            {
                return -5;
            }

            // Cmd
            byte Cmd = SrcData[IndexOfStart + 2];
            if (Cmd != 1 && Cmd != 2 && Cmd != 3 && Cmd != 4)
            {
                return -6;
            }

            // DeviceType
            byte DeviceType = SrcData[IndexOfStart + 3];
            if (isDeviceType(DeviceType) == false)
            {
                return -7;
            }

            // 协议版本
            byte protocol = SrcData[IndexOfStart + 4];
            if (protocol != 3)
            {
                return -8;
            }

            // 负载长度
            byte payLen = SrcData[IndexOfStart + 26];
            if (payLen != 14)
            {
                return -9;
            }

            // 数据类型
            if (SrcData[IndexOfStart + 27] != 0x65 || SrcData[IndexOfStart + 30] != 0x66 || SrcData[IndexOfStart + 33] != 0x7D || SrcData[IndexOfStart + 37] != 0x7E)
            {
                return -10;
            }

            return 0;
        }

        /// <summary>
        /// 读取Sensor数据
        /// </summary>
        /// <param name="SrcData"></param>
        /// <param name="IndexOfStart"></param>
        /// <param name="ExistRssi"></param>
        /// <returns></returns>
        public Int16 ReadSensorDataV3(byte[] SrcData, UInt16 IndexOfStart, bool ExistRssi)
        {
            // 数据包类型
            dataPktType = DataPktType.SensorFromSsToGw;

            // 起始位
            STP = SrcData[IndexOfStart + 0];

            // Cmd
            Pattern = SrcData[IndexOfStart + 2];

            // Device Type
            SetDeviceName(SrcData[IndexOfStart + 3]);

            // protocol
            ProtocolVersion = SrcData[IndexOfStart + 4];

            // CustomerV
            SetDeviceCustomer(SrcData, (UInt16)(IndexOfStart + 5));

            // Sensor ID
            SetDeviceMac(SrcData, (UInt16)(IndexOfStart + 7));

            // Last/History
            LastHistory = CommArithmetic.DecodeLastHistory(SrcData, (UInt16)(IndexOfStart + 11));

            //状态state
            State = SrcData[IndexOfStart + 12];

            //报警项
            AlarmItem = SrcData[IndexOfStart + 13];

            // Serial
            SensorSN = SrcData[IndexOfStart + 14] * 256 + SrcData[IndexOfStart + 15];

            // Sample Calendar
            SensorCollectTime = CommArithmetic.DecodeDateTime(SrcData, (UInt16)(IndexOfStart + 16));

            // IC temp
            ICTemperature = SrcData[IndexOfStart + 22];
            if (ICTemperature >= 128)
            {
                ICTemperature -= 256;
            }

            // voltage
            volt = (UInt16)(SrcData[IndexOfStart + 23] * 256 + SrcData[IndexOfStart + 24]);
            voltF = (double)(volt / 1000.0f);

            // AltSerial
            AltSerial = SrcData[IndexOfStart + 25];

            // AO2
            temp = (Int16)(SrcData[IndexOfStart + 28] * 256 + SrcData[IndexOfStart + 29]);
            tempF = (double)(temp / 100.0f);

            hum = (UInt16)(SrcData[IndexOfStart + 31] * 256 + SrcData[IndexOfStart + 32]);
            humF = (double)(hum / 100.0f);

            UInt32 Value = (UInt32)(SrcData[IndexOfStart + 34] * 256 * 256 + SrcData[IndexOfStart + 35] * 256 + SrcData[IndexOfStart + 36]);
            if(Value >= 0x00800000)
            {
                O2 = (Int32)(Value - 0x01000000);
            }
            else
            {
                O2 = (Int32)Value;
            }

            Value = (UInt32)(SrcData[IndexOfStart + 38] * 256 * 256 + SrcData[IndexOfStart + 39] * 256 + SrcData[IndexOfStart + 40]);
            if (Value >= 0x00800000)
            {
                uO2 = (Int32)(Value - 0x01000000);
            }
            else
            {
                uO2 = (Int32)Value;
            }

            // To Send Ram
            ToSendRam = SrcData[IndexOfStart + 41];

            // To Send Flash
            ToSendFlash = (UInt16)(SrcData[IndexOfStart + 42] * 256 + SrcData[IndexOfStart + 43]);

            // RSSI
            if (ExistRssi == true)
            {
                byte rssi = SrcData[IndexOfStart + 47];
                if (rssi >= 0x80)
                {
                    RSSI = (double)(rssi - 0x100);
                }
                else
                {
                    RSSI = (double)rssi;
                }
            }

            // 传输时间                
            SensorTransforTime = System.DateTime.Now;

            this.SourceData = CommArithmetic.ToHexString(SrcData);

            return 0;
        }

    }
}
