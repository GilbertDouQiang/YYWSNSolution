using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace YyWsnDeviceLibrary
{
    public class M40 : Sensor
    {
        /**************************************
         * 属性
         * ************************************/

        /// <summary>
        /// 数据包的类型
        /// </summary>
        private DataPktType dataPktType { get; set; }       // 外部只读

        /// <summary>
        /// 开门状态，该属性的源数据是一个字节，为了体现负数，所以选择使用Int16类型
        /// </summary>
        public Int16 openStateV { get; set; }

        /// <summary>
        /// 开门状态，字符串显示
        /// </summary>
        public string openStateS { get; set; }

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
            return (byte)DeviceType.M40;
        }

        /// <summary>
        /// 判断输入的设备类型是否与本设备类型一致
        /// </summary>
        /// <param name="iDeviceType"></param>
        /// <returns></returns>
        static public bool isDeviceType(byte iDeviceType)
        {
            if (iDeviceType == (byte)DeviceType.M40)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void OpenState_Set(byte iOpenState)
        {
            openStateV = (Int16)iOpenState;
            if (openStateV > 0x80)
            {
                openStateV -= 256;
            }

            if(openStateV < 0)
            {
                openStateS = "异常" + openStateV.ToString("G");
            }else
            {
                switch (openStateV)
                {
                    case 0:
                        {
                            openStateS = "关闭";
                            break;
                        }
                    case 1:
                        {
                            openStateS = "开启";
                            break;
                        }
                    case 2:
                        {
                            openStateS = "开->关";                           
                            break;
                        }
                    case 3:
                        {
                            openStateS = "关->开";
                            break;
                        }
                    default:
                        {
                            openStateS = "未知" + openStateV.ToString("G");
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

        public M40() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="SrcData"></param>
        public M40(byte[] SrcData, UInt16 IndexOfStart, DataPktType dataPktType)
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

                    AlertCfg = SrcData[38];

                    ICTemperature = SrcData[IndexOfStart + 39];
                    voltF = Math.Round(Convert.ToDouble((SrcData[IndexOfStart + 40] * 256 + SrcData[IndexOfStart + 41])) / 1000, 2);

                    FlashID = CommArithmetic.DecodeClientID(SrcData, IndexOfStart + 42);
                    MaxLength = SrcData[IndexOfStart + 44];

                    FlashFront = SrcData[IndexOfStart + 45] * 256 * 256 + SrcData[IndexOfStart + 46] * 256 + SrcData[IndexOfStart + 47];
                    FlashRear = SrcData[IndexOfStart + 48] * 256 * 256 + SrcData[IndexOfStart + 49] * 256 + SrcData[IndexOfStart + 50];
                    FlashQueueLength = SrcData[IndexOfStart + 51] * 256 * 256 + SrcData[IndexOfStart + 52] * 256 + SrcData[IndexOfStart + 53];

                    OpenState_Set(SrcData[IndexOfStart + 54]);

                    byte rssi = SrcData[IndexOfStart + 58];
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
            else if (dataPktType == DataPktType.SensorFromSsToGw)
            {
                if (isSensorDataV3(SrcData, IndexOfStart, true) >= 0)
                {
                    ReadSensorDataV3(SrcData, IndexOfStart, true);
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
            if (dataType != 0x82)
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

            // 开门状态
            OpenState_Set(SrcData[IndexOfStart + 28]);

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
