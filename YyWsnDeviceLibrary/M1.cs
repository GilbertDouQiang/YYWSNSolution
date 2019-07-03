using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace YyWsnDeviceLibrary
{
    public class M1:Sensor
    {
        /**************************************
         * 属性
         * ************************************/
        
        /// <summary>
        /// 数据包类型
        /// </summary>
        private DataPktType dataPktType { get; set; }       // 外部只读

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

        /**************************************
         * 方法
         * ************************************/

        /// <summary>
        /// 获取设备类型编码
        /// </summary>
        /// <returns></returns>
        static public byte GetDeviceType()
        {
            return (byte)DeviceType.M1;
        }

        /// <summary>
        /// 判断输入的设备类型是否与本设备类型一致
        /// </summary>
        /// <param name="iDeviceType"></param>
        /// <returns></returns>
        static public bool isDeviceType(byte iDeviceType)
        {
            if (iDeviceType == (byte)DeviceType.M1 || iDeviceType == (byte)DeviceType.S1P || iDeviceType == (byte)DeviceType.M1_NTC || iDeviceType == (byte)DeviceType.M1_Beetech || iDeviceType == (byte)DeviceType.M6 || iDeviceType == (byte)DeviceType.M2_SHT30 || iDeviceType == (byte)DeviceType.M30)
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

        public M1() { }

        public M1(byte[] SrcData)
        {
            //2017版协议 v3.5
            //判断第三位 ，01 代表从传感器发出的正常数据，长度不是固定值

            //上电自检数据
            if (SrcData.Length == 82)
            {
                SetDeviceName(SrcData[4]);
                ProtocolVersion = SrcData[5];
                SetDevicePrimaryMac(SrcData, 6);
                SetDeviceMac(SrcData, 10);
                HwVersionS = CommArithmetic.DecodeMAC(SrcData, 14);
                SwVersionS = CommArithmetic.DecodeClientID(SrcData, 18);
                CustomerS = CommArithmetic.DecodeClientID(SrcData, 20);
                DebugV = (UInt16)(SrcData[22] * 256 + SrcData[23]);
                Category = SrcData[24];
                Interval = SrcData[25] * 256 + SrcData[26];
                Calendar = CommArithmetic.DecodeDateTime(SrcData, 27);

                WorkFunction = SrcData[33];
                SymbolRate = SrcData[34];
                TxPower = SrcData[35];
                SampleSend = SrcData[36];
                Channel = SrcData[37];

                TemperatureInfoHigh = CommArithmetic.DecodeTemperature(SrcData, 38);
                TemperatureInfoLow = CommArithmetic.DecodeTemperature(SrcData, 40);

                HumidityInfoHigh = CommArithmetic.DecodeHumidity(SrcData, 42);
                HumidityInfoLow = CommArithmetic.DecodeHumidity(SrcData, 44);

                TemperatureWarnHigh = CommArithmetic.DecodeTemperature(SrcData, 46);
                TemperatureWarnLow = CommArithmetic.DecodeTemperature(SrcData, 48);

                HumidityWarnHigh = CommArithmetic.DecodeHumidity(SrcData, 50);
                HumidityWarnLow = CommArithmetic.DecodeHumidity(SrcData, 52);

                TemperatureCompensation = CommArithmetic.DecodeTemperature(SrcData, 54);
                HumidityCompensation = CommArithmetic.DecodeHumidity(SrcData, 56);

                ICTemperature = SrcData[58];
                volt = (UInt16)(SrcData[59] * 256 + SrcData[60]);
                voltF = (double)(volt / 1000.0f);

                MaxLength = SrcData[63];

                temp = (Int16)(SrcData[73] * 256 + SrcData[74]);
                tempF = (double)(temp / 100.0f);

                hum = (UInt16)(SrcData[75] * 256 + SrcData[76]);
                humF = (double)(hum / 100.0f);

                RSSI = SrcData[78] - 256;

                //Falsh
                FlashID = CommArithmetic.DecodeClientID(SrcData, 61);
                FlashFront = SrcData[64] * 256 * 256 + SrcData[65] * 256 + SrcData[66];
                FlashRear = SrcData[67] * 256 * 256 + SrcData[68] * 256 + SrcData[69];
                FlashQueueLength = SrcData[70] * 256 * 256 + SrcData[71] * 256 + SrcData[72];

                return;
            }

            if (SrcData[0] == 0xEA && SrcData[1] == 0x4B && (SrcData[3] == 0x51 || SrcData[3] == 0x5C || SrcData[3] == 0x5D))
            {

                // Cmd
                WorkFunction = SrcData[2];

                // Device type     
                SetDeviceName(SrcData[3]);

                // protocol
                ProtocolVersion = SrcData[4];

                //PrimaryMacS
                SetDevicePrimaryMac(SrcData, 5);

                // Sensor ID
                SetDeviceMac(SrcData, 9);

                //硬件版本
                HwVersionS = CommArithmetic.DecodeHardwareVersion(SrcData, 13);

                SwVersionS = CommArithmetic.DecodeSoftwareVersion(SrcData, 17);
                // CustomerV
                CustomerS = CommArithmetic.DecodeClientID(SrcData, 19);

                //Debug
                DebugV = (UInt16)(SrcData[21] * 256 + SrcData[22]);

                //category
                Category = SrcData[23];

                //interval
                Interval = SrcData[24] * 256 + SrcData[25];

                //SS的日期和时间
                SensorCollectTime = CommArithmetic.DecodeDateTime(SrcData, 26);

                //pattern
                Pattern = SrcData[32];

                //bps
                Bps = SrcData[33];

                //TxPower
                TxPower = SrcData[34];
                //
                SampleSend = SrcData[35];
                //
                Channel = SrcData[36];

                //温度警戒上限/下限
                TemperatureInfoHigh = CommArithmetic.DecodeTemperature(SrcData, 37);
                TemperatureInfoLow = CommArithmetic.DecodeTemperature(SrcData, 39);

                //湿度警戒上限/下限
                HumidityInfoHigh = CommArithmetic.DecodeHumidity(SrcData, 41);
                HumidityInfoLow = CommArithmetic.DecodeHumidity(SrcData, 43);

                TemperatureWarnHigh = CommArithmetic.DecodeTemperature(SrcData, 45);
                TemperatureWarnLow = CommArithmetic.DecodeTemperature(SrcData, 47);

                HumidityWarnHigh = CommArithmetic.DecodeHumidity(SrcData, 49);
                HumidityWarnLow = CommArithmetic.DecodeHumidity(SrcData, 51);

                TemperatureCompensation = CommArithmetic.DecodeTemperature(SrcData, 53);
                HumidityCompensation = CommArithmetic.DecodeHumidity(SrcData, 55);

                // IC temp
                ICTemperature = SrcData[57];
                if (ICTemperature >= 128)
                {
                    ICTemperature -= 256;
                }

                // voltage
                volt = (UInt16)(SrcData[58] * 256 + SrcData[59]);
                voltF = (double)(volt / 1000.0f);

                FlashID = CommArithmetic.DecodeFlashID(SrcData, 60);

                MaxLength = SrcData[62];

                FlashFront = SrcData[63] * 256 * 256 + SrcData[64] * 256 + SrcData[65];
                FlashRear = SrcData[66] * 256 * 256 + SrcData[67] * 256 + SrcData[68];
                FlashQueueLength = SrcData[69] * 256 * 256 + SrcData[70] * 256 + SrcData[71];

                // temp
                temp = (Int16)(SrcData[72] * 256 + SrcData[73]);
                tempF = (double)(temp / 100.0f);

                // hum
                hum = (UInt16)(SrcData[74] * 256 + SrcData[75]);
                humF = (double)(hum / 100.0f);

                // 传输时间                
                SensorTransforTime = System.DateTime.Now;

                //可能收到没有RSSI的数据
                if (SrcData.Length >= SrcData[1] + 6)
                {
                    RSSI = SrcData[39] - 256;
                }

                this.SourceData = CommArithmetic.ToHexString(SrcData);

                return;
            }

            if (SrcData[0] == 0xAE && (SrcData[1] == 0x0E || SrcData[1] == 0x08) && (SrcData[3] == 0x51 || SrcData[3] == 0x5C || SrcData[3] == 0x5D))
            {
                UInt16 pktLen = SrcData[1];

                // Cmd
                WorkFunction = SrcData[2];

                // Device type     
                SetDeviceName(SrcData[3]);

                // protocol
                ProtocolVersion = SrcData[4];

                // CustomerV
                CustomerS = "00 00";

                //SS ID
                byte[] Mac = new byte[4];
                Mac[0] = 0x00;
                Mac[1] = 0x00;
                Mac[2] = SrcData[5];
                Mac[3] = SrcData[6];
                SetDeviceMac(Mac, 0);

                // Serial
                SensorSN = SrcData[7] * 256 + SrcData[8];

                //Error
                Error = SrcData[9];
                byte error = Error;
                if (error == 1)
                {   //GW的日期和时间
                    GWTime = CommArithmetic.DecodeDateTime(SrcData, 10);
                }
                else
                {
                    byte[] GwCalendar = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                    GWTime = CommArithmetic.DecodeDateTime(GwCalendar, 0);
                }

                // 系统时间
                SensorTransforTime = System.DateTime.Now;

                // RSSI
                RSSI = (double)(SrcData[2 + pktLen + 3] - 256);

                // 源数据
                this.SourceData = CommArithmetic.ToHexString(SrcData);

                return;
            }

            //兼容模式，兼容Z模式
            if (SrcData.Length == 28)
            {
                SetDeviceName(0x51);
                SetDeviceMac(SrcData, 5);

                CustomerS = CommArithmetic.DecodeClientID(SrcData, 3);

                WorkFunction = SrcData[2];

                ProtocolVersion = 0x00;

                SensorSN = SrcData[12] * 256 + SrcData[13];

                //传感器信息
                ICTemperature = 0; //老协议中没有IC 温度

                voltF = CommArithmetic.SHT20Voltage(SrcData[24], SrcData[25]);
                volt = (UInt16)(voltF * 1000.0f);

                tempF = CommArithmetic.SHT20Temperature(SrcData[16], SrcData[17]);
                temp = (Int16)(tempF * 100.0f);

                humF = CommArithmetic.SHT20Humidity(SrcData[20], SrcData[21]);
                hum = (UInt16)(humF * 100.0f);

                //广播模式，补充采集和传输时间
                SensorCollectTime = System.DateTime.Now;
                SensorTransforTime = System.DateTime.Now;
                //RSSI = SrcData[30] / 2 - 138; //1101 方案
                //可能收到没有RSSI的数据
                if (SrcData.Length == 28)
                {
                    RSSI = SrcData[27] - 256;
                }
                this.SourceData = CommArithmetic.ToHexString(SrcData);

                return;
            }


            //模式1 正常传输的数据，
            if (SrcData.Length == 31)
            {
                //将收到的数据填充到属性
                SetDeviceName(SrcData[3]);
                SetDeviceMac(SrcData, 7);

                CustomerS = CommArithmetic.DecodeClientID(SrcData, 5);
                WorkFunction = SrcData[2];
                ProtocolVersion = SrcData[4];

                SensorSN = SrcData[13] * 256 + SrcData[14];
                //传感器信息
                ICTemperature = SrcData[16]; //todo : 有符号整形数
                if (ICTemperature >= 128)
                {
                    ICTemperature -= 256;
                }

                volt = (UInt16)(SrcData[18] * 256 + SrcData[19]);
                voltF = (double)(volt / 1000.0f);

                temp = (Int16)(SrcData[21] * 256 + SrcData[22]);
                tempF = (double)(temp / 100.0f);

                hum = (UInt16)(SrcData[24] * 256 + SrcData[25]);
                humF = (double)(hum / 100.0f);

                //广播模式，补充采集和传输时间
                SensorCollectTime = System.DateTime.Now;
                SensorTransforTime = System.DateTime.Now;
                //RSSI = SrcData[30] / 2 - 138; //1101 方案
                //可能收到没有RSSI的数据
                if (SrcData.Length == 31)
                {
                    RSSI = SrcData[30] - 256;
                }

                this.SourceData = CommArithmetic.ToHexString(SrcData);

                return;
            }

            if (M1_isSensorDataV3(SrcData) >= 0)
            {
                return;
            }

            // 无线监测工具直接监测M1发出的上电自检数据包，数据包的尾部带有RSSI值
            if (M1_isSelfTestPktV3(SrcData) >= 0)
            {
                return;
            }

            return;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="SrcData"></param>
        public M1(byte[] SrcData, UInt16 IndexOfStart)
        {
            if (isSensorDataV3(SrcData, IndexOfStart, true) >= 0)
            {
                ReadSensorDataV3(SrcData, IndexOfStart, true);
            }
            else if (isSensorDataV1(SrcData, IndexOfStart, true) >= 0)
            {
                ReadSensorDataV1(SrcData, IndexOfStart, true);
            }

            return;
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
            if (payLen != 6)
            {
                return -9;
            }

            // 数据类型
            byte dataType = SrcData[IndexOfStart + 27];
            if (dataType != 0x65)
            {
                return -10;
            }

            dataType = SrcData[IndexOfStart + 30];
            if (dataType != 0x66)
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
        public Int16 ReadSensorDataV1(byte[] SrcData, UInt16 IndexOfStart, bool ExistRssi)
        {
            // 数据包类型
            dataPktType = DataPktType.SensorFromSsToGw;

            // 起始位
            STP = SrcData[IndexOfStart + 0];

            // Cmd
            WorkFunction = SrcData[IndexOfStart + 2];

            // Device Type
            SetDeviceName(SrcData[IndexOfStart + 3]);

            // protocol
            ProtocolVersion = SrcData[IndexOfStart + 4];

            // CustomerV
            SetDeviceCustomer(SrcData, (UInt16)(IndexOfStart + 5));

            // Sensor ID
            SetDeviceMac(SrcData, (UInt16)(IndexOfStart + 7));

            // Last/History
            LastHistory = 0x00;

            //状态state
            State = 0x00;

            //报警项
            AlarmItem = 0x00;

            // Serial
            SensorSN = SrcData[IndexOfStart + 13] * 256 + SrcData[IndexOfStart + 14];

            // Sample Calendar
            SensorCollectTime = System.DateTime.Now;

            // IC temp
            ICTemperature = SrcData[IndexOfStart + 16];
            if (ICTemperature >= 128)
            {
                ICTemperature -= 256;
            }

            // voltage
            volt = (UInt16)(SrcData[IndexOfStart + 18] * 256 + SrcData[IndexOfStart + 19]);
            voltF = (double)(volt / 1000.0f);

            // AltSerial
            AltSerial = 0x00;

            // 温湿度传感数据
            temp = (Int16)(SrcData[IndexOfStart + 21] * 256 + SrcData[IndexOfStart + 22]);
            tempF = (double)(temp / 100.0f);

            hum = (UInt16)(SrcData[IndexOfStart + 24] * 256 + SrcData[IndexOfStart + 25]);
            humF = (double)(hum / 100.0f);

            // To Send Ram
            ToSendRam = SrcData[IndexOfStart + 26];

            // To Send Flash
            ToSendFlash = 0;

            // RSSI
            if (ExistRssi == true)
            {
                byte rssi = SrcData[IndexOfStart + 30];
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
            WorkFunction = SrcData[IndexOfStart + 2];

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

            // 温湿度传感数据
            temp = (Int16)(SrcData[IndexOfStart + 28] * 256 + SrcData[IndexOfStart + 29]);
            tempF = (double)(temp / 100.0f);

            hum = (UInt16)(SrcData[IndexOfStart + 31] * 256 + SrcData[IndexOfStart + 32]);
            humF = (double)(hum / 100.0f);

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

        /// <summary>
        /// 处理监测工具监测到的M1发出的传感器数据包（V3版本）
        /// </summary>
        /// <param name="SrcBuf"></param>
        /// <returns></returns>
        private Int16 M1_isSensorDataV3(byte[] SrcData)
        {
            // 数据包的总长度
            UInt16 SrcLen = (UInt16)SrcData.Length;
            if (SrcLen < 40)     // 包含了RSSI值
            {
                return -1;
            }

            // 起始位
            if (SrcData[0] != 0xEA)
            {
                return -2;
            }

            // 长度位
            byte pktLen = SrcData[1];
            if (SrcData[2 + pktLen + 2] != 0xAE)
            {
                return -3;
            }

            // CRC16

            // 协议版本
            byte protocol = SrcData[4];
            if (protocol <= 0 || protocol > 3)
            {
                return -6;
            }

            // Cmd
            if (SrcData[2] != 1 && SrcData[2] != 2 && SrcData[2] != 3 && SrcData[2] != 4)
            {
                return -7;
            }

            // 起始位
            STP = SrcData[0];

            // Cmd
            WorkFunction = SrcData[2];

            // Device Type
            SetDeviceName(SrcData[3]);

            // protocol
            ProtocolVersion = protocol;

            // CustomerV
            SetDeviceCustomer(SrcData, 5);

            // Sensor ID
            SetDeviceMac(SrcData, 7);

            UInt16 iCnt = 0;

            if (protocol == 1)
            {

            }
            else if (protocol == 2)
            {

            }
            else if (protocol == 3)
            {
                byte payLen = SrcData[26];
                if (payLen != pktLen - 28)
                {
                    return -8;
                }

                iCnt = 11;

                // Last/History
                LastHistory = CommArithmetic.DecodeLastHistory(SrcData, iCnt++);

                //状态state
                State = SrcData[iCnt++];

                //报警项
                AlarmItem = SrcData[iCnt++];

                // Serial
                SensorSN = SrcData[iCnt] * 256 + SrcData[iCnt + 1];
                iCnt = (byte)(iCnt + 2);

                // Sample Calendar
                SensorCollectTime = CommArithmetic.DecodeDateTime(SrcData, iCnt);
                iCnt = (byte)(iCnt + 6);

                // IC temp
                ICTemperature = SrcData[iCnt++];
                if (ICTemperature >= 128)
                {
                    ICTemperature -= 256;
                }

                // voltage
                volt = (UInt16)(SrcData[iCnt] * 256 + SrcData[iCnt + 1]);
                voltF = (double)(volt / 1000.0f);
                iCnt = (byte)(iCnt + 2);

                // AltSerial
                AltSerial = SrcData[iCnt++];

                // payLen
                iCnt++;

                // Payload
                Int16 errCnt = HandlePayLoad(SrcData, iCnt, payLen);
                if (errCnt < 0)
                {
                    return -9;
                }
                iCnt = (byte)(iCnt + errCnt);

                // To Send Ram
                ToSendRam = SrcData[iCnt++];

                // To Send Flash
                ToSendFlash = (UInt16)(SrcData[iCnt] * 256 + SrcData[iCnt + 1]);
                iCnt = (byte)(iCnt + 2);

                // CRC, End
                iCnt = (byte)(iCnt + 2 + 1);

                // RSSI
                byte rssi = SrcData[iCnt++];
                if (rssi >= 0x80)
                {
                    RSSI = (double)(rssi - 0x100);
                }
                else
                {
                    RSSI = (double)rssi;
                }
            }
            else
            {

            }

            // 传输时间                
            SensorTransforTime = System.DateTime.Now;

            this.SourceData = CommArithmetic.ToHexString(SrcData);

            return 0;
        }

        private Int16 HandlePayLoad(byte[] SrcBuf, UInt16 StartBase, byte PayLen)
        {
            byte iCnt = 0;

            byte DataType = 0;
            byte DataLen = 0;

            while (iCnt < PayLen)
            {
                DataType = SrcBuf[StartBase + iCnt];
                switch (DataType)
                {
                    case 0x61:      // 采集序列号
                        {
                            DataLen = 2;
                            if (iCnt + 1 + DataLen > PayLen)
                            {
                                return -2;
                            }

                            SensorSN = SrcBuf[StartBase + iCnt + 1] * 256 + SrcBuf[StartBase + iCnt + 2];

                            iCnt = (byte)(iCnt + 1 + DataLen);
                            break;
                        }
                    case 0x63:      // 片内温度    
                        {
                            DataLen = 1;
                            if (iCnt + 1 + DataLen > PayLen)
                            {
                                return -2;
                            }

                            ICTemperature = SrcBuf[StartBase + iCnt + 1];
                            if (ICTemperature >= 128)
                            {
                                ICTemperature -= 256;
                            }

                            iCnt = (byte)(iCnt + 1 + DataLen);
                            break;
                        }
                    case 0x64:      // 电池电压  
                        {
                            DataLen = 2;
                            if (iCnt + 1 + DataLen > PayLen)
                            {
                                return -2;
                            }

                            volt = (UInt16)(SrcBuf[StartBase + iCnt + 1] * 256 + SrcBuf[StartBase + iCnt + 2]);
                            voltF = (double)(volt / 1000.0f);

                            iCnt = (byte)(iCnt + 1 + DataLen);
                            break;
                        }
                    case 0x65:      // 温度
                        {
                            DataLen = 2;
                            if (iCnt + 1 + DataLen > PayLen)
                            {
                                return -2;
                            }

                            temp = (Int16)(SrcBuf[StartBase + iCnt + 1] * 256 + SrcBuf[StartBase + iCnt + 2]);
                            tempF = (double)(temp / 100.0f);

                            iCnt = (byte)(iCnt + 1 + DataLen);
                            break;
                        }
                    case 0x66:      // 湿度
                        {
                            DataLen = 2;
                            if (iCnt + 1 + DataLen > PayLen)
                            {
                                return -2;
                            }

                            hum = (UInt16)(SrcBuf[StartBase + iCnt + 1] * 256 + SrcBuf[StartBase + iCnt + 2]);
                            humF = (double)(hum / 100.0f);
                            
                            iCnt = (byte)(iCnt + 1 + DataLen);
                            break;
                        }
                    default:        // 未知的数据类型
                        {
                            return -1;
                        }
                }
            }

            return (Int16)iCnt;
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
            WorkFunction = SrcBuf[2];

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
            TxPower = SrcBuf[34];

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

            deviceMacBytes = CommArithmetic.HexStringToByteArray(DeviceNewMAC);
            updateBytes[10] = deviceMacBytes[0];
            updateBytes[11] = deviceMacBytes[1];
            updateBytes[12] = deviceMacBytes[2];
            updateBytes[13] = deviceMacBytes[3];

            deviceMacBytes = CommArithmetic.HexStringToByteArray(HwVersionS);
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
            //WorkFunction
            updateBytes[15] = WorkFunction;
            //SymbolRate
            updateBytes[16] = SymbolRate;
            //TxPower
            updateBytes[17] = TxPower;
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
