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
        /// 湿度数据按照温度解析
        /// </summary>
        public Int16 humTemp { get; set; }

        /// <summary>
        /// 湿度数据按照温度解析
        /// </summary>
        public double humTempF { get; set; }

        /// <summary>
        /// 温度预警上限
        /// </summary>
        public double TempWarnHigh { get; set; }

        /// <summary>
        /// 温度预警下限
        /// </summary>
        public double TempWarnLow { get; set; }

        /// <summary>
        /// 温度报警上限
        /// </summary>
        public double TempAlertHigh { get; set; }

        /// <summary>
        /// 温度报警下限
        /// </summary>
        public double TempAlertLow { get; set; }

        /// <summary>
        /// 湿度预警上限
        /// </summary>
        public double HumWarnHigh { get; set; }
        /// <summary>
        /// 湿度预警下限
        /// </summary>
        public double HumWarnLow { get; set; }

        /// <summary>
        /// 湿度报警上限
        /// </summary>
        public double HumAlertHigh { get; set; }

        /// <summary>
        /// 湿度报警下限
        /// </summary>
        public double HumAlertLow { get; set; }

        /// <summary>
        /// 温度补偿
        /// </summary>
        public double TempCompensation { get; set; }

        /// <summary>
        /// 湿度补偿
        /// </summary>
        public double HumCompensation { get; set; }

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

        /// <summary>
        /// 导出序列号
        /// </summary>
        public UInt32 ExportSerial { get; set; }

        /// <summary>
        /// 检查下发
        /// </summary>
        public byte CheckIssue { get; set; }

        /// <summary>
        /// 保留位
        /// </summary>
        public UInt16 ReservedUInt16 { get; set; }

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
            if (iDeviceType == (byte)DeviceType.M1 || iDeviceType == (byte)DeviceType.S1P || iDeviceType == (byte)DeviceType.M1_NTC || iDeviceType == (byte)DeviceType.M1_Beetech || iDeviceType == (byte)DeviceType.M6 || iDeviceType == (byte)DeviceType.M2_SHT30 || iDeviceType == (byte)DeviceType.M30 || iDeviceType == (byte)DeviceType.ZQM1 || iDeviceType == (byte)DeviceType.M10 || iDeviceType == (byte)DeviceType.M20 || iDeviceType == (byte)DeviceType.M1X || iDeviceType == (byte)DeviceType.EK_SHT30 || iDeviceType == (byte)DeviceType.M60_SHT30 || iDeviceType == (byte)DeviceType.M60_MAX31855 || iDeviceType == (byte)DeviceType.M70_SHT30 || iDeviceType == (byte)DeviceType.M70_MAX31855)
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

            if (SrcData[0] == 0xEA && SrcData[1] == 0x4B && (SrcData[3] == 0x51 || SrcData[3] == 0x5C || SrcData[3] == 0x5D))
            {
                // Cmd
                Pattern = SrcData[2];

                // Device type     
                SetDeviceName(SrcData[3]);

                // protocol
                ProtocolVersion = SrcData[4];

                //PrimaryMacS
                SetDevicePrimaryMac(SrcData, 5);

                // Sensor ID
                SetDeviceMac(SrcData, 9);

                //硬件版本
                SetHardwareRevision(SrcData, 13);
                
                // 软件版本
                SetSoftwareRevision(SrcData, 17);

                // CustomerV
                SetDeviceCustomer(SrcData, 19);

                //Debug
                SetDeviceDebug(SrcData, 21);

                //category
                Category = SrcData[23];

                //interval
                Interval = (UInt16)(SrcData[24] * 256 + SrcData[25]);

                //SS的日期和时间
                SensorCollectTime = CommArithmetic.DecodeDateTime(SrcData, 26);

                //pattern
                Pattern = SrcData[32];

                //bps
                Bps = SrcData[33];

                //TxPower
                SetTxPower(SrcData[34]);

                //
                SampleSend = SrcData[35];
                //
                Channel = SrcData[36];

                //温度警戒上限/下限
                TempWarnHigh = CommArithmetic.DecodeTemperature(SrcData, 37);
                TempWarnLow = CommArithmetic.DecodeTemperature(SrcData, 39);

                //湿度警戒上限/下限
                HumWarnHigh = CommArithmetic.DecodeHumidity(SrcData, 41);
                HumWarnLow = CommArithmetic.DecodeHumidity(SrcData, 43);

                TempAlertHigh = CommArithmetic.DecodeTemperature(SrcData, 45);
                TempAlertLow = CommArithmetic.DecodeTemperature(SrcData, 47);

                HumAlertHigh = CommArithmetic.DecodeHumidity(SrcData, 49);
                HumAlertLow = CommArithmetic.DecodeHumidity(SrcData, 51);

                TempCompensation = CommArithmetic.DecodeTemperature(SrcData, 53);
                HumCompensation = CommArithmetic.DecodeHumidity(SrcData, 55);

                // IC temp
                monTemp = SrcData[57];
                ICTemperature = monTemp;
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
                Pattern = SrcData[2];

                // Device type     
                SetDeviceName(SrcData[3]);

                // protocol
                ProtocolVersion = SrcData[4];

                // CustomerV
                CustomerV = 0;
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

                SetDeviceCustomer(SrcData, 3);

                Pattern = SrcData[2];

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
                SetDeviceCustomer(SrcData, 5);
                Pattern = SrcData[2];
                ProtocolVersion = SrcData[4];

                SensorSN = SrcData[13] * 256 + SrcData[14];
                //传感器信息
                monTemp = SrcData[16];
                ICTemperature = monTemp;
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
            else if (isSensorDataV1_MAX31855(SrcData, IndexOfStart, true) >= 0)
            {
                ReadSensorDataV1_MAX31855(SrcData, IndexOfStart, true);
            }
            else if (isNtpPktV1(SrcData, IndexOfStart, true) >= 0)
            {
                ReadNtpPktV1(SrcData, IndexOfStart, true);
            }
            else if (isNtpRespondPktV1(SrcData, IndexOfStart, true) >= 0)
            {
                ReadNtpRespondPktV1(SrcData, IndexOfStart, true);
            }

            return;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="SrcData"></param>
        public M1(byte[] SrcData, UInt16 IndexOfStart, Device.DataPktType pktType, Device.DeviceType deviceType)
        {
            if (isDeviceType((byte)deviceType) == false)
            {
                return;
            }

            if (pktType == Device.DataPktType.SensorDataFromGmToPc)
            {
                UInt16 iCnt = (UInt16)(IndexOfStart + 9);

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
                monTemp = SrcData[iCnt];
                ICTemperature = monTemp;
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

                iCnt += 1;

                // 温湿度传感数据
                if (deviceType == Device.DeviceType.M1)
                {
                    temp = (Int16)(SrcData[iCnt] * 256 + SrcData[iCnt + 1]);
                    tempF = (double)(temp / 100.0f);
                    iCnt += 2;

                    iCnt += 1;

                    hum = (UInt16)(SrcData[iCnt] * 256 + SrcData[iCnt + 1]);
                    humF = (double)(hum / 100.0f);
                    iCnt += 2;
                }
                else    // Device.DeviceType.M2
                {
                    temp = (Int16)(SrcData[iCnt] * 256 + SrcData[iCnt + 1]);
                    tempF = (double)(temp / 100.0f);
                    iCnt += 2;

                    hum = 0;
                    humF = 0.0f;
                }

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
            else if (pktType == Device.DataPktType.SelfTestFromUsbToPc)
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
                    Interval = (UInt16)(SrcData[IndexOfStart + 25] * 256 + SrcData[IndexOfStart + 26]);
                    Calendar = CommArithmetic.DecodeDateTime(SrcData, (UInt16)(IndexOfStart + 27));

                    Pattern = SrcData[IndexOfStart + 33];
                    Bps = SrcData[IndexOfStart + 34];
                    SetTxPower(SrcData[IndexOfStart + 35]);
                    SampleSend = SrcData[IndexOfStart + 36];
                    Channel = SrcData[IndexOfStart + 37];

                    TempWarnHigh = (double)(Int16)(SrcData[IndexOfStart + 38] * 256 + SrcData[IndexOfStart + 39]) / 100.0f;
                    TempWarnLow = (double)(Int16)(SrcData[IndexOfStart + 40] * 256 + SrcData[IndexOfStart + 41]) / 100.0f;

                    HumWarnHigh = (double)(SrcData[IndexOfStart + 42] * 256 + SrcData[IndexOfStart + 43]) / 100.0f;
                    HumWarnLow = (double)(SrcData[IndexOfStart + 44] * 256 + SrcData[IndexOfStart + 45]) / 100.0f;

                    TempAlertHigh = (double)(Int16)(SrcData[IndexOfStart + 46] * 256 + SrcData[IndexOfStart + 47]) / 100.0f;
                    TempAlertLow = (double)(Int16)(SrcData[IndexOfStart + 48] * 256 + SrcData[IndexOfStart + 49]) / 100.0f;

                    HumAlertHigh = (double)(SrcData[IndexOfStart + 50] * 256 + SrcData[IndexOfStart + 51]) / 100.0f;
                    HumAlertLow = (double)(SrcData[IndexOfStart + 52] * 256 + SrcData[IndexOfStart + 53]) / 100.0f;

                    TempCompensation = (double)(Int16)(SrcData[IndexOfStart + 54] * 256 + SrcData[IndexOfStart + 55]) / 100.0f;
                    HumCompensation = (double)(Int16)(SrcData[IndexOfStart + 56] * 256 + SrcData[IndexOfStart + 57]) / 100.0f;

                    monTemp = SrcData[IndexOfStart + 58];
                    ICTemperature = monTemp;
                    voltF = Math.Round(Convert.ToDouble((SrcData[IndexOfStart + 59] * 256 + SrcData[IndexOfStart + 60])) / 1000, 2);

                    FlashID = CommArithmetic.DecodeClientID(SrcData, IndexOfStart + 61);
                    MaxLength = SrcData[IndexOfStart + 63];

                    FlashFront = SrcData[IndexOfStart + 64] * 256 * 256 + SrcData[IndexOfStart + 65] * 256 + SrcData[IndexOfStart + 66];
                    FlashRear = SrcData[IndexOfStart + 67] * 256 * 256 + SrcData[IndexOfStart + 68] * 256 + SrcData[IndexOfStart + 69];
                    FlashQueueLength = SrcData[IndexOfStart + 70] * 256 * 256 + SrcData[IndexOfStart + 71] * 256 + SrcData[IndexOfStart + 72];

                    temp = (Int16)(SrcData[IndexOfStart + 73] * 256 + SrcData[IndexOfStart + 74]);
                    tempF = (double)(temp / 100.0f);

                    hum = (UInt16)(SrcData[IndexOfStart + 75] * 256 + SrcData[IndexOfStart + 76]);
                    humF = (double)(hum / 100.0f);

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

                    TempWarnHigh = (double)(Int16)(SrcData[IndexOfStart + 38] * 256 + SrcData[IndexOfStart + 39]) / 100.0f;
                    TempWarnLow = (double)(Int16)(SrcData[IndexOfStart + 40] * 256 + SrcData[IndexOfStart + 41]) / 100.0f;

                    HumWarnHigh = (double)(SrcData[IndexOfStart + 42] * 256 + SrcData[IndexOfStart + 43]) / 100.0f;
                    HumWarnLow = (double)(SrcData[IndexOfStart + 44] * 256 + SrcData[IndexOfStart + 45]) / 100.0f;

                    TempAlertHigh = (double)(Int16)(SrcData[IndexOfStart + 46] * 256 + SrcData[IndexOfStart + 47]) / 100.0f;
                    TempAlertLow = (double)(Int16)(SrcData[IndexOfStart + 48] * 256 + SrcData[IndexOfStart + 49]) / 100.0f;

                    HumAlertHigh = (double)(SrcData[IndexOfStart + 50] * 256 + SrcData[IndexOfStart + 51]) / 100.0f;
                    HumAlertLow = (double)(SrcData[IndexOfStart + 52] * 256 + SrcData[IndexOfStart + 53]) / 100.0f;

                    TempCompensation = (double)(Int16)(SrcData[IndexOfStart + 54] * 256 + SrcData[IndexOfStart + 55]) / 100.0f;
                    HumCompensation = (double)(Int16)(SrcData[IndexOfStart + 56] * 256 + SrcData[IndexOfStart + 57]) / 100.0f;

                    ICTemperature = SrcData[IndexOfStart + 58];
                    voltF = Math.Round(Convert.ToDouble((SrcData[IndexOfStart + 59] * 256 + SrcData[IndexOfStart + 60])) / 1000, 2);

                    FlashID = CommArithmetic.DecodeClientID(SrcData, IndexOfStart + 61);
                    MaxLength = SrcData[IndexOfStart + 63];

                    FlashFront = SrcData[IndexOfStart + 64] * 256 * 256 + SrcData[IndexOfStart + 65] * 256 + SrcData[IndexOfStart + 66];
                    FlashRear = SrcData[IndexOfStart + 67] * 256 * 256 + SrcData[IndexOfStart + 68] * 256 + SrcData[IndexOfStart + 69];
                    FlashQueueLength = SrcData[IndexOfStart + 70] * 256 * 256 + SrcData[IndexOfStart + 71] * 256 + SrcData[IndexOfStart + 72];

                    temp = (Int16)(SrcData[IndexOfStart + 73] * 256 + SrcData[IndexOfStart + 74]);
                    tempF = (double)(temp / 100.0f);

                    hum = (UInt16)(SrcData[IndexOfStart + 75] * 256 + SrcData[IndexOfStart + 76]);
                    humF = (double)(hum / 100.0f);

                    NormalInterval = (UInt16)(SrcData[IndexOfStart + 77] * 256 + SrcData[IndexOfStart + 78]);
                    WarnInterval = (UInt16)(SrcData[IndexOfStart + 79] * 256 + SrcData[IndexOfStart + 80]);
                    AlertInterval = (UInt16)(SrcData[IndexOfStart + 81] * 256 + SrcData[IndexOfStart + 82]);

                    RstSrc = SrcData[IndexOfStart + 84];

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
            else if (pktType == Device.DataPktType.ExportFromM1Beetech)
            {
                UInt16 iCnt = (UInt16)(IndexOfStart + 12);

                // 导出序列号
                ExportSerial = (UInt32)(SrcData[iCnt] * 256 * 256 + SrcData[iCnt + 1] * 256 + SrcData[iCnt + 2]);
                iCnt += 3;

                // SendOK
                SendOK = SrcData[iCnt];
                iCnt += 1;

                //状态state
                State = SrcData[iCnt];
                iCnt += 1;

                //报警项
                AlarmItem = SrcData[iCnt];
                iCnt += 1;

                // 传输序列号
                SerialOfGw = (UInt16)(SrcData[iCnt] * 256 + SrcData[iCnt + 1]);
                iCnt += 2;

                // Sample Calendar
                SensorCollectTime = CommArithmetic.DecodeDateTime(SrcData, iCnt);
                iCnt += 6;

                // IC temp
                ICTemperature = SrcData[iCnt];
                if (ICTemperature >= 128)
                {
                    ICTemperature -= 256;
                }
                iCnt += 1;

                // voltage
                volt = (UInt16)(SrcData[iCnt] * 256 + SrcData[iCnt + 1]);
                voltF = (double)(volt / 1000.0f);
                iCnt += 2;

                // Serial
                SensorSN = SrcData[iCnt] * 256 + SrcData[iCnt + 1];
                iCnt += 2;

                // 温度
                temp = (Int16)(SrcData[iCnt] * 256 + SrcData[iCnt + 1]);
                tempF = (double)(temp / 100.0f);
                iCnt += 2;

                // 湿度
                hum = (UInt16)(SrcData[iCnt] * 256 + SrcData[iCnt + 1]);
                humF = (double)(hum / 100.0f);
                iCnt += 2;

                // 系统时间            
                SystemTime = System.DateTime.Now;

                // 源数据
                this.SourceData = CommArithmetic.ToHexString(SrcData, IndexOfStart, SrcData[IndexOfStart + 2] + 7);
            }

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
        /// 判断是不是监测工具监测到的Sensor发出的授时申请数据包
        /// </summary>
        /// <param name="SrcBuf"></param>
        /// <param name="ExistRssi">true = 包含RSSI； false = 不包含RSSI；</param>
        /// <returns></returns>
        static public Int16 isNtpPktV1(byte[] SrcData, UInt16 IndexOfStart, bool ExistRssi)
        {
            UInt16 AppendLen = 0;
            if (ExistRssi == true)
            {
                AppendLen = 1;
            }

            // 数据包的总长度
            UInt16 SrcLen = (UInt16)(SrcData.Length - IndexOfStart);
            if (SrcLen < 25 + AppendLen)
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
            if (Cmd != 0xA1)
            {
                return -6;
            }

            // DeviceType
            // 不判断设备类型

            // 协议版本
            byte protocol = SrcData[IndexOfStart + 4];
            if (protocol != 1)
            {
                return -8;
            }

            return 0;
        }

        /// <summary>
        /// 判断是不是监测工具监测到的Gateway发出的授时反馈数据包
        /// </summary>
        /// <param name="SrcBuf"></param>
        /// <param name="ExistRssi">true = 包含RSSI； false = 不包含RSSI；</param>
        /// <returns></returns>
        static public Int16 isNtpRespondPktV1(byte[] SrcData, UInt16 IndexOfStart, bool ExistRssi)
        {
            UInt16 AppendLen = 0;
            if (ExistRssi == true)
            {
                AppendLen = 1;
            }

            // 数据包的总长度
            UInt16 SrcLen = (UInt16)(SrcData.Length - IndexOfStart);
            if (SrcLen < 15 + AppendLen)
            {
                return -1;
            }

            // 起始位
            if (SrcData[IndexOfStart + 0] != 0xAE)
            {
                return -2;
            }

            // 长度位
            byte pktLen = SrcData[IndexOfStart + 1];
            if (pktLen + 5 + AppendLen > SrcLen)
            {
                return -3;
            }

            if (SrcData[IndexOfStart + 2 + pktLen + 2] != 0xEA)
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
            if (Cmd != 0xA1)
            {
                return -6;
            }

            // DeviceType
            // 不判断设备类型

            // 协议版本
            byte protocol = SrcData[IndexOfStart + 4];
            if (protocol != 1)
            {
                return -8;
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

            this.SourceData = CommArithmetic.ToHexString(SrcData, IndexOfStart, 31);

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

            this.SourceData = CommArithmetic.ToHexString(SrcData, IndexOfStart, 37);

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
            monTemp = SrcData[IndexOfStart + 22];
            ICTemperature = monTemp;
            if (ICTemperature >= 128)
            {
                ICTemperature -= 256;
            }

            // voltage
            UInt16 VoltValue = (UInt16)(SrcData[IndexOfStart + 23] * 256 + SrcData[IndexOfStart + 24]);

            if (0 == (VoltValue & 0x8000))
            {
                LinkCharge = false;
            }
            else
            {
                LinkCharge = true;
                VoltValue = (UInt16)(VoltValue & 0x7FFF);
            }

            volt = (UInt16)(VoltValue);
            voltF = (double)(volt / 1000.0f);

            // AltSerial
            AltSerial = SrcData[IndexOfStart + 25];

            // 温湿度传感数据
            temp = (Int16)(SrcData[IndexOfStart + 28] * 256 + SrcData[IndexOfStart + 29]);
            tempF = (double)(temp / 100.0f);

            hum = (UInt16)(SrcData[IndexOfStart + 31] * 256 + SrcData[IndexOfStart + 32]);
            humF = (double)(hum / 100.0f);

            // 湿度数据按照温度解析
            humTemp = (Int16)hum;
            humTempF = (double)(humTemp / 100.0f);

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

            this.SourceData = CommArithmetic.ToHexString(SrcData, IndexOfStart, 40);

            return 0;
        }

        /// <summary>
        /// 读取Sensor的授时申请数据包
        /// </summary>
        /// <param name="SrcData"></param>
        /// <param name="IndexOfStart"></param>
        /// <param name="ExistRssi"></param>
        /// <returns></returns>
        public Int16 ReadNtpPktV1(byte[] SrcData, UInt16 IndexOfStart, bool ExistRssi)
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

            // Serial
            SensorSN = SrcData[IndexOfStart + 11] * 256 + SrcData[IndexOfStart + 12];

            // Sample Calendar
            SensorCollectTime = CommArithmetic.DecodeDateTime(SrcData, (UInt16)(IndexOfStart + 13));

            // 检查下发
            CheckIssue = SrcData[IndexOfStart + 19];

            // 保留
            ReservedUInt16 = (UInt16)(SrcData[IndexOfStart + 20] * 256 + SrcData[IndexOfStart + 21]);

            // RSSI
            if (ExistRssi == true)
            {
                byte rssi = SrcData[IndexOfStart + 25];
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

            this.SourceData = CommArithmetic.ToHexString(SrcData, IndexOfStart, 26);

            return 0;
        }

        /// <summary>
        /// 读取Sensor的授时反馈数据包
        /// </summary>
        /// <param name="SrcData"></param>
        /// <param name="IndexOfStart"></param>
        /// <param name="ExistRssi"></param>
        /// <returns></returns>
        public Int16 ReadNtpRespondPktV1(byte[] SrcData, UInt16 IndexOfStart, bool ExistRssi)
        {
            // 数据包类型
            dataPktType = DataPktType.SensorFromSsToGw;

            // 起始位
            STP = SrcData[IndexOfStart + 0];

            // 数据包长度
            byte pktLen = SrcData[IndexOfStart + 1];

            // Cmd
            Pattern = SrcData[IndexOfStart + 2];

            // Device Type
            SetDeviceName(SrcData[IndexOfStart + 3]);

            // Custom
            CustomerS = "0000";
            CustomerV = 0;

            // protocol
            ProtocolVersion = SrcData[IndexOfStart + 4];

            // Sensor ID
            SetDeviceMac(SrcData, (UInt16)(IndexOfStart + 5));

            // Serial
            SensorSN = SrcData[IndexOfStart + 9] * 256 + SrcData[IndexOfStart + 10];

            // Error
            Error = SrcData[IndexOfStart + 11];

            if (pktLen >= 16)
            {
                // Sample Calendar
                SensorCollectTime = CommArithmetic.DecodeDateTime(SrcData, (UInt16)(IndexOfStart + 12));
            }            

            // RSSI
            if (ExistRssi == true)
            {
                byte rssi = SrcData[IndexOfStart + 2 + pktLen + 3];
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

            this.SourceData = CommArithmetic.ToHexString(SrcData, IndexOfStart, pktLen + 6);

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
            Pattern = SrcData[2];

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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] UpdateFactory()
        {
            byte[] ByteBuf = new byte[21];
            UInt16 ByteLen = 0;

            // 起始位
            ByteBuf[ByteLen++] = 0xCE;

            // 长度位
            ByteBuf[ByteLen++] = (byte)(ByteBuf.Length - 5);

            // 命令位
            ByteBuf[ByteLen++] = 0xA1;

            // USB Protocol
            ByteBuf[ByteLen++] = 0x01;

            // 设备类型
            if (DeviceTypeS == "M1")
            {
                ByteBuf[ByteLen++] = 0x51;
            }
            else if (DeviceTypeS == "M1P")
            {
                ByteBuf[ByteLen++] = 0x53;
            }
            else if (DeviceTypeS == "M2")
            {
                ByteBuf[ByteLen++] = 0x57;
            }
            else if (DeviceTypeS == "M1NTC")
            {
                ByteBuf[ByteLen++] = 0x5C;
            }
            else if (DeviceTypeS == "M1_Beetech")
            { 
                ByteBuf[ByteLen++] = 0x5D;
            }
            else if (DeviceTypeS == "M1_ZheQin")
            {
                ByteBuf[ByteLen++] = 0x7D;
            }else
            {
                ByteBuf[ByteLen++] = 0x00;
            }

            // Protocol
            ByteBuf[ByteLen++] = 0x02;

            // Sensor Mac
            byte[] deviceMacBytes = CommArithmetic.HexStringToByteArray(DeviceMacS);
            ByteBuf[ByteLen++] = deviceMacBytes[0];
            ByteBuf[ByteLen++] = deviceMacBytes[1];
            ByteBuf[ByteLen++] = deviceMacBytes[2];
            ByteBuf[ByteLen++] = deviceMacBytes[3];

            // New Sensor Mac
            deviceMacBytes = CommArithmetic.HexStringToByteArray(DeviceMacNewS);
            ByteBuf[ByteLen++] = deviceMacBytes[0];
            ByteBuf[ByteLen++] = deviceMacBytes[1];
            ByteBuf[ByteLen++] = deviceMacBytes[2];
            ByteBuf[ByteLen++] = deviceMacBytes[3];

            // Hardware Revision
            deviceMacBytes = CommArithmetic.HexStringToByteArray(HwRevisionS);
            ByteBuf[ByteLen++] = deviceMacBytes[0];
            ByteBuf[ByteLen++] = deviceMacBytes[1];
            ByteBuf[ByteLen++] = deviceMacBytes[2];
            ByteBuf[ByteLen++] = deviceMacBytes[3];

            // CRC
            UInt16 crc = MyCustomFxn.CRC16(0x1021, 0, ByteBuf, 2, (UInt16)(ByteLen - 2));
            ByteBuf[ByteLen++] = (byte)((crc & 0xFF00) >> 8);
            ByteBuf[ByteLen++] = (byte)((crc & 0x00FF) >> 0);

            // 结束位
            ByteBuf[ByteLen++] = 0xEC;

            return ByteBuf;
        }

        public byte[] UpdateUserConfig()
        {
            byte[] updateBytes = null;

            if (ProtocolVersion == 2)
            {
                updateBytes = new byte[27];
                updateBytes[0] = 0xCE;

                if (DeviceTypeS == "57")
                {   // M2 类型
                    updateBytes[1] = 0x14;
                }
                else
                {   // M1 所有类型
                    updateBytes[1] = 0x16;
                }

                updateBytes[2] = 0xA2;
                updateBytes[3] = 0x01;

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
                else if (DeviceTypeS == "5C")
                {
                    updateBytes[4] = 0x5C;
                }
                else if (DeviceTypeS == "5D")
                {
                    updateBytes[4] = 0x5D;
                }
                else if (DeviceTypeS == "7D")
                {
                    updateBytes[4] = 0x7D;
                }

                updateBytes[5] = 0x02;

                //Mac
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
                deviceMacBytes = CommArithmetic.HexStringToByteArray(DebugS);
                updateBytes[12] = deviceMacBytes[0];
                updateBytes[13] = deviceMacBytes[1];

                //category
                updateBytes[14] = Category;

                //Pattern
                updateBytes[15] = Pattern;

                //Bps
                updateBytes[16] = Bps;

                //TXPower
                updateBytes[17] = (byte)TxPower;

                //Frequency
                updateBytes[18] = Channel;

                //温度补偿
                Int16 tempComp = (Int16)Math.Round(TempCompensation * 100.0f);       // 单位：0.01℃
                updateBytes[19] = (byte)((tempComp & 0xFF00) >> 8);
                updateBytes[20] = (byte)((tempComp & 0x00FF) >> 0);

                if (DeviceTypeS == "57")
                {   // M2无湿度补偿

                    // 存储容量
                    updateBytes[21] = MaxLength;

                    //CRC
                    updateBytes[22] = 0x00;
                    updateBytes[23] = 0x00;

                    // 结束位
                    updateBytes[24] = 0xEC;
                }
                else
                {
                    //湿度补偿
                    UInt16 humComp = (UInt16)Math.Round(HumCompensation * 100.0f);     // 单位：0.01%
                    updateBytes[21] = (byte)((humComp & 0xFF00) >> 8);
                    updateBytes[22] = (byte)((humComp & 0x00FF) >> 0);

                    // 存储容量
                    updateBytes[23] = MaxLength;

                    //CRC
                    updateBytes[22] = 0x00;
                    updateBytes[23] = 0x00;

                    // 结束位
                    updateBytes[26] = 0xEC;
                }

            }          
            else if (ProtocolVersion == 3)
            {
                updateBytes = new byte[35];
                updateBytes[0] = 0xCE;
                updateBytes[1] = 0x1E;
                updateBytes[2] = 0xA2;
                updateBytes[3] = 0x02;

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
                else if (DeviceTypeS == "5C")
                {
                    updateBytes[4] = 0x5C;
                }
                else if (DeviceTypeS == "5D")
                {
                    updateBytes[4] = 0x5D;
                }
                else if (DeviceTypeS == "7D")
                {
                    updateBytes[4] = 0x7D;
                }

                updateBytes[5] = 0x03;

                //Mac
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
                deviceMacBytes = CommArithmetic.HexStringToByteArray(DebugS);
                updateBytes[12] = deviceMacBytes[0];
                updateBytes[13] = deviceMacBytes[1];

                //category
                updateBytes[14] = Category;
                //Pattern
                updateBytes[15] = Pattern;
                //Bps
                updateBytes[16] = Bps;
                //TXPower
                updateBytes[17] = (byte)TxPower;
                //Frequency
                updateBytes[18] = Channel;
                //MaxLength
                updateBytes[19] = MaxLength;
                //datetime
                deviceMacBytes = CommArithmetic.EncodeDateTime(Calendar);
                updateBytes[20] = deviceMacBytes[0];
                updateBytes[21] = deviceMacBytes[1];
                updateBytes[22] = deviceMacBytes[2];
                updateBytes[23] = deviceMacBytes[3];
                updateBytes[24] = deviceMacBytes[4];
                updateBytes[25] = deviceMacBytes[5];

                // 温度补偿
                Int16 tempComp = (Int16)Math.Round(TempCompensation * 100.0f);       // 单位：0.01℃
                updateBytes[26] = (byte)((tempComp & 0xFF00) >> 8);
                updateBytes[27] = (byte)((tempComp & 0x00FF) >> 0);

                // 湿度补偿
                UInt16 humComp = (UInt16)Math.Round(HumCompensation * 100.0f);     // 单位：0.01%
                updateBytes[28] = (byte)((humComp & 0xFF00) >> 8);
                updateBytes[29] = (byte)((humComp & 0x00FF) >> 0);

                // 保留位
                updateBytes[30] = 0x00;
                updateBytes[31] = 0x00;

                // CRC
                updateBytes[32] = 0x00;
                updateBytes[33] = 0x00;

                // 结束位
                updateBytes[34] = 0xEC;
            }

            return updateBytes;
        }

        public byte[] UpdateApplicationConfig()
        {
            byte[] ByteBuf = null;

            UInt16 ByteTotalLen = 0;
            UInt16 ByteLen = 0;

            byte usbProtocol = 0;

            if (ProtocolVersion == 2)
            {
                if (DeviceTypeS == "57")
                {   // M2
                    ByteTotalLen = 30;
                }
                else
                {   // M1
                    ByteTotalLen = 38;
                }

                usbProtocol = 0x01;
            }
            else if (ProtocolVersion == 3)
            {
                ByteTotalLen = 45;
                usbProtocol = 0x02;
            }
            else
            {
                return null;
            }

            // 创建字节数组
            ByteBuf = new byte[ByteTotalLen];

            // 起始位
            ByteBuf[ByteLen++] = 0xCE;

            // 长度位
            ByteBuf[ByteLen++] = (byte)(ByteTotalLen - 5);

            // 命令位
            ByteBuf[ByteLen++] = 0xA3;

            // USB协议版本
            ByteBuf[ByteLen++] = usbProtocol;

            // 设备类型
            if (DeviceTypeS == "51")
            {
                ByteBuf[ByteLen++] = 0x51;
            }
            else if (DeviceTypeS == "53")
            {
                ByteBuf[ByteLen++] = 0x53;
            }
            else if (DeviceTypeS == "57")
            {
                ByteBuf[ByteLen++] = 0x57;
            }
            else if (DeviceTypeS == "5C")
            {
                ByteBuf[ByteLen++] = 0x5C;
            }
            else if (DeviceTypeS == "5D")
            {
                ByteBuf[ByteLen++] = 0x5D;
            }
            else if (DeviceTypeS == "7D")
            {
                ByteBuf[ByteLen++] = 0x7D;
            }
            else
            {
                ByteBuf[ByteLen++] = 0x00;
            }

            // 协议版本
            ByteBuf[ByteLen++] = ProtocolVersion;

            // Sensor Mac
            byte[] deviceMacBytes = CommArithmetic.HexStringToByteArray(DeviceMacS);
            ByteBuf[ByteLen++] = deviceMacBytes[0];
            ByteBuf[ByteLen++] = deviceMacBytes[1];
            ByteBuf[ByteLen++] = deviceMacBytes[2];
            ByteBuf[ByteLen++] = deviceMacBytes[3];

            if (ProtocolVersion == 2)
            {
                //Interval
                deviceMacBytes = CommArithmetic.Int16_2Bytes(Interval);
                ByteBuf[ByteLen++] = deviceMacBytes[0];
                ByteBuf[ByteLen++] = deviceMacBytes[1];

                //Calendar
                deviceMacBytes = CommArithmetic.EncodeDateTime(Calendar);
                ByteBuf[ByteLen++] = deviceMacBytes[0];
                ByteBuf[ByteLen++] = deviceMacBytes[1];
                ByteBuf[ByteLen++] = deviceMacBytes[2];
                ByteBuf[ByteLen++] = deviceMacBytes[3];
                ByteBuf[ByteLen++] = deviceMacBytes[4];
                ByteBuf[ByteLen++] = deviceMacBytes[5];

                //采集发送倍数
                ByteBuf[ByteLen++] = SampleSend;

                if (DeviceTypeS == "57")
                {   // M2
                    // 温度预警上限
                    Int16 tempValue = (Int16)Math.Round(TempWarnHigh * 100.0f);
                    ByteBuf[ByteLen++] = (byte)((tempValue & 0xFF00) >> 8);
                    ByteBuf[ByteLen++] = (byte)((tempValue & 0x00FF) >> 0);

                    // 温度预警下限
                    tempValue = (Int16)Math.Round(TempWarnLow * 100.0f);
                    ByteBuf[ByteLen++] = (byte)((tempValue & 0xFF00) >> 8);
                    ByteBuf[ByteLen++] = (byte)((tempValue & 0x00FF) >> 0);

                    // 温度阈值上限
                    tempValue = (Int16)Math.Round(TempAlertHigh * 100.0f);
                    ByteBuf[ByteLen++] = (byte)((tempValue & 0xFF00) >> 8);
                    ByteBuf[ByteLen++] = (byte)((tempValue & 0x00FF) >> 0);

                    // 温度阈值下限
                    tempValue = (Int16)Math.Round(TempAlertLow * 100.0f);
                    ByteBuf[ByteLen++] = (byte)((tempValue & 0xFF00) >> 8);
                    ByteBuf[ByteLen++] = (byte)((tempValue & 0x00FF) >> 0);

                    // CRC 
                    UInt16 crc = MyCustomFxn.CRC16(0x1021, 0, ByteBuf, 2, (UInt16)(ByteLen - 2));
                    ByteBuf[ByteLen++] = (byte)((crc & 0xFF00) >> 8);
                    ByteBuf[ByteLen++] = (byte)((crc & 0x00FF) >> 0);

                    // 结束位
                    ByteBuf[ByteLen++] = 0xEC;
                }
                else
                {   // M1
                    // 温度预警上限
                    Int16 tempValue = (Int16)Math.Round(TempWarnHigh * 100.0f);
                    ByteBuf[ByteLen++] = (byte)((tempValue & 0xFF00) >> 8);
                    ByteBuf[ByteLen++] = (byte)((tempValue & 0x00FF) >> 0);

                    // 温度预警下限
                    tempValue = (Int16)Math.Round(TempWarnLow * 100.0f);
                    ByteBuf[ByteLen++] = (byte)((tempValue & 0xFF00) >> 8);
                    ByteBuf[ByteLen++] = (byte)((tempValue & 0x00FF) >> 0);

                    // 湿度预警上限
                    UInt16 humValue = (UInt16)Math.Round(HumWarnHigh * 100.0f);
                    ByteBuf[ByteLen++] = (byte)((humValue & 0xFF00) >> 8);
                    ByteBuf[ByteLen++] = (byte)((humValue & 0x00FF) >> 0);

                    // 湿度预警下限
                    humValue = (UInt16)Math.Round(HumWarnLow * 100.0f);
                    ByteBuf[ByteLen++] = (byte)((humValue & 0xFF00) >> 8);
                    ByteBuf[ByteLen++] = (byte)((humValue & 0x00FF) >> 0);

                    // 温度阈值上限
                    tempValue = (Int16)Math.Round(TempAlertHigh * 100.0f);
                    ByteBuf[ByteLen++] = (byte)((tempValue & 0xFF00) >> 8);
                    ByteBuf[ByteLen++] = (byte)((tempValue & 0x00FF) >> 0);

                    // 温度阈值下限
                    tempValue = (Int16)Math.Round(TempAlertLow * 100.0f);
                    ByteBuf[ByteLen++] = (byte)((tempValue & 0xFF00) >> 8);
                    ByteBuf[ByteLen++] = (byte)((tempValue & 0x00FF) >> 0);

                    // 湿度阈值上限
                    humValue = (UInt16)Math.Round(HumAlertHigh * 100.0f);
                    ByteBuf[ByteLen++] = (byte)((humValue & 0xFF00) >> 8);
                    ByteBuf[ByteLen++] = (byte)((humValue & 0x00FF) >> 0);

                    // 湿度阈值下限
                    humValue = (UInt16)Math.Round(HumAlertLow * 100.0f);
                    ByteBuf[ByteLen++] = (byte)((humValue & 0xFF00) >> 8);
                    ByteBuf[ByteLen++] = (byte)((humValue & 0x00FF) >> 0);

                    // CRC 
                    UInt16 crc = MyCustomFxn.CRC16(0x1021, 0, ByteBuf, 2, (UInt16)(ByteLen - 2));
                    ByteBuf[ByteLen++] = (byte)((crc & 0xFF00) >> 8);
                    ByteBuf[ByteLen++] = (byte)((crc & 0x00FF) >> 0);

                    // 结束位
                    ByteBuf[ByteLen++] = 0xEC;
                }
            }
            else if (ProtocolVersion == 3)
            {
                // 日期和时间
                deviceMacBytes = CommArithmetic.EncodeDateTime(Calendar);
                ByteBuf[ByteLen++] = deviceMacBytes[0];
                ByteBuf[ByteLen++] = deviceMacBytes[1];
                ByteBuf[ByteLen++] = deviceMacBytes[2];
                ByteBuf[ByteLen++] = deviceMacBytes[3];
                ByteBuf[ByteLen++] = deviceMacBytes[4];
                ByteBuf[ByteLen++] = deviceMacBytes[5];

                // 采集间隔
                ByteBuf[ByteLen++] = (byte)((Interval & 0xFF00) >> 8);
                ByteBuf[ByteLen++] = (byte)((Interval & 0x00FF) >> 0);

                // 正常传输间隔
                ByteBuf[ByteLen++] = (byte)((NormalInterval & 0xFF00) >> 8);
                ByteBuf[ByteLen++] = (byte)((NormalInterval & 0x00FF) >> 0);

                // 预警传输间隔
                ByteBuf[ByteLen++] = (byte)((WarnInterval & 0xFF00) >> 8);
                ByteBuf[ByteLen++] = (byte)((WarnInterval & 0x00FF) >> 0);

                // 报警传输间隔
                ByteBuf[ByteLen++] = (byte)((AlertInterval & 0xFF00) >> 8);
                ByteBuf[ByteLen++] = (byte)((AlertInterval & 0x00FF) >> 0);

                // 温度预警上限
                Int16 tempValue = (Int16)Math.Round(TempWarnHigh * 100.0f);
                ByteBuf[ByteLen++] = (byte)((tempValue & 0xFF00) >> 8);
                ByteBuf[ByteLen++] = (byte)((tempValue & 0x00FF) >> 0);

                // 温度预警下限
                tempValue = (Int16)Math.Round(TempWarnLow * 100.0f);
                ByteBuf[ByteLen++] = (byte)((tempValue & 0xFF00) >> 8);
                ByteBuf[ByteLen++] = (byte)((tempValue & 0x00FF) >> 0);

                // 湿度预警上限
                UInt16 humValue = (UInt16)Math.Round(HumWarnHigh * 100.0f);
                ByteBuf[ByteLen++] = (byte)((humValue & 0xFF00) >> 8);
                ByteBuf[ByteLen++] = (byte)((humValue & 0x00FF) >> 0);

                // 湿度预警下限
                humValue = (UInt16)Math.Round(HumWarnLow * 100.0f);
                ByteBuf[ByteLen++] = (byte)((humValue & 0xFF00) >> 8);
                ByteBuf[ByteLen++] = (byte)((humValue & 0x00FF) >> 0);

                // 温度阈值上限
                tempValue = (Int16)Math.Round(TempAlertHigh * 100.0f);
                ByteBuf[ByteLen++] = (byte)((tempValue & 0xFF00) >> 8);
                ByteBuf[ByteLen++] = (byte)((tempValue & 0x00FF) >> 0);

                // 温度阈值下限
                tempValue = (Int16)Math.Round(TempAlertLow * 100.0f);
                ByteBuf[ByteLen++] = (byte)((tempValue & 0xFF00) >> 8);
                ByteBuf[ByteLen++] = (byte)((tempValue & 0x00FF) >> 0);

                // 湿度阈值上限
                humValue = (UInt16)Math.Round(HumAlertHigh * 100.0f);
                ByteBuf[ByteLen++] = (byte)((humValue & 0xFF00) >> 8);
                ByteBuf[ByteLen++] = (byte)((humValue & 0x00FF) >> 0);

                // 湿度阈值下限
                humValue = (UInt16)Math.Round(HumAlertLow * 100.0f);
                ByteBuf[ByteLen++] = (byte)((humValue & 0xFF00) >> 8);
                ByteBuf[ByteLen++] = (byte)((humValue & 0x00FF) >> 0);

                // 保留位
                ByteBuf[ByteLen++] = 0x00;
                ByteBuf[ByteLen++] = 0x00;

                // CRC 
                UInt16 crc = MyCustomFxn.CRC16(0x1021, 0, ByteBuf, 2, (UInt16)(ByteLen - 2));
                ByteBuf[ByteLen++] = (byte)((crc & 0xFF00) >> 8);
                ByteBuf[ByteLen++] = (byte)((crc & 0x00FF) >> 0);

                // 结束位
                ByteBuf[ByteLen++] = 0xEC;
            }

            return ByteBuf;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] DeleteData()
        {
            byte[] ByteBuf = new byte[19];
            UInt16 ByteLen = 0;

            // 起始位
            ByteBuf[ByteLen++] = 0xCE;

            // 长度位
            ByteBuf[ByteLen++] = (byte)(ByteBuf.Length - 5);

            // 命令位
            ByteBuf[ByteLen++] = 0xA4;

            // USB Protocol
            ByteBuf[ByteLen++] = 0x01;

            // 设备类型
            if (DeviceTypeS == "51")
            {
                ByteBuf[ByteLen++] = 0x51;
            }
            else if (DeviceTypeS == "53")
            {
                ByteBuf[ByteLen++] = 0x53;
            }
            else if (DeviceTypeS == "57")
            {
                ByteBuf[ByteLen++] = 0x57;
            }
            else if (DeviceTypeS == "5C")
            {
                ByteBuf[ByteLen++] = 0x5C;
            }
            else if (DeviceTypeS == "5D")
            {
                ByteBuf[ByteLen++] = 0x5D;
            }
            else if (DeviceTypeS == "7D")
            {
                ByteBuf[ByteLen++] = 0x7D;
            }
            else
            {
                ByteBuf[ByteLen++] = 0x00;
            }

            // Protocol
            ByteBuf[ByteLen++] = 0x02;

            // Sensor Mac
            byte[] deviceMacBytes = CommArithmetic.HexStringToByteArray(DeviceMacS);
            ByteBuf[ByteLen++] = deviceMacBytes[0];
            ByteBuf[ByteLen++] = deviceMacBytes[1];
            ByteBuf[ByteLen++] = deviceMacBytes[2];
            ByteBuf[ByteLen++] = deviceMacBytes[3];

            // Front
            ByteBuf[ByteLen++] = 0x00;
            ByteBuf[ByteLen++] = 0x00;
            ByteBuf[ByteLen++] = 0x00;

            // Rear
            ByteBuf[ByteLen++] = 0x00;
            ByteBuf[ByteLen++] = 0x00;
            ByteBuf[ByteLen++] = 0x00;

            // CRC 
            UInt16 crc = MyCustomFxn.CRC16(0x1021, 0, ByteBuf, 2, (UInt16)(ByteLen - 2));
            ByteBuf[ByteLen++] = (byte)((crc & 0xFF00) >> 8);
            ByteBuf[ByteLen++] = (byte)((crc & 0x00FF) >> 0);

            // 结束位
            ByteBuf[ByteLen++] = 0xEC;

            return ByteBuf;
        }
    }
}
