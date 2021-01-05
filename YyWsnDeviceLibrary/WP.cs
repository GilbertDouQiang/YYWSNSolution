using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace YyWsnDeviceLibrary
{
    public class WP : Sensor
    {
        /**************************************
         * 属性
         * ************************************/

        /// <summary>
        /// 数据包的类型
        /// </summary>
        private DataPktType dataPktType { get; set; }       // 外部只读

        /// <summary>
        /// 上传间隔
        /// </summary>
        public UInt16 UploadInterval { get; set; }

        /// <summary>
        /// 正常组网间隔
        /// </summary>
        public UInt16 AdhocIntervalSuc { get; set; }
        
        /// <summary>
        /// 异常组网间隔
        /// </summary>
        public UInt16 AdhocIntervalFai { get; set; }

        /// <summary>
        /// 日志模式
        /// </summary>
        public byte LogMode { get; set; }

        /// <summary>
        /// 组网时RSSI阈值
        /// </summary>
        public Int16 AdhocRssiThr { get; set; }

        /// <summary>
        /// 传输时RSSI阈值
        /// </summary>
        public Int16 TransRssiThr { get; set; }

        /// <summary>
        /// 串口波特率
        /// </summary>
        public byte BaudRate { get; set; }

        /// <summary>
        /// HOP
        /// </summary>
        public byte Hop { get; set; }

        /// <summary>
        /// 存储错误
        /// </summary>
        public Int16 SaveError { get; set; }

        /// <summary>
        /// 文件数量
        /// </summary>
        public UInt16 FileNum { get; set; }

        /// <summary>
        /// 状态数据包的数量
        /// </summary>
        public UInt32 StatusPktNum { get; set; }


        /**************************************
         * 方法
         * ************************************/

        /// <summary>
        /// 获取设备类型编码
        /// </summary>
        /// <returns></returns>
        static public byte GetDeviceType()
        {
            return (byte)DeviceType.WP;
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

        public WP() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="SrcData"></param>
        public WP(byte[] SrcData, UInt16 IndexOfStart, DataPktType dataPktType)
        {
            if (dataPktType == DataPktType.SelfTestFromUsbToPc)
            {
                if ((byte)Device.IsPowerOnSelfTestPktFromUsbToPc(SrcData, IndexOfStart) == GetDeviceType())
                {
                    byte protocol = SrcData[IndexOfStart + 5];
                    if (protocol != 1)
                    {
                        return;
                    }

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
                    Channel = SrcData[IndexOfStart + 34];

                    Interval = (UInt16)(SrcData[IndexOfStart + 35] * 256 + SrcData[IndexOfStart + 36]);

                    UploadInterval = (UInt16)(SrcData[IndexOfStart + 37] * 256 + SrcData[IndexOfStart + 38]);

                    AdhocIntervalSuc = (UInt16)(SrcData[IndexOfStart + 39] * 256 + SrcData[IndexOfStart + 40]);

                    AdhocIntervalFai = (UInt16)(SrcData[IndexOfStart + 41] * 256 + SrcData[IndexOfStart + 42]);

                    LogMode = SrcData[IndexOfStart + 43];

                    byte rssi = SrcData[IndexOfStart + 44];
                    if (rssi >= 0x80)
                    {
                        AdhocRssiThr = (Int16)(rssi - 0x100);
                    }
                    else
                    {
                        AdhocRssiThr = (Int16)rssi;
                    }

                    rssi = SrcData[IndexOfStart + 45];
                    if (rssi >= 0x80)
                    {
                        TransRssiThr = (Int16)(rssi - 0x100);
                    }
                    else
                    {
                        TransRssiThr = (Int16)rssi;
                    }

                    BaudRate = SrcData[IndexOfStart + 46];

                    Hop = SrcData[IndexOfStart + 47];

                    ICTemperature = SrcData[IndexOfStart + 54];
                    voltF = Math.Round(Convert.ToDouble((SrcData[IndexOfStart + 55] * 256 + SrcData[IndexOfStart + 56])) / 1000, 2);

                    SaveError = (Int16)SrcData[IndexOfStart + 57];
                    if (SaveError >= 0x80)
                    {
                        SaveError = (Int16)(SaveError - 0x100);
                    }

                    FileNum = (UInt16)(SrcData[IndexOfStart + 58] * 256 + SrcData[IndexOfStart + 59]);

                    StatusPktNum = (UInt32)(SrcData[IndexOfStart + 60] * 256 * 256 + SrcData[IndexOfStart + 61] * 256 + SrcData[IndexOfStart + 62]);

                    RstSrc = SrcData[IndexOfStart + 63];

                    rssi = SrcData[IndexOfStart + 65];
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

            return;
        }

        /// <summary>
        /// 判断是不是监测工具监测到的L1发出的传感器数据包（V3版本）
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
            if (SrcLen < 36 + AppendLen)
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
            if (payLen != 3)
            {
                return -9;
            }

            // 数据类型
            if (SrcData[IndexOfStart + 27] != 0x87)
            {
                return -10;
            }

            return 0;
        }
        
        //----------------        

    }
}
