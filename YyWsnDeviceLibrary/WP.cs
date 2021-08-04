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
        /// 节点类型
        /// </summary>
        public byte nodeTypeV { get; set; }

        /// <summary>
        /// 节点类型
        /// </summary>
        public string nodeTypeS { get; set; }

        /// <summary>
        /// 透传时的序列号
        /// </summary>
        public byte aSerial { get; set; }

        /// <summary>
        /// 数据包类型
        /// </summary>
        public byte pktTypeV { get; set; }

        /// <summary>
        /// 数据包类型
        /// </summary>
        public string pktTypeS { get; set; }

        /// <summary>
        /// 负载内容中的协议版本
        /// </summary>
        public byte inProtocol { get; set; }

        /// <summary>
        /// 第一RT/GW ID
        /// </summary>
        public UInt32 FirstRtGwIdV { get; set; }

        /// <summary>
        /// 第一RT/GW ID
        /// </summary>
        public string FirstRtGwIdS { get; set; }

        /// <summary>
        /// 第一RT/GW 接收时间
        /// </summary>
        public DateTime FirstRtGwRdTime { get; set; }

        /// <summary>
        /// 第一RT/GW RSSI
        /// </summary>
        public double FirstRtGwRssi { get; set; }

        /// <summary>
        /// 负载内容：节点类型
        /// </summary>
        public byte inNodeTypeV { get; set; }

        /// <summary>
        /// 负载内容：节点类型
        /// </summary>
        public string inNodeTypeS { get; set; }

        /// <summary>
        /// WP/RT ID
        /// </summary>
        public UInt32 WpRtIdV { get; set; }

        /// <summary>
        /// WP/RT ID
        /// </summary>
        public string WpRtIdS { get; set; }

        /// <summary>
        /// 负载内容
        /// </summary>
        public string PayloadTxt { get; set; }

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
            if(isAdhocV1(SrcData,IndexOfStart, ExistRssi) >= 0)
            {
                ExplainAdhocV1(SrcData, IndexOfStart, ExistRssi);
                return;                
            }

            if (isAdhocDataUpV1(SrcData, IndexOfStart, ExistRssi) >= 0)
            {
                ExplainAdhocDataUpV1(SrcData, IndexOfStart, ExistRssi);
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
        /// 判断是不是自组网过程的数据包
        /// </summary>
        /// <param name="SrcData"></param>
        /// <param name="IndexOfStart"></param>
        /// <param name="ExistRssi"></param>
        /// <returns></returns>
        static public Int16 isAdhocV1(byte[] SrcData, UInt16 IndexOfStart, bool ExistRssi)
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
        /// 判断是不是通过自组网传输的数据包
        /// </summary>
        /// <param name="SrcData"></param>
        /// <param name="IndexOfStart"></param>
        /// <param name="ExistRssi"></param>
        /// <returns></returns>
        static public Int16 isAdhocDataUpV1(byte[] SrcData, UInt16 IndexOfStart, bool ExistRssi)
        {
            UInt16 AppendLen = 0;
            if (ExistRssi == true)
            {
                AppendLen = 1;
            }

            // 数据包的总长度
            UInt16 SrcLen = (UInt16)(SrcData.Length - IndexOfStart);
            if (SrcLen < 19 + AppendLen)
            {
                return -1;
            }

            // 起始位
            if (SrcData[IndexOfStart + 0] != 0xB4)
            {
                return -2;
            }

            // 长度位
            byte pktLen = SrcData[IndexOfStart + 1];
            if (pktLen + 4 + AppendLen > SrcLen)
            {
                return -3;
            }

            if (SrcData[IndexOfStart + 0] == 0xB4 && SrcData[IndexOfStart + 2 + pktLen + 1] != 0x4B)
            {
                return -4;
            }

            // 命令位
            if (SrcData[IndexOfStart + 2] != 0x30)
            {
                return -5;
            }

            // CRC16
            byte crc = MyCustomFxn.CRC8(MyCustomFxn.GetItuPolynomialOfCrc8(), 0, SrcData, (UInt16)(IndexOfStart + 2), pktLen);
            byte crc_chk = SrcData[IndexOfStart + 2 + pktLen + 0];
            if (crc_chk != crc && crc_chk != 0)
            {
                return -6;
            }

            return 0;
        }


        /// <summary>
        /// 解析自组网过程数据包
        /// </summary>
        /// <param name="SrcData"></param>
        /// <param name="IndexOfStart"></param>
        /// <param name="ExistRssi"></param>
        private void ExplainAdhocV1(byte[] SrcData, UInt16 IndexOfStart, bool ExistRssi)
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
            up = SrcData[IndexOfStart + 25];

            // DOWN
            down = SrcData[IndexOfStart + 26];

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

        private string ExplainPayload(byte[] Paybuf, UInt16 IndexOfStart, UInt16 PayLen)
        {
            string text = "";

            int error = 0;

            for (int iCnt = 0; iCnt < PayLen;)
            {
                byte dataType = Paybuf[IndexOfStart + iCnt];

                error = 0;

                switch (dataType)
                {
                    case 0x65:      // 温度
                        {
                            if (PayLen - iCnt < 3)
                            {
                                error = -2;
                                break;
                            }

                            text += ", 温度：";
                            double temp = (double)(Int16)CommArithmetic.ByteBuf_to_UInt16(Paybuf, IndexOfStart + iCnt + 1) / 100.0f;
                            text += temp.ToString("F2");

                            iCnt += 3;
                            break;
                        }
                    case 0x66:      // 湿度
                        {
                            if (PayLen - iCnt < 3)
                            {
                                error = -2;
                                break;
                            }

                            text += ", 湿度：";
                            double hum = (double)CommArithmetic.ByteBuf_to_UInt16(Paybuf, IndexOfStart + iCnt + 1) / 100.0f;
                            text += hum.ToString("F2");

                            iCnt += 3;
                            break;
                        }
                    case 0x7A:      // 三轴
                        {
                            if (PayLen - iCnt < 9)
                            {
                                error = -2;
                                break;
                            }

                            text += ", 三轴：";
                            text += ((Int16)CommArithmetic.ByteBuf_to_UInt16(Paybuf, IndexOfStart + iCnt + 1)).ToString() + ", ";
                            text += ((Int16)CommArithmetic.ByteBuf_to_UInt16(Paybuf, IndexOfStart + iCnt + 3)).ToString() + ", ";
                            text += ((Int16)CommArithmetic.ByteBuf_to_UInt16(Paybuf, IndexOfStart + iCnt + 5)).ToString() + ", ";
                            text += CommArithmetic.ByteBuf_to_UInt16(Paybuf, IndexOfStart + iCnt + 7).ToString();

                            iCnt += 9;
                            break;
                        }
                    case 0x83:      // 气压
                        {
                            if (PayLen - iCnt < 4)
                            {
                                error = -2;
                                break;
                            }

                            text += ", 气压：";
                            Int32 pressure = Paybuf[IndexOfStart + iCnt + 1] * 256 * 256 + Paybuf[IndexOfStart + iCnt + 2] * 256 + Paybuf[IndexOfStart + iCnt + 3];
                            if(pressure >= 0x800000)
                            {
                                pressure -= 0x1000000;
                            }
                            text += ((double)pressure/10.0f).ToString("F1");

                            iCnt += 4;
                            break;
                        }
                    case 0x87:      // 光照强度
                        {
                            if (PayLen - iCnt < 3)
                            {
                                error = -2;
                                break;
                            }

                            text += ", 光照强度：";
                            text += ((Int16)CommArithmetic.ByteBuf_to_UInt16(Paybuf, IndexOfStart + iCnt + 1)).ToString();

                            iCnt += 3;
                            break;
                        }
                    case 0x88:      // 经纬度
                        {
                            if (PayLen - iCnt < 9)
                            {
                                error = -2;
                                break;
                            }

                            text += ", 经纬度：";
                            text += ((double)(Int32)CommArithmetic.ByteBuf_to_UInt32(Paybuf, IndexOfStart + iCnt + 1) / 1000000.0f).ToString("F6") + ", ";
                            text += ((double)(Int32)CommArithmetic.ByteBuf_to_UInt32(Paybuf, IndexOfStart + iCnt + 5) / 1000000.0f).ToString("F6");

                            iCnt += 9;
                            break;
                        }
                    default:
                        {
                            text += "不识别数据类型";
                            error = -2;
                            break;
                        }
                }

                if (error < 0)
                {
                    text += "数据长度错误";
                    break;
                }
            } // for

            return text;
        }

        /// <summary>
        /// 解析通过自组网的网络传输的数据包
        /// </summary>
        /// <param name="SrcData"></param>
        /// <param name="IndexOfStart"></param>
        /// <param name="ExistRssi"></param>
        private void ExplainAdhocDataUpV1(byte[] SrcData, UInt16 IndexOfStart, bool ExistRssi)
        {
            // 传输方向
            byte StartByte = SrcData[IndexOfStart + 0];
            if (StartByte == 0xB4)
            {
                transDirectS = "上行传输";
            }
            else if (StartByte == 0x4B)
            {
                transDirectS = "下行反馈";
            }
            else
            {
                transDirectS = StartByte.ToString("X2");
            }

            // 数据包长度
            byte pktLen = SrcData[IndexOfStart + 1];

            STP = StartByte;

            // 功能
            byte Cmd = SrcData[IndexOfStart + 2];
            Pattern = Cmd;
            if (Cmd == 0x30)
            {
                cmdS = "透传数据";
            }
            else
            {
                transDirectS = Cmd.ToString("X2");
            }

            // 协议版本
            ProtocolVersion = SrcData[IndexOfStart + 3];

            // 客户码
            SetDeviceCustomer(SrcData, (UInt16)(IndexOfStart + 4));

            // 目的地址
            DstIdV = CommArithmetic.ByteBuf_to_UInt32(SrcData, IndexOfStart + 6);
            DstIdS = CommArithmetic.ToHexString(SrcData, IndexOfStart + 6, 4);

            // 源地址
            SrcIdV = CommArithmetic.ByteBuf_to_UInt32(SrcData, IndexOfStart + 10);
            SrcIdS = CommArithmetic.ToHexString(SrcData, IndexOfStart + 10, 4);
            SetDeviceMac(SrcData, (UInt16)(IndexOfStart + 10));

            // HOP
            hop = SrcData[IndexOfStart + 14];

            // 节点类型
            nodeTypeV = SrcData[IndexOfStart + 15];
            if (nodeTypeV == 0x00)
            {
                nodeTypeS = "相机";
            }
            else if (nodeTypeV == 0x01)
            {
                nodeTypeS = "中继";
            }
            else if (nodeTypeV == 0x02)
            {
                nodeTypeS = "传感器";
            }
            else
            {
                nodeTypeS = nodeTypeV.ToString("X2");
            }

            // 序列号
            aSerial = SrcData[IndexOfStart + 16];

            // 附加RSSI
            byte rssi = 0;
            if (ExistRssi == true)
            {
                rssi = SrcData[IndexOfStart + 2 + pktLen + 2];
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
                this.SourceData = CommArithmetic.ToHexString(SrcData, IndexOfStart, pktLen + 4 + 1);
            }
            else
            {
                this.SourceData = CommArithmetic.ToHexString(SrcData, IndexOfStart, pktLen + 4);
            }

            // 透传内容
            {
                IndexOfStart += 17;

                // 数据包类型
                pktTypeV = SrcData[IndexOfStart + 0];
                if (pktTypeV == 0x00)
                {
                    pktTypeS = "传感器数据";
                }
                else if (pktTypeV == 0x01)
                {
                    pktTypeS = "状态路由";
                }
                else if (pktTypeV == 0x02)
                {
                    pktTypeS = "RT传感数据";
                }
                else
                {
                    pktTypeS = pktTypeV.ToString("X2");
                }

                // 协议版本
                inProtocol = SrcData[IndexOfStart + 1];

                // 第一RT/GW ID
                FirstRtGwIdV = CommArithmetic.ByteBuf_to_UInt32(SrcData, IndexOfStart + 2);
                FirstRtGwIdS = CommArithmetic.ToHexString(SrcData, IndexOfStart + 2, 4);

                // 第一RT/GW 接收时间
                FirstRtGwRdTime = CommArithmetic.DecodeDateTime(SrcData, IndexOfStart + 6);

                // 第一RT/GW RSSI
                rssi = SrcData[IndexOfStart + 12];
                if (rssi >= 0x80)
                {
                    FirstRtGwRssi = (double)(rssi - 0x100);
                }
                else
                {
                    FirstRtGwRssi = (double)rssi;
                }

                // 节点类型
                inNodeTypeV = SrcData[IndexOfStart + 13];
                if (inNodeTypeV == 0x00)
                {
                    inNodeTypeS = "相机";
                }
                else if (inNodeTypeV == 0x01)
                {
                    inNodeTypeS = "中继";
                }
                else if (inNodeTypeV == 0x02)
                {
                    inNodeTypeS = "传感器";
                }
                else
                {
                    inNodeTypeS = inNodeTypeV.ToString("X2");
                }

                // WP/RT ID
                WpRtIdV = CommArithmetic.ByteBuf_to_UInt32(SrcData, IndexOfStart + 14);
                WpRtIdS = CommArithmetic.ToHexString(SrcData, IndexOfStart + 14, 4);

                // 设备类型
                SetDeviceName(SrcData[IndexOfStart + 18]);

                // 采集时间
                SensorCollectTime = CommArithmetic.DecodeDateTime(SrcData, IndexOfStart + 19);

                // 采集序列号
                Serial = CommArithmetic.ByteBuf_to_UInt16(SrcData, IndexOfStart + 25);

                // 负载内容
                if (pktTypeV == 0 && inProtocol == 4)
                {   // SS传感数据包
                    PayloadTxt = "功能：" + SrcData[IndexOfStart + 27].ToString("X2");

                    PayloadTxt += ", L/H：" + SrcData[IndexOfStart + 28].ToString("X2");

                    PayloadTxt += ", 状态：" + SrcData[IndexOfStart + 29].ToString("X2");

                    PayloadTxt += ", 报警项：" + SrcData[IndexOfStart + 30].ToString("X2");

                    Int16 icTemp = SrcData[IndexOfStart + 31];
                    if (icTemp >= 0x80)
                    {
                        icTemp = (Int16)(icTemp - 0x100);
                    }
                    PayloadTxt += ", IC温度：" + icTemp.ToString();

                    PayloadTxt += ", 电压：" + ((double)CommArithmetic.ByteBuf_to_UInt16(SrcData, IndexOfStart + 32)/1000.0f).ToString("F3");

                    PayloadTxt += ", 小采：" + SrcData[IndexOfStart + 34].ToString();

                    PayloadTxt += ", 待发RAM：" + SrcData[IndexOfStart + 35].ToString();

                    PayloadTxt += ", 待发FLASH：" + CommArithmetic.ByteBuf_to_UInt16(SrcData, IndexOfStart + 36).ToString();

                    byte payLen = SrcData[IndexOfStart + 38];

                    IndexOfStart += 39;

                    PayloadTxt += ExplainPayload(SrcData, IndexOfStart, payLen);
                }
                else if (pktTypeV == 1 && inProtocol == 2)
                {   // WP/RT状态数据
                    PayloadTxt = "设备运行状态：" + SrcData[IndexOfStart + 27].ToString("X2") + " " + SrcData[IndexOfStart + 28].ToString("X2") +
                        " " + SrcData[IndexOfStart + 29].ToString("X2") + " " + SrcData[IndexOfStart + 30].ToString("X2");

                    PayloadTxt += ", 充电状态：";
                    byte chargeState = SrcData[IndexOfStart + 31];
                    if (chargeState == 0)
                    {
                        PayloadTxt += "无充电器";
                    }
                    else if (chargeState == 1)
                    {
                        PayloadTxt += "正在充电";
                    }
                    else if (chargeState == 2)
                    {
                        PayloadTxt += "充电完成";
                    }
                    else if (chargeState == 3)
                    {
                        PayloadTxt += "未接电池";
                    }
                    else if (chargeState == 4)
                    {
                        PayloadTxt += "未知错误";
                    }
                    else
                    {
                        PayloadTxt += "未定义";
                    }

                    PayloadTxt += ", 供电来源：";
                    byte supplySrc = SrcData[IndexOfStart + 32];
                    if (supplySrc == 0)
                    {
                        PayloadTxt += "无供电";
                    }
                    else if (supplySrc == 1)
                    {
                        PayloadTxt += "主电供电";
                    }
                    else if (supplySrc == 2)
                    {
                        PayloadTxt += "备电供电";
                    }
                    else if (supplySrc == 3)
                    {
                        PayloadTxt += "主电和备电同时供电";
                    }
                    else
                    {
                        PayloadTxt += "未定义";
                    }

                    PayloadTxt += ", 主电电量：";
                    double volt = (double)CommArithmetic.ByteBuf_to_UInt16(SrcData, IndexOfStart + 33) / 1000.0f;
                    PayloadTxt += volt.ToString("F3");

                    PayloadTxt += ", 备电电量：";
                    volt = (double)CommArithmetic.ByteBuf_to_UInt16(SrcData, IndexOfStart + 35) / 1000.0f;
                    PayloadTxt += volt.ToString("F3");

                    PayloadTxt += ", 文件待传数量：";
                    PayloadTxt += CommArithmetic.ByteBuf_to_UInt16(SrcData, IndexOfStart + 37).ToString();

                    PayloadTxt += ", 文件待传大小：";
                    PayloadTxt += CommArithmetic.ByteBuf_to_UInt32(SrcData, IndexOfStart + 39).ToString();

                    PayloadTxt += ", 状态包数量：";
                    PayloadTxt += CommArithmetic.ByteBuf_to_UInt16(SrcData, IndexOfStart + 43).ToString();

                    PayloadTxt += ", 传感包数量：";
                    PayloadTxt += CommArithmetic.ByteBuf_to_UInt16(SrcData, IndexOfStart + 45).ToString();

                    PayloadTxt += ", 转发包数量：";
                    PayloadTxt += CommArithmetic.ByteBuf_to_UInt32(SrcData, IndexOfStart + 47).ToString();

                    PayloadTxt += ", HOP：";
                    PayloadTxt += SrcData[IndexOfStart + 51].ToString();

                    PayloadTxt += ", 路径数量：";
                    byte routeNum = SrcData[IndexOfStart + 52];
                    PayloadTxt += routeNum.ToString();

                    IndexOfStart += 53;
                    int Unit = 10;

                    Int16 aRssi = 0;

                    for (int iCnt = 0; iCnt < routeNum; iCnt++)
                    {
                        PayloadTxt += ", 路径" + (iCnt + 1).ToString() + ":";
                        PayloadTxt += CommArithmetic.ByteBuf_to_UInt32(SrcData, IndexOfStart + iCnt * Unit).ToString("X8") + ",";

                        aRssi = (Int16)SrcData[IndexOfStart + iCnt * Unit + 4];
                        if (aRssi >= 0x80)
                        {
                            aRssi = (Int16)(aRssi - 0x100);
                        }
                        PayloadTxt += aRssi.ToString() + ",";

                        aRssi = (Int16)SrcData[IndexOfStart + iCnt * Unit + 5];
                        if (aRssi >= 0x80)
                        {
                            aRssi = (Int16)(aRssi - 0x100);
                        }
                        PayloadTxt += aRssi.ToString() + ",";

                        aRssi = (Int16)SrcData[IndexOfStart + iCnt * Unit + 6];
                        if (aRssi >= 0x80)
                        {
                            aRssi = (Int16)(aRssi - 0x100);
                        }
                        PayloadTxt += aRssi.ToString() + ",";

                        aRssi = (Int16)SrcData[IndexOfStart + iCnt * Unit + 7];
                        if (aRssi >= 0x80)
                        {
                            aRssi = (Int16)(aRssi - 0x100);
                        }
                        PayloadTxt += aRssi.ToString() + ",";

                        aRssi = (Int16)SrcData[IndexOfStart + iCnt * Unit + 8];
                        if (aRssi >= 0x80)
                        {
                            aRssi = (Int16)(aRssi - 0x100);
                        }
                        PayloadTxt += aRssi.ToString() + ",";

                        aRssi = (Int16)SrcData[IndexOfStart + iCnt * Unit + 9];
                        if (aRssi >= 0x80)
                        {
                            aRssi = (Int16)(aRssi - 0x100);
                        }
                        PayloadTxt += aRssi.ToString();
                    }
                }
                else if (pktTypeV == 2 && inProtocol == 1)
                {   // RT传感数据
                    PayloadTxt = "状态：" + SrcData[IndexOfStart + 27].ToString("X2");

                    byte payLen = SrcData[IndexOfStart + 28];

                    IndexOfStart += 29;

                    PayloadTxt += ExplainPayload(SrcData, IndexOfStart, payLen);                    
                }
            }

            // 附加时间
            SensorTransforTime = System.DateTime.Now;
        }

        //----------------        

    }
}
