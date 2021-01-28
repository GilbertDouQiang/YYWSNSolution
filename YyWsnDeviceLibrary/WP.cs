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
        /// RSSI阈值/反馈的RSSI值
        /// </summary>
        public double ExpRxRssi { get; set; }

        /// <summary>
        /// 当前时间
        /// </summary>
        public DateTime Current { get; set; }

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

        /// <summary>
        /// 传输方向
        /// </summary>
        public string transDirectS { get; set; }

        /// <summary>
        /// 命令位
        /// </summary>
        public string cmdS { get; set; }

        /// <summary>
        /// 目的地址
        /// </summary>
        public UInt32 DstIdV { get; set; }

        public string DstIdS { get; set; }

        /// <summary>
        /// 源地址
        /// </summary>
        public UInt32 SrcIdV { get; set; }

        public string SrcIdS { get; set; }

        /// <summary>
        /// HOP
        /// </summary>
        public byte hop { get; set; }

        /// <summary>
        /// 上行路由的数量
        /// </summary>
        public byte up { get; set; }

        /// <summary>
        /// 下行路由的数量
        /// </summary>
        public byte down { get; set; }

        /// <summary>
        /// 保留位
        /// </summary>
        public string reserved { get; set; }


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

        public WP(byte[] SrcData, UInt16 IndexOfStart, bool ExistRssi)
        {
            if(isAdhocDataV1(SrcData,IndexOfStart, ExistRssi) >= 0)
            {
                ExplainAdhocDataV1(SrcData, IndexOfStart, ExistRssi);
                return;                
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

        /// <summary>
        /// 判断是不是自组网的数据包
        /// </summary>
        /// <param name="SrcData"></param>
        /// <param name="IndexOfStart"></param>
        /// <param name="ExistRssi"></param>
        /// <returns></returns>
        static public Int16 isAdhocDataV1(byte[] SrcData, UInt16 IndexOfStart, bool ExistRssi)
        {
            UInt16 AppendLen = 0;
            if (ExistRssi == true)
            {
                AppendLen = 1;
            }

            // 数据包的总长度
            UInt16 SrcLen = (UInt16)(SrcData.Length - IndexOfStart);
            if (SrcLen < 38 + AppendLen)
            {
                return -1;
            }

            // 起始位
            if (SrcData[IndexOfStart + 0] != 0xED && SrcData[IndexOfStart + 0] != 0xDE)
            {
                return -2;
            }

            // 长度位
            byte pktLen = SrcData[IndexOfStart + 1];
            if (pktLen + 5 + AppendLen > SrcLen)
            {
                return -3;
            }

            if ((SrcData[IndexOfStart + 0] == 0xED && SrcData[IndexOfStart + 2 + pktLen + 2] != 0xDE) || (SrcData[IndexOfStart + 0] == 0xDE && SrcData[IndexOfStart + 2 + pktLen + 2] != 0xED))
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

            return 0;
        }

        /// <summary>
        /// 解析自组网数据包
        /// </summary>
        /// <param name="SrcData"></param>
        /// <param name="IndexOfStart"></param>
        /// <param name="ExistRssi"></param>
        private void ExplainAdhocDataV1(byte[] SrcData, UInt16 IndexOfStart, bool ExistRssi)
        {
            // 传输方向
            byte StartByte = SrcData[IndexOfStart + 0];
            if (StartByte == 0xED)
            {
                transDirectS = "上行请求";
            }
            else if (StartByte == 0xDE)
            {
                transDirectS = "下行反馈";
            }
            else
            {
                transDirectS = StartByte.ToString("X2");
            }

            STP = StartByte;

            // 功能
            byte Cmd = SrcData[IndexOfStart + 2];
            Pattern = Cmd;
            if (Cmd == 0x11)
            {
                cmdS = "搜索网络";
            }
            else if (Cmd == 0x12)
            {
                cmdS = "确定网络";
            }
            else if (Cmd == 0x13)
            {
                cmdS = "维系网络";
            }
            else
            {
                transDirectS = Cmd.ToString("X2");
            }

            // 设备类型
            SetDeviceName(SrcData[IndexOfStart + 3]);

            // 协议版本
            ProtocolVersion = SrcData[IndexOfStart + 4];

            // 客户码
            SetDeviceCustomer(SrcData, (UInt16)(IndexOfStart + 5));

            // 目的地址
            DstIdV = CommArithmetic.ByteBuf_to_UInt32(SrcData, IndexOfStart + 7);
            DstIdS = CommArithmetic.ToHexString(SrcData, IndexOfStart + 7, 4);

            // 源地址
            SrcIdV = CommArithmetic.ByteBuf_to_UInt32(SrcData, IndexOfStart + 11);
            SrcIdS = CommArithmetic.ToHexString(SrcData, IndexOfStart + 11, 4);
            SetDeviceMac(SrcData, (UInt16)(IndexOfStart + 11));

            // 序列号
            Serial = CommArithmetic.ByteBuf_to_UInt16(SrcData, IndexOfStart + 15);

            // RSSI
            byte rssi = SrcData[IndexOfStart + 17];
            if (rssi >= 0x80)
            {
                ExpRxRssi = (double)(rssi - 0x100);
            }
            else
            {
                ExpRxRssi = (double)rssi;
            }

            // 当前时间
            Current = CommArithmetic.DecodeDateTime(SrcData, IndexOfStart + 18);

            // HOP
            hop = SrcData[IndexOfStart + 24];

            // UP
            hop = SrcData[IndexOfStart + 25];

            // DOWN
            hop = SrcData[IndexOfStart + 26];

            // 保留
            reserved = CommArithmetic.ToHexString(SrcData, IndexOfStart + 27, 8);

            // 系统时间
            SensorTransforTime = System.DateTime.Now;

            // RSSI
            if(ExistRssi == true)
            {
                rssi = SrcData[IndexOfStart + 38];
                if (rssi >= 0x80)
                {
                    RSSI = (double)(rssi - 0x100);
                }
                else
                {
                    RSSI = (double)rssi;
                }
            }

            // 源数据
            if (ExistRssi == true)
            {
                this.SourceData = CommArithmetic.ToHexString(SrcData, IndexOfStart, 39);
            }
            else
            {
                this.SourceData = CommArithmetic.ToHexString(SrcData, IndexOfStart, 38);
            }            
        }

        //----------------        

    }
}
