using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace YyWsnDeviceLibrary
{
    public class ESK : Sensor
    {   // 爱立信，使用SK来监控电动螺丝刀的操作

        /**************************************
         * 属性
         * ************************************/

        /// <summary>
        /// 数据包的类型
        /// </summary>
        private DataPktType dataPktType { get; set; }       // 外部只读

        /// <summary>
        /// 插座电压，单位：V
        /// </summary>
        public UInt16 supplyVolt { get; set; }

        /// <summary>
        /// 截停电流阈值，单位：mA；
        /// </summary>
        public UInt16 BlockCurrentThr { get; set; }

        /// <summary>
        /// 截停时间阈值，单位：ms；
        /// </summary>
        public UInt16 BlockDurationThr { get; set; }

        /// <summary>
        /// 配置保留位
        /// </summary>
        public UInt32 CfgReserved { get; set; }

        /// <summary>
        /// 待机电流，单位：mA；
        /// </summary>
        public UInt16 IdleCurrent { get; set; }

        /// <summary>
        /// 启动电流，单位：mA；
        /// </summary>
        public UInt16 StartCurrent { get; set; }

        /// <summary>
        /// 空打电流，单位：mA；
        /// </summary>
        public UInt16 RaceCurrent { get; set; }

        /**************************************
         * 方法
         * ************************************/

        /// <summary>
        /// 获取设备类型编码
        /// </summary>
        /// <returns></returns>
        static public byte GetDeviceType()
        {
            return (byte)DeviceType.ESK;
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

        public ESK() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="SrcData"></param>
        public ESK(byte[] SrcData, UInt16 IndexOfStart, DataPktType dataPktType)
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
                        Calendar = CommArithmetic.DecodeDateTime(SrcData, (UInt16)(IndexOfStart + 25));

                        Pattern = SrcData[IndexOfStart + 31];
                        Bps = SrcData[IndexOfStart + 32];
                        SetTxPower(SrcData[IndexOfStart + 33]);
                        SampleSend = SrcData[IndexOfStart + 34];
                        Channel = SrcData[IndexOfStart + 35];                        

                        BlockCurrentThr = (UInt16)(SrcData[IndexOfStart + 36] * 256 + SrcData[IndexOfStart + 37]);
                        BlockDurationThr = (UInt16)(SrcData[IndexOfStart + 38] * 256 + SrcData[IndexOfStart + 39]);

                        CfgReserved = (UInt32)(SrcData[IndexOfStart + 40] * 256 * 256 * 256 + SrcData[IndexOfStart + 41] * 256 * 256 + SrcData[IndexOfStart + 42] * 256 + SrcData[IndexOfStart + 43]);

                        ICTemperature = SrcData[IndexOfStart + 44];
                        voltF = Math.Round(Convert.ToDouble((SrcData[IndexOfStart + 45] * 256 + SrcData[IndexOfStart + 46])) / 1000, 2);

                        IdleCurrent = (UInt16)(SrcData[IndexOfStart + 47] * 256 + SrcData[IndexOfStart + 48]);
                        StartCurrent = (UInt16)(SrcData[IndexOfStart + 49] * 256 + SrcData[IndexOfStart + 50]);
                        RaceCurrent = (UInt16)(SrcData[IndexOfStart + 51] * 256 + SrcData[IndexOfStart + 52]);
                        supplyVolt = (UInt16)(SrcData[IndexOfStart + 53] * 256 + SrcData[IndexOfStart + 54]);

                        FlashID = CommArithmetic.DecodeClientID(SrcData, IndexOfStart + 55);
                        MaxLength = SrcData[IndexOfStart + 57];

                        FlashFront = SrcData[IndexOfStart + 58] * 256 * 256 + SrcData[IndexOfStart + 59] * 256 + SrcData[IndexOfStart + 60];
                        FlashRear = SrcData[IndexOfStart + 61] * 256 * 256 + SrcData[IndexOfStart + 62] * 256 + SrcData[IndexOfStart + 63];
                        FlashQueueLength = SrcData[IndexOfStart + 64] * 256 * 256 + SrcData[IndexOfStart + 65] * 256 + SrcData[IndexOfStart + 66];

                        byte rssi = SrcData[IndexOfStart + 68];
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
    }
}
