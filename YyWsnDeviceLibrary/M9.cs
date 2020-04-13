using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace YyWsnDeviceLibrary
{
    public class M9 : Sensor
    {
        /**************************************
         * 属性
         * ************************************/

        /// <summary>
        /// 数据包的类型
        /// </summary>
        private DataPktType dataPktType { get; set; }       // 外部只读

        /// <summary>
        /// 运动状态，该属性的源数据是一个字节，为了体现负数，所以选择使用Int16类型
        /// </summary>
        public Int16 moveStateV { get; set; }

        /// <summary>
        /// 运动状态，字符串显示
        /// </summary>
        public string moveStateS { get; set; }

        /// <summary>
        /// 运动检测阈值，单位：mg
        /// </summary>
        public UInt16 MoveDetectThr { get; set; }

        /// <summary>
        /// 运动检测时间，单位：ms
        /// </summary>
        public UInt16 MoveDetectTime { get; set; }

        /// <summary>
        /// 静止检测阈值，单位：mg
        /// </summary>
        public UInt16 StaticDetectThr { get; set; }

        /// <summary>
        /// 静止检测时间，单位：ms
        /// </summary>
        public UInt16 StaticDetectTime { get; set; }

        /// <summary>
        /// 报警条件
        /// </summary>
        public byte AlertCfg { get; set; }


        /**************************************
         * 方法
         * ************************************/

        /// <summary>
        /// 获取设备类型编码
        /// </summary>
        /// <returns></returns>
        static public byte GetDeviceType()
        {
            return (byte)DeviceType.M9;
        }

        /// <summary>
        /// 判断输入的设备类型是否与本设备类型一致
        /// </summary>
        /// <param name="iDeviceType"></param>
        /// <returns></returns>
        static public bool isDeviceType(byte iDeviceType)
        {
            if (iDeviceType == (byte)DeviceType.M9)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void MoveState_Set(byte iMoveState)
        {
            moveStateV = (Int16)iMoveState;
            if (moveStateV > 0x80)
            {
                moveStateV -= 256;
            }

            if(moveStateV < 0)
            {
                moveStateS = "异常" + moveStateV.ToString("G");
            }else
            {
                switch (moveStateV)
                {
                    case 0:
                        {
                            moveStateS = "静止";
                            break;
                        }
                    case 1:
                        {
                            moveStateS = "运动";
                            break;
                        }
                    case 2:
                        {
                            moveStateS = "动->静";
                            break;
                        }
                    case 3:
                        {
                            moveStateS = "静->动";
                            break;
                        }
                    default:
                        {
                            moveStateS = "未知" + moveStateV.ToString("G");
                            break;
                        }
                }                
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

        public M9() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="SrcData"></param>
        public M9(byte[] SrcData, UInt16 IndexOfStart)
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
        public M9(byte[] SrcData, UInt16 IndexOfStart, DataPktType dataPktType)
        {
            if (dataPktType == DataPktType.SelfTestFromUsbToPc)
            {
                if ((byte)Device.IsPowerOnSelfTestPktFromUsbToPc(SrcData, IndexOfStart) == GetDeviceType())
                {
                    byte protocol = SrcData[IndexOfStart + 5];
                    if (protocol != 3)
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
                    Interval = (UInt16)(SrcData[IndexOfStart + 25] * 256 + SrcData[IndexOfStart + 26]);
                    Calendar = CommArithmetic.DecodeDateTime(SrcData, (UInt16)(IndexOfStart + 27));

                    Pattern = SrcData[IndexOfStart + 33];
                    Bps = SrcData[IndexOfStart + 34];
                    SetTxPower(SrcData[IndexOfStart + 35]);
                    SampleSend = SrcData[IndexOfStart + 36];
                    Channel = SrcData[IndexOfStart + 37];

                    MoveDetectThr = (UInt16)(SrcData[IndexOfStart + 38] * 256 + SrcData[IndexOfStart + 39]);
                    MoveDetectTime = (UInt16)(SrcData[IndexOfStart + 40] * 256 + SrcData[IndexOfStart + 41]);

                    StaticDetectThr = (UInt16)(SrcData[IndexOfStart + 42] * 256 + SrcData[IndexOfStart + 43]);
                    StaticDetectTime = (UInt16)(SrcData[IndexOfStart + 44] * 256 + SrcData[IndexOfStart + 45]);

                    AlertCfg = SrcData[46];

                    ICTemperature = SrcData[IndexOfStart + 47];
                    voltF = Math.Round(Convert.ToDouble((SrcData[IndexOfStart + 48] * 256 + SrcData[IndexOfStart + 49])) / 1000, 2);

                    FlashID = CommArithmetic.DecodeClientID(SrcData, IndexOfStart + 50);
                    MaxLength = SrcData[IndexOfStart + 52];

                    FlashFront = SrcData[IndexOfStart + 53] * 256 * 256 + SrcData[IndexOfStart + 54] * 256 + SrcData[IndexOfStart + 55];
                    FlashRear = SrcData[IndexOfStart + 56] * 256 * 256 + SrcData[IndexOfStart + 57] * 256 + SrcData[IndexOfStart + 58];
                    FlashQueueLength = SrcData[IndexOfStart + 59] * 256 * 256 + SrcData[IndexOfStart + 60] * 256 + SrcData[IndexOfStart + 61];

                    MoveState_Set(SrcData[IndexOfStart + 62]);

                    byte rssi = SrcData[IndexOfStart + 66];
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
        /// 判断是不是监测工具监测到的M9发出的传感器数据包（V3版本）
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
            if (SrcLen < 35 + AppendLen)
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
            if (payLen != 2)
            {
                return -9;
            }

            // 数据类型
            byte dataType = SrcData[IndexOfStart + 27];
            if (dataType != 0x7F)
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

            // 运动状态
            MoveState_Set(SrcData[IndexOfStart + 28]);

            // To Send Ram
            ToSendRam = SrcData[IndexOfStart + 29];

            // To Send Flash
            ToSendFlash = (UInt16)(SrcData[IndexOfStart + 30] * 256 + SrcData[IndexOfStart + 31]);

            // RSSI
            if (ExistRssi == true)
            {
                byte rssi = SrcData[IndexOfStart + 35];
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
