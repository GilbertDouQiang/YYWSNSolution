﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YyWsnDeviceLibrary
{
    /// <summary>
    /// 所有设备的父类，抽象类，不能直接实例化
    /// </summary>
    public abstract class Device
    {
        /// <summary>
        /// 设备类型列表
        /// </summary>
        public enum DeviceType
        {
            M1 = 0x51,                  // M1：CC1310+W25Q16CL+SHT30
            SG2CC1310 = 0x52,           // SG2:CC1310+W25Q16CL+ADP5062+MSP430F5529+W25Q16CL+Air200
            S1P = 0x53,                 // S1+：CC1310+PM25LQ020+SHT20
            USB0 = 0x54,                // USB0: MSP430F5529+CC1101
            GM = 0x55,                  // GM:CC1310+W25Q16CL
            USB1 = 0x56,                // USB1: MSP430F5529+CC1310
            M2 = 0x57,                  // M2:CC1310+W25Q16CL+Buzzer+段码屏+MAX31855
            SK = 0x58,                  // SK:CC1310+W25Q16CL+HLW8012
            AlertMSP432 = 0x59,         // Alert: MSP432P401R+MSP430F5510+CC1310+W25Q256FV+LCD+Buzzer
            S1 = 0x5A,                  // S1: MSP430F5510+CC1101+SHT20+PM25LD020
            SGA3 = 0x5B,                // SGA3: MSP432P401R
            M1_NTC = 0x5C,              // M1_NTC: CC1310+W25Q16CL+NTC3950
            M1_Beetech = 0x5D,          // M1_Beetech:CC1310+W25Q16CL+SHT30+CP2102
            AlertCC1310 = 0x5E,         // Alert(CC1310)
            SG5CC1310 = 0x5F,           // SG5(CC1310):MSP432+CC1310+W25Q256FV+LCD+M26+泰斗+CP2102
            SG5MSP432= 0x60,            // SG5(MSP432):MSP432+CC1310+W25Q256FV+LCD+M26+泰斗+CP2102
            SC = 0x61,                  // SC: MSP430F5529+ADG712
            TB2 = 0x62,                 // TB2: CC1310+CMT2300A
            USB2 = 0x63,                // USB2: CC1310+CP2102
            BB = 0x64,                  // BB: MSP430F5529+CC1101+SIM800A(+SIM28)
            SG5CC1310SHT30 = 0x65,      // SG5_CC1310_SHT30
            SG5CC1310NTC = 0x66,        // SG5_CC1310_NTC
            SG5CC1310PT100 = 0x67,      // SG5_CC1310_PT100
            SG6MSP432 = 0x68,           // SG6(MSP432):MSP432+CC1310+W25Q256FV+LCD+SIM7600CE+CP2102
            SG6CC1310 = 0x69,           // SG6(CC1310):MSP432+CC1310+W25Q256FV+LCD+SIM7600CE+CP2102
            SG6PMSP432 = 0x6A,          // SG6P(MSP432):MSP432+CC1310(PA)+W25Q256FV+LCD+SIM7600CE+CP2102+CC2640+ADXL362
            SG6PCC1310 = 0x6B,          // SG6P(CC1310):MSP432+CC1310(PA)+W25Q256FV+LCD+SIM7600CE+CP2102+CC2640+ADXL362
            SG6PCC2640 = 0x6C,          // SG6P(CC2640):MSP432+CC1310(PA)+W25Q256FV+LCD+SIM7600CE+CP2102+CC2640+ADXL362
            M6 = 0x6D,                  // M6:CC1310(PA)+W25Q16CL+SHT30+MCP144+段码屏
            M2_PT100 = 0x6E,            // M2:CC1310+W25Q16CL+Buzzer+段码屏+ADS1220+PT100
            M2_SHT30 = 0x6F,            // M2:CC1310+W25Q16CL+Buzzer+段码屏+SHT30
            PM = 0x70,                  // PM: CC1310+SKY66115
            LBGZ_TC04MSP432 = 0x71,     // LBGZ_TC04(MSP432):MSP432+CC1310(PA)+W25Q256FV+LCD+SIM7600CE+CP2102
            LBGZ_TC04CC1310 = 0x72,     // LBGZ_TC04(CC1310):MSP432+CC1310(PA)+W25Q256FV+LCD+SIM7600CE+CP2102
            SG6XMSP432 = 0x73,          // SG6X(MSP432):MSP432+CC1310(PA)+CC1101+W25Q256FV+LCD+SIM7600CE+CP2102+CC2640+ADXL362
            SG6XCC1310 = 0x74,          // SG6X(CC1310):MSP432+CC1310(PA)+CC1101+W25Q256FV+LCD+SIM7600CE+CP2102+CC2640+ADXL362
            SG6XCC2640 = 0x75,          // SG6X(CC2640):MSP432+CC1310(PA)+CC1101+W25Q256FV+LCD+SIM7600CE+CP2102+CC2640+ADXL362
            S2 = 0x76,                  // S2: MSP430F5529+CC1101+PM25LD020+MAX31855
            M9 = 0x77,                  // M9: CC1310+W25Q16CL+ADXL362
            ACO2 = 0x78,                // ACO2:CC1310+SHT30+MinIR-C02+W25Q16CL
            M30 = 0x79,                 // M30:MSP432+CC1310+W25Q256FV+LCD+CP2102
            AO2 = 0x7A,                 // AO2:CC1310+W25Q16CL+SHT30+LMP91000+02
            RT = 0x7B,                  // RT:CC1352P+W25Q16CL+TPL5010
            GMP = 0x7C,                 // GMP:CC1352P+GD25VQ32C+TPL5010，支持蓝牙
            ZQM1 = 0x7D,                // ZQM1:CC1310+GD25VQ32C+SHT30
            M5 = 0x7E,                  // M5:CC1310+GD25VQ32C+HP303S(BMP280) 
            M40 = 0x7F,                 // （门磁）M40: CC1310+GD25VQ32C+干簧管
            M20 = 0x80,                 // M20:M20+3SHT30
            M1X = 0x81,                 // M1X: CC1310+W25Q16CL+SHT30+TPL5010
            ZQSG1CC1352P = 0x82,        // ZQSG1(CC1352P)
            ZQSG1MSP432 = 0x83,         // ZQSG1(MSP432)
            M10 = 0x84,                 // M10
            L1 = 0x85,                  // L1(光照传感器)
            ESK = 0x8A,                 // 爱立信，ESK
            IR20 = 0x8B,                // IR20
        }

        /// <summary>
        /// 数据包类型列表
        /// </summary>
        public enum DataPktType
        {
            Null,                   // 
            SelfTest,               // SS发出的上电自检数据包
            SensorFromSsToGw,       // SS发给GW的传感数据包
            AckFromGwToSs,          // GW发给SS的确认包
            SensorDataFromGmToPc,   // GM反馈给上位机的Sensor数据包
            SensorDataMax31855Debug, // MAX31855 Debug数据包
            SelfTestFromUsbToPc,    // USB修改工具接收到的Sensor的上电自检数据包
        }

        /// <summary>
        /// 源数据
        /// </summary>
        public String SourceData { get; set; }

        /// <summary>
        /// 列表显示时的序号
        /// </summary>
        public int DisplayID { get; set; }

        /// <summary>
        /// 电池电压，单位：mV
        /// </summary>
        public UInt16 volt { get; set; }

        /// <summary>
        /// 电池电压，单位：V
        /// </summary>
        public double voltF { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 设备类型
        /// </summary>
        public byte DeviceTypeV { get; set; }

        /// <summary>
        /// 设备类型，如M1 代号为51
        /// </summary>
        public String DeviceTypeS { get; set; }

        /// <summary>
        /// DQ: 设备的原始MAC
        /// </summary>
        public UInt32 PrimaryMacV { get; set; }

        /// <summary>
        /// 设备的原始MAC
        /// </summary>
        public String PrimaryMacS { get; set; }

        /// <summary>
        /// DQ: 设备的MAC地址
        /// </summary>
        public UInt32 DeviceMacV { get; set; }

        /// <summary>
        /// 设备的8位MAC地址
        /// </summary>
        public String DeviceMacS { get; set; }

        /// <summary>
        /// 设备的新的8位MAC地址
        /// </summary>
        public string DeviceMacNewS { get; set; }

        /// <summary>
        /// DQ: 硬件版本
        /// </summary>
        public UInt32 HwRevisionV { get; set; }

        /// <summary>
        /// 硬件版本
        /// </summary>
        public string HwRevisionS { get; set; }

        /// <summary>
        /// DQ: 软件版本
        /// </summary>
        public UInt16 SwRevisionV { get; set; }

        /// <summary>
        /// 软件版本
        /// </summary>
        public string SwRevisionS { get; set; }

        /// <summary>
        /// DQ: 客户码
        /// </summary>
        public UInt16 CustomerV { get; set; }

        /// <summary>
        /// 客户识别码
        /// </summary>
        public String CustomerS { get; set; }

        /// <summary>
        /// Debug
        /// </summary>
        public UInt16 DebugV { get; set; }

        /// <summary>
        /// Debug
        /// </summary>
        public String DebugS { get; set; }

        /// <summary>
        /// 协议版本
        /// </summary>
        public byte ProtocolVersion { get; set; }

        /// <summary>
        /// 分类码
        /// </summary>
        public byte Category { get; set; }

        /// <summary>
        /// 采集间隔，单位：秒
        /// </summary>
        public UInt16 Interval { get; set; }

        /// <summary>
        /// 正常传输间隔，单位：秒
        /// </summary>
        public UInt16 NormalInterval { get; set; }

        /// <summary>
        /// 预警传输间隔，单位：秒
        /// </summary>
        public UInt16 WarnInterval { get; set; }

        /// <summary>
        /// 报警传输间隔，单位：秒
        /// </summary>
        public UInt16 AlertInterval { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime Calendar { get; set; }

        /// <summary>
        /// 工作模式
        /// </summary>
        public byte Pattern { get; set; }

        /// <summary>
        /// 传输速率
        /// </summary>
        public byte Bps { get; set; }

        /// <summary>
        /// 数据包的起始位
        /// </summary>
        public byte STP { get; set; }

        /// <summary>
        /// 设备的最后传输日期和时间
        /// </summary>
        public DateTime LastTransforDate { get; set; }

        /// <summary>
        /// DQ: 设备的当前时间
        /// </summary>
        public DateTime CurrentT { get; set; }

        /// <summary>
        /// 最新数据/历史数据
        /// </summary>
        public byte LastHistory { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public byte State { get; set; }

        /// <summary>
        /// 报警项
        /// </summary>
        public byte AlarmItem { get; set; }

        /// <summary>
        /// 小采
        /// </summary>
        public byte AltSerial { get; set; }

        /// <summary>
        /// RAM队列中的待发数据的数量
        /// </summary>
        public byte ToSendRam { get; set; }

        /// <summary>
        /// Flash队列中的待发数据的数量
        /// </summary>
        public UInt16 ToSendFlash { get; set; }

        public override string ToString()
        {
            return SourceData;
        }

        /// <summary>
        /// 设置设备类型，并且更新Name属性
        /// </summary>
        /// <param name="deviceType"></param>
        public string SetDeviceName(byte deviceType)
        {
            DeviceTypeV = deviceType;
            DeviceTypeS = deviceType.ToString("X2");

            switch (deviceType)
            {
                case 0x51:
                    {
                        Name = "M1";
                        break;
                    }
                case 0x52:
                    {
                        Name = "SG2";
                        break;
                    }
                case 0x53:
                    {
                        Name = "M1P";
                        break;
                    }
                case 0x54:
                    {
                        Name = "USB_MSP430+CC1101";
                        break;
                    }
                case 0x55:
                    {
                        Name = "GM";
                        break;
                    }
                case 0x56:
                    {
                        Name = "USB_MSP430+CC1310";
                        break;
                    }
                case 0x57:
                    {
                        Name = "M2";
                        break;
                    }
                case 0x58:
                    {
                        Name = "M4(SK)";
                        break;
                    }
                case 0x59:
                    {
                        Name = "Alert";
                        break;
                    }
                case 0x5A:
                    {
                        Name = "S1";
                        break;
                    }
                case 0x5B:
                    {
                        Name = "SGA3";
                        break;
                    }
                case 0x5C:
                    {
                        Name = "M1_NTC";
                        break;
                    }
                case 0x5D:
                    {
                        Name = "M1_Beetech";
                        break;
                    }
                case 0x60:
                    {
                        Name = "SG5";
                        break;
                    }
                case 0x61:
                    {
                        Name = "SC";
                        break;
                    }
                case 0x62:
                    {
                        Name = "TB2";
                        break;
                    }
                case 0x63:
                    {
                        Name = "USB_CC1310+CP2102";
                        break;
                    }
                case 0x64:
                    {
                        Name = "BB";
                        break;
                    }
                case 0x65:
                    {
                        Name = "SGX_SHT30";
                        break;
                    }
                case 0x66:
                    {
                        Name = "SGX_NTC";
                        break;
                    }
                case 0x67:
                    {
                        Name = "SGX_PT100";
                        break;
                    }
                case 0x68:
                    {
                        Name = "SG6";
                        break;
                    }
                case 0x6A:
                    {
                        Name = "SG6P";
                        break;
                    }
                case 0x6D:
                    {
                        Name = "M6";
                        break;
                    }
                case 0x6E:
                    {
                        Name = "M2_PT100";
                        break;
                    }
                case 0x6F:
                    {
                        Name = "M2_SHT30";
                        break;
                    }
                case 0x70:
                    {
                        Name = "PM";
                        break;
                    }
                case 0x77:
                    {
                        Name = "M9(振动)";
                        break;
                    }
                case 0x7A:
                    {
                        Name = "AO2";
                        break;
                    }
                case 0x7D:
                    {
                        Name = "M1_Zigin";
                        break;
                    }
                case 0x7F:
                    {
                        Name = "M40（门磁）";
                        break;
                    }
                case 0x80:
                    {
                        Name = "M20";
                        break;
                    }
                case 0x81:
                    {
                        Name = "M1X";
                        break;
                    }
                case 0x82:
                    {
                        Name = "ZQSG1CC1352P";
                        break;
                    }
                case 0x83:
                    {
                        Name = "ZQSG1MSP432";
                        break;
                    }
                case 0x84:
                    {
                        Name = "M10";
                        break;
                    }
                case 0x85:
                    {
                        Name = "L1";
                        break;
                    }
                case 0x8A:
                    {
                        Name = "ESK";
                        break;
                    }
                default:
                    {
                        Name = deviceType.ToString("X2");
                        break;
                    }
            }

            return Name;
        }

        /// <summary>
        /// 设置Device的Primary Mac
        /// </summary>
        public void SetDevicePrimaryMac(byte[] SrcData, UInt16 StartIndex)
        {
            PrimaryMacS = CommArithmetic.DecodeMAC(SrcData, StartIndex);

            PrimaryMacV = (UInt32)(SrcData[StartIndex] * 256 * 256 * 256 + SrcData[StartIndex + 1] * 256 * 256 + SrcData[StartIndex + 2] * 256 + SrcData[StartIndex + 3]);
        }

        /// <summary>
        /// 设置Device Mac
        /// </summary>
        public void SetDeviceMac(byte[] SrcData, UInt16 StartIndex)
        {
            DeviceMacS = CommArithmetic.DecodeMAC(SrcData, StartIndex);

            DeviceMacV = (UInt32)(SrcData[StartIndex] * 256 * 256 * 256 + SrcData[StartIndex + 1] * 256 * 256 + SrcData[StartIndex + 2] * 256 + SrcData[StartIndex + 3]);
        }

        /// <summary>
        /// 设置Haredware Revision
        /// </summary>
        public void SetHardwareRevision(byte[] SrcData, UInt16 StartIndex)
        {
            HwRevisionS = CommArithmetic.DecodeMAC(SrcData, StartIndex);

            HwRevisionV = (UInt32)(SrcData[StartIndex] * 256 * 256 * 256 + SrcData[StartIndex + 1] * 256 * 256 + SrcData[StartIndex + 2] * 256 + SrcData[StartIndex + 3]);
        }


        /// <summary>
        /// 设置Software Revision
        /// </summary>
        public void SetSoftwareRevision(byte[] SrcData, UInt16 StartIndex)
        {
            SwRevisionS = CommArithmetic.DecodeClientID(SrcData, StartIndex);

            SwRevisionV = (UInt16)(SrcData[StartIndex] * 256 + SrcData[StartIndex + 1]);
        }

        /// <summary>
        /// 设置Device的客户码
        /// </summary>
        public void SetDeviceCustomer(byte[] SrcData, UInt16 StartIndex)
        {
            CustomerS = CommArithmetic.DecodeClientID(SrcData, StartIndex);

            CustomerV = (UInt16)(SrcData[StartIndex] * 256 + SrcData[StartIndex + 1]);
        }

        /// <summary>
        /// 设置Device的Debug
        /// </summary>
        public void SetDeviceDebug(byte[] SrcData, UInt16 StartIndex)
        {
            DebugS = CommArithmetic.DecodeClientID(SrcData, StartIndex);

            DebugV = (UInt16)(SrcData[StartIndex] * 256 + SrcData[StartIndex + 1]);
        }

        /// <summary>
        /// 判断数据包是否是USB监控到的Sensor上电自检数据包
        /// </summary>
        static public Int16 IsPowerOnSelfTestPktFromUsbToPc(byte[] SrcData, UInt16 StartIndex)
        {
            // 确保数据包长度达到最小长度要求
            if(SrcData.Length - StartIndex < 5)
            {
                return -1;
            }

            // 起始位
            if (SrcData[StartIndex] != 0xEC)
            {
                return -2;
            }

            // 长度位
            byte pktLen = SrcData[StartIndex + 1];
            if(pktLen + 5 > SrcData.Length - StartIndex)
            {
                return -3;
            }

            // 结束位
            if(SrcData[StartIndex + 2 + pktLen + 2] != 0xCE)
            {
                return -4;
            }

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, SrcData, (UInt16)(StartIndex + 2), pktLen);
            UInt16 crc_chk = (UInt16)(((UInt16)SrcData[StartIndex + 2 + pktLen] << 8) | ((UInt16)SrcData[StartIndex + 2 + pktLen + 1] << 0));
            if(crc != crc_chk && crc_chk != 0)
            {
                return -5;
            }

            // Cmd
            if(SrcData[StartIndex + 2] != 0x01)
            {
                return -6;
            }

            // 设备类型
            byte deviceType = SrcData[StartIndex + 4];

            // Protocol
            byte protocol = SrcData[StartIndex + 5];
            if (protocol == 0)
            {
                // 空
            }

            return (Int16)deviceType;
        }


        /// <summary>
        /// 判断数据包是否是服务器收到的网关上传的数据包
        /// </summary>
        /// <param name="SrcData"></param>
        /// <param name="StartIndex"></param>
        /// <returns></returns>
        static public Int16 IsPktFromGatewayToServer(byte[] SrcData)
        {
            // 确保数据包长度达到最小长度要求
            if (SrcData.Length < 9)
            {
                return -1;
            }

            // 起始位
            if (SrcData[0] != 0xBE || SrcData[1] != 0xBE)
            {
                return -2;
            }

            // 长度位
            byte pktLen = SrcData[2];
            if (pktLen + 7 > SrcData.Length)
            {
                return -3;
            }

            // 结束位
            if (SrcData[3 + pktLen + 2] != 0xEB || SrcData[3 + pktLen + 3] != 0xEB)
            {
                return -4;
            }

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, SrcData, 3, pktLen);
            UInt16 crc_chk = (UInt16)(((UInt16)SrcData[3 + pktLen] << 8) | ((UInt16)SrcData[3 + pktLen + 1] << 0));
            if (crc != crc_chk && crc_chk != 0)
            {
                return -5;
            }

            return 0;
        }
    }
}
