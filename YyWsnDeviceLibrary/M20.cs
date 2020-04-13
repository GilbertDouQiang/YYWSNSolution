using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace YyWsnDeviceLibrary
{
    public class M20:Sensor
    {
        /**************************************
         * 属性
         * ************************************/
        
        /// <summary>
        /// 数据包类型
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
        /// 标识
        /// </summary>
        public byte ifg { get; set; }

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
        /// 湿度，单位：℃
        /// </summary>
        public double humF { get; set; }

        /// <summary>
        /// 标识
        /// </summary>
        public byte ifg1 { get; set; }

        /// <summary>
        /// 温度，单位：0.01℃
        /// </summary>
        public Int16 temp1 { get; set; }

        /// <summary>
        /// 温度，单位：℃
        /// </summary>
        public double temp1F { get; set; }

        /// <summary>
        /// 湿度，单位：0.01%
        /// </summary>
        public UInt16 hum1 { get; set; }

        /// <summary>
        /// 湿度，单位：℃
        /// </summary>
        public double hum1F { get; set; }

        /// <summary>
        /// 标识
        /// </summary>
        public byte ifg2 { get; set; }

        /// <summary>
        /// 温度，单位：0.01℃
        /// </summary>
        public Int16 temp2 { get; set; }

        /// <summary>
        /// 温度，单位：℃
        /// </summary>
        public double temp2F { get; set; }

        /// <summary>
        /// 湿度，单位：0.01%
        /// </summary>
        public UInt16 hum2 { get; set; }

        /// <summary>
        /// 湿度，单位：℃
        /// </summary>
        public double hum2F { get; set; }

        /// <summary>
        /// 温度预警上限
        /// </summary>
        public double TemperatureInfoHigh { get; set; }

        /// <summary>
        /// 温度预警下限
        /// </summary>
        public double TemperatureInfoLow { get; set; }

        /// <summary>
        /// 温度报警上限
        /// </summary>
        public double TemperatureWarnHigh { get; set; }

        /// <summary>
        /// 温度报警下限
        /// </summary>
        public double TemperatureWarnLow { get; set; }

        /// <summary>
        /// 湿度预警上限
        /// </summary>
        public double HumidityInfoHigh { get; set; }
        /// <summary>
        /// 湿度预警下限
        /// </summary>
        public double HumidityInfoLow { get; set; }

        /// <summary>
        /// 湿度报警上限
        /// </summary>
        public double HumidityWarnHigh { get; set; }

        /// <summary>
        /// 湿度报警下限
        /// </summary>
        public double HumidityWarnLow { get; set; }

        /// <summary>
        /// 温度补偿
        /// </summary>
        public double TemperatureCompensation { get; set; }

        /// <summary>
        /// 湿度补偿
        /// </summary>
        public double HumidityCompensation { get; set; }

        /// <summary>
        /// MAX31855的采集结果，源数据是一个字节
        /// </summary>
        public Int16 SampleResult { get; set; }

        /// <summary>
        /// MAX31855的寄存器值
        /// </summary>
        public UInt32 RegValue { get; set; }

        /// <summary>
        /// MAX31855的热电偶温度，单位：0.01℃
        /// </summary>
        public Int16 thermocoupleTemp { get; set; }

        /// <summary>
        /// MAX31855的热电偶温度，单位：℃
        /// </summary>
        public double thermocoupleTempF { get; set; }

        /// <summary>
        /// MAX31855的芯片温度，单位：0.01℃
        /// </summary>
        public Int16 internalTemp { get; set; }

        /// <summary>
        /// MAX31855的芯片温度，单位：℃
        /// </summary>
        public double internalTempF { get; set; }


        /**************************************
         * 方法
         * ************************************/

        /// <summary>
        /// 获取设备类型编码
        /// </summary>
        /// <returns></returns>
        static public byte GetDeviceType()
        {
            return (byte)DeviceType.M20;
        }

        /// <summary>
        /// 判断输入的设备类型是否与本设备类型一致
        /// </summary>
        /// <param name="iDeviceType"></param>
        /// <returns></returns>
        static public bool isDeviceType(byte iDeviceType)
        {
            if (iDeviceType == (byte)DeviceType.M20)
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

        public M20() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="SrcData"></param>
        public M20(byte[] SrcData, UInt16 IndexOfStart)
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
        public M20(byte[] SrcData, UInt16 IndexOfStart, Device.DataPktType pktType, Device.DeviceType deviceType)
        {
            if (deviceType != Device.DeviceType.M20)
            {
                return;
            }

            UInt16 iCnt = 0;

            if (pktType == Device.DataPktType.SensorDataFromGmToPc)
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

                iCnt += 2;

                // 标识符、温度、湿度  
                ifg = SrcData[iCnt];
                iCnt++;

                temp = (Int16)(SrcData[iCnt] * 256 + SrcData[iCnt + 1]);
                tempF = (double)(temp / 100.0f);
                iCnt += 2;

                hum = (UInt16)(SrcData[iCnt] * 256 + SrcData[iCnt + 1]);
                humF = (double)(hum / 100.0f);
                iCnt += 2;

                // 标识符、温度、湿度  
                ifg1 = SrcData[iCnt];
                iCnt++;

                temp1 = (Int16)(SrcData[iCnt] * 256 + SrcData[iCnt + 1]);
                temp1F = (double)(temp1 / 100.0f);
                iCnt += 2;

                hum1 = (UInt16)(SrcData[iCnt] * 256 + SrcData[iCnt + 1]);
                hum1F = (double)(hum1 / 100.0f);
                iCnt += 2;

                // 标识符、温度、湿度  
                ifg2 = SrcData[iCnt];
                iCnt++;

                temp2 = (Int16)(SrcData[iCnt] * 256 + SrcData[iCnt + 1]);
                temp2F = (double)(temp2 / 100.0f);
                iCnt += 2;

                hum2 = (UInt16)(SrcData[iCnt] * 256 + SrcData[iCnt + 1]);
                hum2F = (double)(hum2 / 100.0f);
                iCnt += 2;

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

            return;
        }

        /// <summary>
        /// 判断是不是USB修改工具抓到的上电自检数据包
        /// </summary>
        /// <param name="SrcBuf"></param>
        /// <param name="ExistRssi">true = 包含RSSI； false = 不包含RSSI；</param>
        /// <returns></returns>
        static public Int16 isPowerOnSelfTestPkt(byte[] SrcData, UInt16 IndexOfStart)
        {
            // 数据包的总长度
            UInt16 SrcLen = (UInt16)(SrcData.Length - IndexOfStart);
            if (SrcLen < 90)
            {
                return -1;
            }

            // 起始位
            if (SrcData[IndexOfStart + 0] != 0xEC)
            {
                return -2;
            }

            // 长度位
            byte pktLen = SrcData[IndexOfStart + 1];
            if (pktLen + 5 > SrcLen)
            {
                return -3;
            }

            if (SrcData[IndexOfStart + 2 + pktLen + 2] != 0xCE)
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
            byte DeviceType = SrcData[IndexOfStart + 4];
            if (isDeviceType(DeviceType) == false)
            {
                return -7;
            }

            // 协议版本
            byte protocol = SrcData[IndexOfStart + 5];
            if (protocol != 3)
            {
                return -8;
            }

            return 0;
        }

        /// <summary>
        /// 判断是不是监测工具监测到的M1发出的传感器数据包（V1版本）
        /// </summary>
        /// <param name="SrcBuf"></param>
        /// <param name="ExistRssi">true = 包含RSSI； false = 不包含RSSI；</param>
        /// <returns></returns>
        static public Int16 isSensorDataV1(byte[] SrcData, UInt16 IndexOfStart, bool ExistRssi)
        {
            UInt16 AppendLen = 0;
            if (ExistRssi == true)
            {
                AppendLen = 1;
            }

            // 数据包的总长度
            UInt16 SrcLen = (UInt16)(SrcData.Length - IndexOfStart);
            if (SrcLen < 30 + AppendLen)
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
            if (protocol != 1)
            {
                return -8;
            }

            // 负载长度
            byte payLen = SrcData[IndexOfStart + 11];
            if (payLen != 0x0E)
            {
                return -9;
            }

            // 数据类型
            byte dataType = SrcData[IndexOfStart + 12];
            if (dataType != 0x61)
            {
                return -10;
            }

            dataType = SrcData[IndexOfStart + 15];
            if (dataType != 0x63)
            {
                return -11;
            }

            dataType = SrcData[IndexOfStart + 17];
            if (dataType != 0x64)
            {
                return -12;
            }

            dataType = SrcData[IndexOfStart + 20];
            if (dataType != 0x65)
            {
                return -13;
            }

            dataType = SrcData[IndexOfStart + 23];
            if (dataType != 0x66)
            {
                return -14;
            }

            return 0;
        }

        /// <summary>
        /// 判断是不是监测工具监测到的M1发出的传感器数据包（V1版本）
        /// </summary>
        /// <param name="SrcBuf"></param>
        /// <param name="ExistRssi">true = 包含RSSI； false = 不包含RSSI；</param>
        /// <returns></returns>
        static public Int16 isSensorDataV1_MAX31855(byte[] SrcData, UInt16 IndexOfStart, bool ExistRssi)
        {
            UInt16 AppendLen = 0;
            if (ExistRssi == true)
            {
                AppendLen = 1;
            }

            // 数据包的总长度
            UInt16 SrcLen = (UInt16)(SrcData.Length - IndexOfStart);
            if (SrcLen < 30 + AppendLen)
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
            if (protocol != 1)
            {
                return -8;
            }

            // 负载长度
            byte payLen = SrcData[IndexOfStart + 11];
            if (payLen != 0x14)
            {
                return -9;
            }

            // 数据类型
            byte dataType = SrcData[IndexOfStart + 12];
            if (dataType != 0x61)
            {
                return -10;
            }

            dataType = SrcData[IndexOfStart + 15];
            if (dataType != 0x63)
            {
                return -11;
            }

            dataType = SrcData[IndexOfStart + 17];
            if (dataType != 0x64)
            {
                return -12;
            }

            dataType = SrcData[IndexOfStart + 20];
            if (dataType != 0x81)
            {
                return -13;
            }

            return 0;
        }

        /// <summary>
        /// 判断是不是监测工具监测到的M1发出的传感器数据包（V2和V3版本）
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
            if (SrcLen < 50 + AppendLen)
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
            if(pktLen + 5 + AppendLen > SrcLen)
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
            if (payLen != 17)
            {
                return -9;
            }

            // 数据类型
            byte dataType = SrcData[IndexOfStart + 27];
            if (dataType != 0x85)
            {
                return -10;
            }

            // 数据长度
            byte dataLen = SrcData[IndexOfStart + 28];
            if (dataLen != 15)
            {
                return -11;
            }

            return 0;
        }

        /// <summary>
        /// 读取Sensor的数据包（V1版本）
        /// </summary>
        /// <param name="SrcData"></param>
        /// <param name="IndexOfStart"></param>
        /// <param name="ExistRssi"></param>
        /// <returns></returns>
        public Int16 ReadSensorDataV1_MAX31855(byte[] SrcData, UInt16 IndexOfStart, bool ExistRssi)
        {
            // 数据包类型
            dataPktType = DataPktType.SensorDataMax31855Debug;

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

            // Serial
            SensorSN = SrcData[IndexOfStart + 13] * 256 + SrcData[IndexOfStart + 14];

            // IC temp
            ICTemperature = SrcData[IndexOfStart + 16];
            if (ICTemperature >= 128)
            {
                ICTemperature -= 256;
            }

            // voltage
            volt = (UInt16)(SrcData[IndexOfStart + 18] * 256 + SrcData[IndexOfStart + 19]);
            voltF = (double)(volt / 1000.0f);

            // MAX31855 采集结果
            SampleResult = SrcData[IndexOfStart + 21];
            if(SampleResult >= 0x80)
            {
                SampleResult -= 0x100;
            }

            // MAX31855 寄存器值
            RegValue = (UInt32)(SrcData[IndexOfStart + 22] * 256 * 256 * 256 + SrcData[IndexOfStart + 23] * 256 * 256 + SrcData[IndexOfStart + 24] * 256 + SrcData[IndexOfStart + 25]);

            // MAX31855 热电偶温度
            thermocoupleTemp = (Int16)(SrcData[IndexOfStart + 26] * 256 + SrcData[IndexOfStart + 27]);
            thermocoupleTempF = (double)(thermocoupleTemp / 100.0f);

            // MAX31855 芯片温度
            internalTemp = (Int16)(SrcData[IndexOfStart + 28] * 256 + SrcData[IndexOfStart + 29]);
            internalTempF = (double)(internalTemp / 100.0f);

            // MAX31855 被测环境温度
            temp = (Int16)(SrcData[IndexOfStart + 30] * 256 + SrcData[IndexOfStart + 31]);
            tempF = (double)(temp / 100.0f);

            // RSSI
            if (ExistRssi == true)
            {
                byte rssi = SrcData[IndexOfStart + 36];
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

        /// <summary>
        /// 读取Sensor的数据包（V2和V3版本）
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

            // 标识符、温度、湿度
            ifg = SrcData[IndexOfStart + 29];
            temp = (Int16)(SrcData[IndexOfStart + 30] * 256 + SrcData[IndexOfStart + 31]);
            tempF = (double)(temp / 100.0f);
            hum = (UInt16)(SrcData[IndexOfStart + 32] * 256 + SrcData[IndexOfStart + 33]);
            humF = (double)(hum / 100.0f);

            ifg1 = SrcData[IndexOfStart + 34];
            temp1 = (Int16)(SrcData[IndexOfStart + 35] * 256 + SrcData[IndexOfStart + 36]);
            temp1F = (double)(temp1 / 100.0f);
            hum1 = (UInt16)(SrcData[IndexOfStart + 37] * 256 + SrcData[IndexOfStart + 38]);
            hum1F = (double)(hum1 / 100.0f);

            ifg2 = SrcData[IndexOfStart + 39];
            temp2 = (Int16)(SrcData[IndexOfStart + 40] * 256 + SrcData[IndexOfStart + 41]);
            temp2F = (double)(temp2 / 100.0f);
            hum2 = (UInt16)(SrcData[IndexOfStart + 42] * 256 + SrcData[IndexOfStart + 43]);
            hum2F = (double)(hum2 / 100.0f);

            // To Send Ram
            ToSendRam = SrcData[IndexOfStart + 44];

            // To Send Flash
            ToSendFlash = (UInt16)(SrcData[IndexOfStart + 45] * 256 + SrcData[IndexOfStart + 45]);

            // RSSI
            if (ExistRssi == true)
            {
                byte rssi = SrcData[IndexOfStart + 50];
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

        /// <summary>
        /// 无线监测工具直接监测M1发出的上电自检数据包，数据包的尾部带有RSSI值
        /// </summary>
        /// <param name="SrcData"></param>
        /// <returns></returns>
        private Int16 M1_isSelfTestPktV3(byte[] SrcBuf)
        {
            // 数据包的总长度
            UInt16 SrcLen = (UInt16)SrcBuf.Length;
            if (SrcLen < 89)     // 包含了RSSI值
            {
                return -1;
            }

            // 起始位
            if (SrcBuf[0] != 0xEA)
            {
                return -2;
            }

            // 长度位
            byte pktLen = SrcBuf[1];
            if (SrcBuf[2 + pktLen + 2] != 0xAE)
            {
                return -3;
            }

            // CRC16

            // 命令位
            if (SrcBuf[2] != 0)
            {
                return -5;
            }

            // 协议版本
            if (SrcBuf[4] != 3)
            {
                return -6;
            }

            // 起始位
            STP = SrcBuf[0];

            // Cmd
            Pattern = SrcBuf[2];

            // Device type
            SetDeviceName(SrcBuf[3]);

            // protocol
            ProtocolVersion = SrcBuf[4];

            // Primary DeviceMacV
            SetDevicePrimaryMac(SrcBuf, 5);

            // SS ID
            SetDeviceMac(SrcBuf, 9);

            // Hardware Revision
            HwRevisionV = (UInt32)(SrcBuf[13] * 256 * 256 * 256 + SrcBuf[14] * 256 * 256 + SrcBuf[15] * 256 + SrcBuf[16]);

            // Software Revision
            SwRevisionV = (UInt16)(SrcBuf[17] * 256 + SrcBuf[18]);

            // 客户码
            SetDeviceCustomer(SrcBuf, 19);

            // debug
            DebugV = (UInt16)(SrcBuf[21] * 256 + SrcBuf[22]);

            // category
            Category = SrcBuf[23];

            // 采集间隔
            Interval = (UInt16)(SrcBuf[24] * 256 + SrcBuf[25]);

            // Calendar
            CurrentT = CommArithmetic.DecodeDateTime(SrcBuf, 26);

            // pattern
            Pattern = SrcBuf[32];

            // bps
            Bps = SrcBuf[33];

            // TxPower
            SetTxPower(SrcBuf[34]);

            // 发/采
            SampleSend = SrcBuf[35];

            // channel
            Channel = SrcBuf[36];

            // 温度预警上限

            // 温度预警下限

            // 湿度预警上限

            // 湿度预警下限

            // 温度阈值上限

            // 温度阈值下限

            // 湿度阈值上限

            // 湿度阈值下限

            // 温度补偿

            // 湿度补偿

            // 片内温度

            // 电池电压

            // flash device ID

            // 存储容量

            // front

            // rear

            // queue length

            // 温度

            // 湿度

            // 正常发送间隔

            // 预警发送间隔

            // 报警发送间隔

            // BVN

            // Reset Source

            // Reserved

            // 源数据
            this.SourceData = CommArithmetic.ToHexString(SrcBuf);

            return 0;
        }

        public byte[] UpdateFactory()
        {
            byte[] updateBytes = new byte[21];
            updateBytes[0] = 0xCE;
            updateBytes[1] = 0x10;
            updateBytes[2] = 0xA1;
            updateBytes[3] = 0x01;

            //兼容M1 和 M1P
            if (DeviceTypeS == "51")
            {
                updateBytes[4] = 0x51;
            }
            else if (DeviceTypeS == "53")
            {
                updateBytes[4] = 0x53;
            }
            else if (DeviceTypeS == "57")
            {
                updateBytes[4] = 0x57;
            }

            updateBytes[5] = 0x02;

            byte[] deviceMacBytes = CommArithmetic.HexStringToByteArray(DeviceMacS);
            updateBytes[6] = deviceMacBytes[0];
            updateBytes[7] = deviceMacBytes[1];
            updateBytes[8] = deviceMacBytes[2];
            updateBytes[9] = deviceMacBytes[3];

            deviceMacBytes = CommArithmetic.HexStringToByteArray(DeviceMacNewS);
            updateBytes[10] = deviceMacBytes[0];
            updateBytes[11] = deviceMacBytes[1];
            updateBytes[12] = deviceMacBytes[2];
            updateBytes[13] = deviceMacBytes[3];

            deviceMacBytes = CommArithmetic.HexStringToByteArray(HwRevisionS);
            updateBytes[14] = deviceMacBytes[0];
            updateBytes[15] = deviceMacBytes[1];
            updateBytes[16] = deviceMacBytes[2];
            updateBytes[17] = deviceMacBytes[3];

            updateBytes[18] = 0x00;
            updateBytes[19] = 0x00;
            updateBytes[20] = 0xEC;

            return updateBytes;
        }

        public byte[] UpdateUserConfig()
        {
            byte[] updateBytes = new byte[27];
            updateBytes[0] = 0xCE;
            if (DeviceTypeS == "57")
            {
                updateBytes[1] = 0x14;
            }                
            else
            {
                updateBytes[1] = 0x16;
            }
            
            updateBytes[2] = 0xA2;
            updateBytes[3] = 0x01;
            //兼容M1 和 M1P
            if (DeviceTypeS == "51")
                updateBytes[4] = 0x51;
            else if (DeviceTypeS == "53")
                updateBytes[4] = 0x53;
            else if (DeviceTypeS == "57")
                updateBytes[4] = 0x57;

            updateBytes[5] = 0x02;
            //DeviceMacV
            byte[] deviceMacBytes = CommArithmetic.HexStringToByteArray(DeviceMacS);
            updateBytes[6] = deviceMacBytes[0];
            updateBytes[7] = deviceMacBytes[1];
            updateBytes[8] = deviceMacBytes[2];
            updateBytes[9] = deviceMacBytes[3];
            //clientID
            deviceMacBytes = CommArithmetic.HexStringToByteArray(CustomerS);
            updateBytes[10] = deviceMacBytes[0];
            updateBytes[11] = deviceMacBytes[1];

            //Debug      
            updateBytes[12] = (byte) (DebugV / 256);
            updateBytes[13] = (byte) (DebugV % 256);

            //category
            updateBytes[14] = Category;
            //Pattern
            updateBytes[15] = Pattern;
            //Bps
            updateBytes[16] = Bps;
            //TxPower
            updateBytes[17] = (byte)TxPower;
            //Channel
            updateBytes[18] = Channel;

            //温度补偿：TODO 暂未实现
            updateBytes[19] = 0x00;
            updateBytes[20] = 0x00;

            if (DeviceTypeS == "57")
            {
                //湿度补偿：TODO 暂未实现,M2 没有湿度补偿               

                updateBytes[21] = MaxLength;

                //CRC：TODO 暂未实现
                updateBytes[22] = 0x00;
                updateBytes[23] = 0x00;

                updateBytes[24] = 0xEC;
            }
            else
            {
                updateBytes[21] = 0x00;
                updateBytes[22] = 0x00;

                updateBytes[23] = MaxLength;

                //CRC：TODO 暂未实现
                updateBytes[24] = 0x00;
                updateBytes[25] = 0x00;

                updateBytes[26] = 0xEC;
            }

            return updateBytes;
        }

        public byte[] UpdateApplicationConfig()
        {
            byte[] updateBytes = new byte[38];
            updateBytes[0] = 0xCE;
            if (DeviceTypeS != "57")
            {
                updateBytes[1] = 0x21;
            }
            else
            {
                updateBytes[1] = 0x19;

            }
                
            updateBytes[2] = 0xA3;
            updateBytes[3] = 0x01;
            //兼容M1 和 M1P
            if (DeviceTypeS == "51")
                updateBytes[4] = 0x51;
            else if (DeviceTypeS == "53")
                updateBytes[4] = 0x53;
            else if (DeviceTypeS == "57")
                updateBytes[4] = 0x57;

            updateBytes[5] = 0x02;
            //DeviceMacV
            byte[] deviceMacBytes = CommArithmetic.HexStringToByteArray(DeviceMacS);
            updateBytes[6] = deviceMacBytes[0];
            updateBytes[7] = deviceMacBytes[1];
            updateBytes[8] = deviceMacBytes[2];
            updateBytes[9] = deviceMacBytes[3];
            //Interval
            deviceMacBytes = CommArithmetic.Int16_2Bytes(Interval);
            updateBytes[10] = deviceMacBytes[0];
            updateBytes[11] = deviceMacBytes[1];

            //Calendar
            deviceMacBytes = CommArithmetic.EncodeDateTime(Calendar);
            updateBytes[12] = deviceMacBytes[0];
            updateBytes[13] = deviceMacBytes[1];
            updateBytes[14] = deviceMacBytes[2];
            updateBytes[15] = deviceMacBytes[3];
            updateBytes[16] = deviceMacBytes[4];
            updateBytes[17] = deviceMacBytes[5];

            updateBytes[18] = SampleSend;

            if (DeviceTypeS == "57")
            {
                deviceMacBytes = CommArithmetic.Double_2Bytes(TemperatureInfoHigh);
                updateBytes[19] = deviceMacBytes[0];
                updateBytes[20] = deviceMacBytes[1];

                deviceMacBytes = CommArithmetic.Double_2Bytes(TemperatureInfoLow);
                updateBytes[21] = deviceMacBytes[0];
                updateBytes[22] = deviceMacBytes[1];

               

                deviceMacBytes = CommArithmetic.Double_2Bytes(TemperatureWarnHigh);
                updateBytes[23] = deviceMacBytes[0];
                updateBytes[24] = deviceMacBytes[1];


                deviceMacBytes = CommArithmetic.Double_2Bytes(TemperatureWarnLow);
                updateBytes[25] = deviceMacBytes[0];
                updateBytes[26] = deviceMacBytes[1];

               



                //CRC：TODO 暂未实现
                updateBytes[27] = 0x00;
                updateBytes[28] = 0x00;


                updateBytes[29] = 0xEC;

            }
            else
            {
                deviceMacBytes = CommArithmetic.Double_2Bytes(TemperatureInfoHigh);
                updateBytes[19] = deviceMacBytes[0];
                updateBytes[20] = deviceMacBytes[1];

                deviceMacBytes = CommArithmetic.Double_2Bytes(TemperatureInfoLow);
                updateBytes[21] = deviceMacBytes[0];
                updateBytes[22] = deviceMacBytes[1];

                deviceMacBytes = CommArithmetic.Double_2Bytes(HumidityInfoHigh);
                updateBytes[23] = deviceMacBytes[0];
                updateBytes[24] = deviceMacBytes[1];

                deviceMacBytes = CommArithmetic.Double_2Bytes(HumidityInfoLow);
                updateBytes[25] = deviceMacBytes[0];
                updateBytes[26] = deviceMacBytes[1];

                deviceMacBytes = CommArithmetic.Double_2Bytes(TemperatureWarnHigh);
                updateBytes[27] = deviceMacBytes[0];
                updateBytes[28] = deviceMacBytes[1];


                deviceMacBytes = CommArithmetic.Double_2Bytes(TemperatureWarnLow);
                updateBytes[29] = deviceMacBytes[0];
                updateBytes[30] = deviceMacBytes[1];

                deviceMacBytes = CommArithmetic.Double_2Bytes(HumidityWarnHigh);
                updateBytes[31] = deviceMacBytes[0];
                updateBytes[32] = deviceMacBytes[1];

                deviceMacBytes = CommArithmetic.Double_2Bytes(HumidityWarnLow);
                updateBytes[33] = deviceMacBytes[0];
                updateBytes[34] = deviceMacBytes[1];



                //CRC：TODO 暂未实现
                updateBytes[35] = 0x00;
                updateBytes[36] = 0x00;


                updateBytes[37] = 0xEC;

            }
            

            //updateBytes[0] = 0xCE;




            return updateBytes;
        }

        public byte[] DeleteData()
        {
            byte[] updateBytes = new byte[19];
            updateBytes[0] = 0xCE;
            updateBytes[1] = 0x0E;
            updateBytes[2] = 0xA4;
            updateBytes[3] = 0x01;
            //兼容M1 和 M1P
            if (DeviceTypeS == "51")
                updateBytes[4] = 0x51;
            else if (DeviceTypeS == "53")
                updateBytes[4] = 0x53;
            else if (DeviceTypeS == "57")
                updateBytes[4] = 0x57;

            updateBytes[5] = 0x02;

            byte[] deviceMacBytes = CommArithmetic.HexStringToByteArray(DeviceMacS);
            updateBytes[6] = deviceMacBytes[0];
            updateBytes[7] = deviceMacBytes[1];
            updateBytes[8] = deviceMacBytes[2];
            updateBytes[9] = deviceMacBytes[3];

           
            updateBytes[18] = 0xEC;

            return updateBytes;
        }
    }
}
