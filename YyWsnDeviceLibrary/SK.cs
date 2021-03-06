﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace YyWsnDeviceLibrary
{
    public class SK : Sensor
    {
        /**************************************
         * 属性
         * ************************************/

        /// <summary>
        /// 数据包的类型
        /// </summary>
        private DataPktType dataPktType { get; set; }       // 外部只读

        /// <summary>
        /// 负载功率，单位：W
        /// </summary>
        public UInt16 loadPower { get; set; }

        /// <summary>
        /// 插座电压，单位：V
        /// </summary>
        public UInt16 supplyVolt { get; set; }

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

        /**************************************
         * 方法
         * ************************************/

        /// <summary>
        /// 获取设备类型编码
        /// </summary>
        /// <returns></returns>
        static public byte GetDeviceType()
        {
            return (byte)DeviceType.SK;
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

        public SK() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="SrcData"></param>
        public SK(byte[] SrcData, UInt16 IndexOfStart)
        {
            if (isSensorDataV2_and_V3(SrcData, IndexOfStart, true) >= 0)
            {
                ReadSensorDataV2_and_V3(SrcData, IndexOfStart, true);
            }

            return;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="SrcData"></param>
        public SK(byte[] SrcData, UInt16 IndexOfStart, DataPktType dataPktType)
        {
            if (dataPktType == DataPktType.SelfTestFromUsbToPc)
            {
                if ((byte)Device.IsPowerOnSelfTestPktFromUsbToPc(SrcData, IndexOfStart) == GetDeviceType())
                {
                    byte protocol = SrcData[IndexOfStart + 5];

                    if (protocol == 2)
                    {
                        SetDeviceName(SrcData[IndexOfStart + 4]);
                        ProtocolVersion = protocol;
                        SetDevicePrimaryMac(SrcData, (UInt16)(IndexOfStart + 6));
                        SetDeviceMac(SrcData, (UInt16)(IndexOfStart + 10));
                        SetHardwareRevision(SrcData, (UInt16)(IndexOfStart + 14));
                        SetSoftwareRevision(SrcData, (UInt16)(IndexOfStart + 18));
                        SetDeviceCustomer(SrcData, (UInt16)(IndexOfStart + 20));
                        SetDeviceDebug(SrcData, (UInt16)(IndexOfStart + 22));

                        Category = SrcData[IndexOfStart + 24];
                        Interval = (UInt16)(SrcData[IndexOfStart+25] * 256 + SrcData[IndexOfStart+26]);
                        Calendar = CommArithmetic.DecodeDateTime(SrcData, (UInt16)(IndexOfStart + 27));

                        Pattern = SrcData[IndexOfStart + 33];
                        Bps = SrcData[IndexOfStart + 34];
                        SetTxPower(SrcData[IndexOfStart + 35]);
                        SampleSend = SrcData[IndexOfStart + 36];
                        Channel = SrcData[IndexOfStart + 37];

                        PowerWarnHigh = (UInt16)(SrcData[IndexOfStart + 38] * 256 + SrcData[IndexOfStart + 39]);
                        PowerWarnLow = (UInt16)(SrcData[IndexOfStart + 40] * 256 + SrcData[IndexOfStart + 41]);

                        VoltageWarnHigh = (UInt16)(SrcData[IndexOfStart + 42] * 256 + SrcData[IndexOfStart + 43]);
                        VoltageWarnLow = (UInt16)(SrcData[IndexOfStart + 44] * 256 + SrcData[IndexOfStart + 45]);

                        PowerAlertHigh = (UInt16)(SrcData[IndexOfStart + 46] * 256 + SrcData[IndexOfStart + 47]);
                        PowerAlertLow = (UInt16)(SrcData[IndexOfStart + 48] * 256 + SrcData[IndexOfStart + 49]);

                        VoltageAlertHigh = (UInt16)(SrcData[IndexOfStart + 50] * 256 + SrcData[IndexOfStart + 51]);
                        VoltageAlertLow = (UInt16)(SrcData[IndexOfStart + 52] * 256 + SrcData[IndexOfStart + 53]);

                        PowerCompensation = (Int16)(SrcData[IndexOfStart + 54] * 256 + SrcData[IndexOfStart + 55]);
                        VoltageCompensation = (Int16)(SrcData[IndexOfStart + 56] * 256 + SrcData[IndexOfStart + 57]);

                        ICTemperature = SrcData[IndexOfStart + 58];
                        voltF = Math.Round(Convert.ToDouble((SrcData[IndexOfStart + 59] * 256 + SrcData[IndexOfStart + 60])) / 1000, 2);

                        FlashID = CommArithmetic.DecodeClientID(SrcData, IndexOfStart + 61);
                        MaxLength = SrcData[IndexOfStart + 63];

                        FlashFront = SrcData[IndexOfStart + 64] * 256 * 256 + SrcData[IndexOfStart + 65] * 256 + SrcData[IndexOfStart + 66];
                        FlashRear = SrcData[IndexOfStart + 67] * 256 * 256 + SrcData[IndexOfStart + 68] * 256 + SrcData[IndexOfStart + 69];
                        FlashQueueLength = SrcData[IndexOfStart + 70] * 256 * 256 + SrcData[IndexOfStart + 71] * 256 + SrcData[IndexOfStart + 72];

                        loadPower = (UInt16)(SrcData[IndexOfStart + 73] * 256 + SrcData[IndexOfStart + 74]);
                        supplyVolt = (UInt16)(SrcData[IndexOfStart + 75] * 256 + SrcData[IndexOfStart + 76]);

                        byte rssi = SrcData[IndexOfStart + 78];
                        if (rssi >= 0x80)
                        {
                            RSSI = (double)(rssi - 0x100);
                        }
                        else
                        {
                            RSSI = (double)rssi;
                        }
                    }
                    else if (protocol == 3)
                    {
                        SetDeviceName(SrcData[IndexOfStart + 4]);
                        ProtocolVersion = protocol;
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

                        PowerWarnHigh = (UInt16)(SrcData[IndexOfStart + 38] * 256 + SrcData[IndexOfStart + 39]);
                        PowerWarnLow = (UInt16)(SrcData[IndexOfStart + 40] * 256 + SrcData[IndexOfStart + 41]);

                        VoltageWarnHigh = (UInt16)(SrcData[IndexOfStart + 42] * 256 + SrcData[IndexOfStart + 43]);
                        VoltageWarnLow = (UInt16)(SrcData[IndexOfStart + 44] * 256 + SrcData[IndexOfStart + 45]);

                        PowerAlertHigh = (UInt16)(SrcData[IndexOfStart + 46] * 256 + SrcData[IndexOfStart + 47]);
                        PowerAlertLow = (UInt16)(SrcData[IndexOfStart + 48] * 256 + SrcData[IndexOfStart + 49]);

                        VoltageAlertHigh = (UInt16)(SrcData[IndexOfStart + 50] * 256 + SrcData[IndexOfStart + 51]);
                        VoltageAlertLow = (UInt16)(SrcData[IndexOfStart + 52] * 256 + SrcData[IndexOfStart + 53]);

                        PowerCompensation = (Int16)(SrcData[IndexOfStart + 54] * 256 + SrcData[IndexOfStart + 55]);
                        VoltageCompensation = (Int16)(SrcData[IndexOfStart + 56] * 256 + SrcData[IndexOfStart + 57]);

                        ICTemperature = SrcData[IndexOfStart + 58];
                        voltF = Math.Round(Convert.ToDouble((SrcData[IndexOfStart + 59] * 256 + SrcData[IndexOfStart + 60])) / 1000, 2);

                        FlashID = CommArithmetic.DecodeClientID(SrcData, IndexOfStart + 61);
                        MaxLength = SrcData[IndexOfStart + 63];

                        FlashFront = SrcData[IndexOfStart + 64] * 256 * 256 + SrcData[IndexOfStart + 65] * 256 + SrcData[IndexOfStart + 66];
                        FlashRear = SrcData[IndexOfStart + 67] * 256 * 256 + SrcData[IndexOfStart + 68] * 256 + SrcData[IndexOfStart + 69];
                        FlashQueueLength = SrcData[IndexOfStart + 70] * 256 * 256 + SrcData[IndexOfStart + 71] * 256 + SrcData[IndexOfStart + 72];

                        loadPower = (UInt16)(SrcData[IndexOfStart + 73] * 256 + SrcData[IndexOfStart + 74]);
                        supplyVolt = (UInt16)(SrcData[IndexOfStart + 75] * 256 + SrcData[IndexOfStart + 76]);

                        NormalInterval = (UInt16)(SrcData[IndexOfStart + 77] * 256 + SrcData[IndexOfStart + 78]);
                        WarnInterval = (UInt16)(SrcData[IndexOfStart + 79] * 256 + SrcData[IndexOfStart + 80]);
                        AlertInterval = (UInt16)(SrcData[IndexOfStart + 81] * 256 + SrcData[IndexOfStart + 82]);

                        byte rssi = SrcData[IndexOfStart + 86];
                        if (rssi >= 0x80)
                        {
                            RSSI = (double)(rssi - 0x100);
                        }
                        else
                        {
                            RSSI = (double)rssi;
                        }
                    }
                    
                }
            }

            return;
        }

        /// <summary>
        /// 判断是不是监测工具监测到的SK发出的传感器数据包（V2和V3版本）
        /// </summary>
        /// <param name="SrcBuf"></param>
        /// <param name="ExistRssi">true = 包含RSSI； false = 不包含RSSI；</param>
        /// <returns></returns>
        static public Int16 isSensorDataV2_and_V3(byte[] SrcData, UInt16 IndexOfStart, bool ExistRssi)
        {
            UInt16 AppendLen = 0;
            if (ExistRssi == true)
            {
                AppendLen = 1;
            }

            // 数据包的总长度
            UInt16 SrcLen = (UInt16)(SrcData.Length - IndexOfStart);
            if (SrcLen < 39 + AppendLen)
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
            if (protocol != 2 && protocol != 3)
            {
                return -8;
            }

            // 负载长度
            byte payLen = SrcData[IndexOfStart + 26];
            if (payLen != 6)
            {
                return -9;
            }

            // 数据类型
            if (SrcData[IndexOfStart + 27] != 0x71 || SrcData[IndexOfStart + 30] != 0x72)
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
        public Int16 ReadSensorDataV2_and_V3(byte[] SrcData, UInt16 IndexOfStart, bool ExistRssi)
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

            // 负载功率
            loadPower = (UInt16)(SrcData[IndexOfStart + 28] * 256 + SrcData[IndexOfStart + 29]);
            
            // 插座电压
            supplyVolt = (UInt16)(SrcData[IndexOfStart + 31] * 256 + SrcData[IndexOfStart + 32]);

            // To Send Ram
            ToSendRam = SrcData[IndexOfStart + 33];

            // To Send Flash
            ToSendFlash = (UInt16)(SrcData[IndexOfStart + 34] * 256 + SrcData[IndexOfStart + 35]);

            // RSSI
            if (ExistRssi == true)
            {
                byte rssi = SrcData[IndexOfStart + 39];
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
